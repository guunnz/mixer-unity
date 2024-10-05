using AxieCore.SimpleJSON;
using Newtonsoft.Json;
using SimpleGraphQL.GraphQLParser;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using static GetAxiesExample;

public class SkyMavisLogin : MonoBehaviour
{
    private string redirectUri = "http://localhost:3000/login/callback/"; // Used only for Desktop
    private string authorizationEndpoint = "https://www.axielandbattles.com/api/v1/auth/url";
    private string userInfoEndpoint = "https://www.axielandbattles.com/api/v1/auth/login";
    private string refreshUserInfoEndpoint = "https://www.axielandbattles.com/api/v1/auth/refresh";
    private string NFTsUserInfoEndpoint = "https://www.axielandbattles.com/api/v1/user/nfts";
    private string buildVersion = "https://melodic-voice-423218-s4.ue.r.appspot.com/api/v1/unity/buildversion";
    public AuthToken authToken;
    public Button loginButton;
    public Text resultText;
    public AccountManager accountManager;
    private HttpListener httpListener;
    private string codeVerifier;
    private string accessToken;
    private string userWallet;
    private ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();
    private bool version = false;
    public VideoPlayer introVideoPlayer;
    public VideoPlayer loopedVideoPlayer;
    public VideoClip FirstTimeIntroVideoclip;
    public VideoClip SecondTimeIntroVideoclip;
    public AudioSource mainMenuSong;

    public GameObject cursor;
    public void LogOut()
    {
        PlayerPrefs.SetString("Auth", "");
        SceneManager.LoadScene(0);
        Loading.instance.DisableLoading();
    }


    private IEnumerator Start()
    {
#if UNITY_ANDROID || UNITY_IOS

        Application.targetFrameRate = 30;

#endif
        cursor.SetActive(false);

        if (!Loading.instance.IsLoadingEnabled())
        {
            if (!string.IsNullOrEmpty(PlayerPrefs.GetString("Auth")))
            {
                introVideoPlayer.clip = SecondTimeIntroVideoclip;
                introVideoPlayer.Play();
                yield return new WaitForSeconds(2.02f);
                loopedVideoPlayer.targetCameraAlpha = 1;
                loopedVideoPlayer.Play();
                yield return new WaitForSeconds(0.1f);
        
                introVideoPlayer.targetCameraAlpha = 0;
                introVideoPlayer.Stop();
                yield return new WaitForSeconds(1f);
            }
            else
            {
                introVideoPlayer.clip = FirstTimeIntroVideoclip;
                introVideoPlayer.Play();
                yield return new WaitForSeconds(4.06f);
                loopedVideoPlayer.targetCameraAlpha = 1;
                loopedVideoPlayer.Play();
                yield return new WaitForSeconds(0.1f);
                introVideoPlayer.targetCameraAlpha = 0;
                introVideoPlayer.Stop();
            

            }
        }
        cursor.SetActive(true);
        StartCoroutine(CheckVersion());

        while (!version)
        {
            yield return null;
        }
        string token = GetTokenFromCommandLineArgs();

        if (!string.IsNullOrEmpty(token))
        {
            if (authToken.IsExpired())
            {
                StartCoroutine(RefreshToken(3, true));
            }
            Loading.instance.EnableLoading();
            mainMenuSong.enabled = true;
            introVideoPlayer.Stop();
            authToken = new AuthToken { AccessToken = token };
            StartCoroutine(GetNFTS());  // Proceed directly with NFT fetching if token is valid
        }
        else
        {
            PartFinder.LoadFromResources();
            loginButton.gameObject.SetActive(true);
            loginButton.onClick.AddListener(OnLoginButtonClicked);
            string auth = PlayerPrefs.GetString("Auth");

            if (!string.IsNullOrEmpty(auth))
            {
                authToken = JsonConvert.DeserializeObject<AuthToken>(auth);

                if (authToken.IsExpired())
                {
                    StartCoroutine(RefreshToken(3, true));
                }
                else
                {
                    Loading.instance.EnableLoading();
                    mainMenuSong.enabled = true;
                    introVideoPlayer.Stop();
                    StartCoroutine(GetNFTS());
                }
            }
        }

        // Register deep link handler ONLY for mobile platforms
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Application.deepLinkActivated += HandleDeepLink;
        }
        yield return null;
    }

    private void OnLoginButtonClicked()
    {
#if UNITY_STANDALONE || UNITY_WEBGL
        if (httpListener != null && httpListener.IsListening)
            httpListener.Stop();
#endif
        StartCoroutine(LogIn());
    }

    public IEnumerator CheckVersion(int retries = 5)
    {
        using (UnityWebRequest www = new UnityWebRequest(buildVersion, "GET"))
        {
            Debug.Log("REQ LINK: " + buildVersion);
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            www.downloadHandler = dH;
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
                if (retries > 0)
                {
                    Debug.Log("Retrying POST request. Attempts remaining: " + (retries - 1));
                    StartCoroutine(CheckVersion(retries - 1));
                }
            }
            else
            {
                LoginClass login = JsonUtility.FromJson<LoginClass>(www.downloadHandler.text);

                if (IsCurrentVersionHigher(login.version))
                {
                    SceneManager.LoadScene(1);
                }
                else
                {
                    version = true;
                }
            }
        }
    }

    // Method to compare Application.version with a specified version string
    public bool IsCurrentVersionHigher(string versionToCheck)
    {
        // Parse the Application's version and the versionToCheck
        string currentVersion = Application.version;

        // Compare versions
        return CompareVersions(versionToCheck, currentVersion) > 0;
    }

    // Method to compare two version strings
    private int CompareVersions(string version1, string version2)
    {
        // Split by periods and separate numeric parts and suffixes
        string[] parts1 = version1.Split('.');
        string[] parts2 = version2.Split('.');

        // Compare the first three numeric parts
        for (int i = 0; i < 3; i++)
        {
            if (i >= parts1.Length || i >= parts2.Length)
                return parts1.Length.CompareTo(parts2.Length); // If one version has fewer parts, it is lower

            int num1 = ExtractNumber(parts1[i]);
            int num2 = ExtractNumber(parts2[i]);

            if (num1 < num2) return -1;
            if (num1 > num2) return 1;
        }

        // If numeric parts are equal, compare the alphanumeric suffixes (e.g., "cb")
        string suffix1 = ExtractSuffix(parts1[2]);
        string suffix2 = ExtractSuffix(parts2[2]);

        // Compare suffixes if present
        if (suffix1 == suffix2) return 0;
        if (string.IsNullOrEmpty(suffix1)) return -1; // No suffix means lower version
        if (string.IsNullOrEmpty(suffix2)) return 1;

        return string.Compare(suffix1, suffix2, System.StringComparison.Ordinal);
    }

    // Extract the numeric portion from a version string part
    private int ExtractNumber(string part)
    {
        for (int i = 0; i < part.Length; i++)
        {
            if (!char.IsDigit(part[i]))
                return int.Parse(part.Substring(0, i));
        }
        return int.Parse(part);
    }

    // Extract the alphanumeric suffix from a version string part
    private string ExtractSuffix(string part)
    {
        for (int i = 0; i < part.Length; i++)
        {
            if (!char.IsDigit(part[i]))
                return part.Substring(i);
        }
        return null;
    }

    [System.Serializable]
    public class LoginClass
    {
        public string id;
        public string version;
    }

    public IEnumerator LogIn(int retries = 5)
    {
        using (UnityWebRequest www = new UnityWebRequest(authorizationEndpoint, "GET"))
        {
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            www.downloadHandler = dH;
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
                if (retries > 0)
                {
                    Debug.Log("Retrying POST request. Attempts remaining: " + (retries - 1));
                    StartCoroutine(LogIn(retries - 1));
                }
            }
            else
            {
                string authorizationUrl = JsonUtility.FromJson<AuthorizationJSON>(www.downloadHandler.text).authUrl;

                // Use platform-specific code to handle login
#if UNITY_ANDROID || UNITY_IOS
                // Mobile-specific login with deep link
                Debug.Log($"Mobile login with deep link: {authorizationUrl}");
                OpenAuthorizationUrlForMobile(authorizationUrl);
#else
                // Desktop-specific login with localhost callback
                Debug.Log($"Desktop login with redirect URI: {redirectUri}");
                httpListener = new HttpListener();
                httpListener.Prefixes.Add(redirectUri);  // Ensure redirectUri ends with '/'
                httpListener.Start();
                ThreadPool.QueueUserWorkItem(StartHttpListener);
                Application.OpenURL(authorizationUrl);  // Open in desktop browser
#endif
            }
        }
    }



    private void OpenAuthorizationUrlForMobile(string authorizationUrl)
    {
        // Parse the existing authorization URL
        UriBuilder uriBuilder = new UriBuilder(authorizationUrl);
        var query = System.Web.HttpUtility.ParseQueryString(uriBuilder.Query);

        // Replace the redirect_uri parameter with your mobile deep link
        query.Set("redirect_uri", "axielandbattles://auth");

        // Add the response_mode parameter to use fragment
        query.Set("response_mode", "fragment");

        // Update the query in the UriBuilder
        uriBuilder.Query = query.ToString();

        // Get the modified authorization URL
        string modifiedAuthorizationUrl = uriBuilder.ToString();

        Debug.Log($"Opening URL on mobile: {modifiedAuthorizationUrl}");

        // Open the modified URL
        Application.OpenURL(modifiedAuthorizationUrl);
    }



    private void HandleDeepLink(string url)
    {
        Debug.Log("DeepLink activated: " + url);
        if (url.StartsWith("axielandbattles://auth"))
        {
            Uri uri = new Uri(url);

            // Get the fragment (after the '#')
            string fragment = uri.Fragment;

            // Remove the leading '#' if present
            if (fragment.StartsWith("#"))
            {
                fragment = fragment.Substring(1);
            }

            // Parse the fragment as a query string
            var queryParams = System.Web.HttpUtility.ParseQueryString(fragment);

            // Extract parameters
            string authorizationCode = queryParams.Get("code");
            string scope = queryParams.Get("scope");
            string state = queryParams.Get("state");

            Debug.Log("Authorization Code: " + authorizationCode);
            Debug.Log("Scope: " + scope);
            Debug.Log("State: " + state);

            if (!string.IsNullOrEmpty(authorizationCode))
            {
                StartCoroutine(HandleAuthorizationResponse(authorizationCode));
            }
            else
            {
                Debug.LogWarning("Authorization code not found in the deep link.");
            }
        }
    }



    public struct AuthorizationJSON
    {
        public string authUrl;
    }

    private void OnDisable()
    {
        if (httpListener != null)
            httpListener.Stop();
    }

    private void StartHttpListener(object state)
    {
        while (httpListener.IsListening)
        {
            HttpListenerContext context = httpListener.GetContext();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            if (request.Url.AbsolutePath == "/login/callback" && request.QueryString["code"] != null)
            {
                string authorizationCode = request.QueryString["code"];
                Debug.Log("Authorization code received: " + authorizationCode);

                actions.Enqueue(() => StartCoroutine(HandleAuthorizationResponse(authorizationCode)));

                string responseString = "<html><body>Login successful! You can close this window.</body></html>";
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                System.IO.Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                output.Close();

                httpListener.Stop();
            }
            else
            {
                Debug.LogWarning("Authorization code not found in the request.");
            }
        }
    }
#if UNITY_STANDALONE || UNITY_WEBGL
    private void Update()
    {

        while (actions.TryDequeue(out var action))
        {
            action.Invoke();
        }

    }
#endif
    private IEnumerator HandleAuthorizationResponse(string authorizationCode, int retries = 5)
    {
        Debug.Log("AUTHORIZATION CODE: " + authorizationCode);
        UnityWebRequest webRequest = new UnityWebRequest(userInfoEndpoint, "POST");
        webRequest.SetRequestHeader("auth-code", authorizationCode);

#if UNITY_ANDROID || UNITY_IOS
        webRequest.SetRequestHeader("mobile", "true");
#endif
        DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
        webRequest.downloadHandler = dH;
        yield return webRequest.SendWebRequest();

        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.LogError(webRequest.error);
            if (retries > 0)
            {
                Debug.Log("Retrying POST request. Attempts remaining: " + (retries - 1));
                StartCoroutine(LogIn(retries - 1));
            }
        }

        authToken = JsonConvert.DeserializeObject<AuthToken>(webRequest.downloadHandler.text);
        PlayerPrefs.SetString("Auth", webRequest.downloadHandler.text);
        StartCoroutine(GetNFTS());
    }

    private IEnumerator GetNFTS(int retries = 5, int page = 0)
    {
        UnityWebRequest webRequest = new UnityWebRequest(NFTsUserInfoEndpoint + "?page=" + page.ToString(), "GET");
        webRequest.SetRequestHeader("access-token", authToken.AccessToken);
        DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
        webRequest.downloadHandler = dH;
        yield return webRequest.SendWebRequest();

        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.LogError(webRequest.error);
            if (retries > 0)
            {
                Debug.Log("Retrying GET request. Attempts remaining: " + (retries - 1) + " GetNFTS");
                StartCoroutine(GetNFTS(retries - 1, page));
            }
            else
            {
                PlayerPrefs.SetString("Auth", "");

                SceneManager.LoadScene(0);
                Loading.instance.DisableLoading();
            }
        }

        string userInfo = webRequest.downloadHandler.text;

        if (!string.IsNullOrEmpty(userInfo))
        {
            if (page > 0)
            {
                AddNFTs(userInfo);

                SkyMavisLogin.Root userInfoObj = JsonUtility.FromJson<SkyMavisLogin.Root>(userInfo);

                if ((userInfoObj.axies.result.paging.total - page * 100) >= userInfoObj.axies.result.items.Count || (userInfoObj.lands.result.paging.total - page * 100) >= userInfoObj.lands.result.items.Count)
                {
                    Debug.Log("LOOKING FOR NEW PAGE: " + (page + 1));
                    StartCoroutine(GetNFTS(5, page + 1));
                }
            }
            else
            {
                GetUserInfo(userInfo);
                SkyMavisLogin.Root userInfoObj = JsonUtility.FromJson<SkyMavisLogin.Root>(userInfo);
                MavisTracking.Instance.InitializeTracking(userInfoObj.userInfo);

                Debug.Log("TOTAL NUMBERS: " + userInfoObj.axies.result.paging.total.ToString());
                Debug.Log("TOTAL AXIES IN PAGE: " + userInfoObj.axies.result.items.Count.ToString());

                if ((userInfoObj.axies.result.paging.total - page * 100) >= userInfoObj.axies.result.items.Count || (userInfoObj.lands.result.paging.total - page * 100) >= userInfoObj.lands.result.items.Count)
                {
                    Debug.Log("LOOKING FOR NEW PAGE: " + (page + 1));
                    StartCoroutine(GetNFTS(5, page + 1));
                }
            }
        }
        else
        {
            Debug.LogError("Login Failed" + " " + webRequest.downloadHandler.text);
        }
    }

    public IEnumerator RefreshToken(int retries = 3, bool getNfts = false)
    {
        UnityWebRequest webRequest = new UnityWebRequest(refreshUserInfoEndpoint, "POST");
        webRequest.SetRequestHeader("refresh_token", authToken.RefreshToken);
        DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
        webRequest.downloadHandler = dH;
        yield return webRequest.SendWebRequest();

        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.LogError(webRequest.error);
            if (retries > 0)
            {
                Debug.Log("Retrying POST request. Attempts remaining: " + (retries - 1) + " Refresh token");
                StartCoroutine(RefreshToken(retries - 1));
            }
            else
            {
                PlayerPrefs.SetString("Auth", "");
                SceneManager.LoadScene(0);
                Loading.instance.DisableLoading();
            }
        }

        authToken = JsonConvert.DeserializeObject<AuthToken>(webRequest.downloadHandler.text);

        if (getNfts)
        {
            Loading.instance.EnableLoading();
            mainMenuSong.enabled = true;
            introVideoPlayer.Stop();
            StartCoroutine(GetNFTS());
        }
    }

    [System.Serializable]
    public class AuthToken
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("id_token")]
        public string IdToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        private DateTime issueTime;

        public AuthToken()
        {
            this.issueTime = DateTime.UtcNow;
        }

        public bool IsExpired()
        {
            return DateTime.UtcNow >= issueTime.AddSeconds(ExpiresIn);
        }
    }

    private void GetUserInfo(string userInfoString)
    {
        if (accountManager != null)
        {
            accountManager.LoginAccount(userInfoString);
        }
    }

    private void AddNFTs(string userInfoString)
    {
        if (accountManager != null)
        {
            accountManager.AddNFTs(userInfoString);
        }
    }

    [System.Serializable]
    public struct UserInfo
    {
        public string addr;
        public string email;
        public string name;
        public string roninAddress;
    }

    [System.Serializable]
    public struct RawMetadata
    {
        public string external_url;
        public string genes;
        public long id;
        public string image;
        public string name;
        public Properties properties;
        public string title;
    }

    [System.Serializable]
    public struct Properties
    {
        public long axie_id;
        public string back_id;
        public long birthdate;
        public string bodyshape;
        public int breed_count;
        public string @class;
        public string ears_id;
        public string eyes_id;
        public string horn_id;
        public long matron_id;
        public string mouth_id;
        public int num_japan;
        public int num_mystic;
        public int num_shiny;
        public int num_summer;
        public int num_xmas;
        public string primary_color;
        public int pureness;
        public int purity;
        public long sire_id;
        public int stage;
        public string tail_id;
        public string col;
        public string row;
        public string land_type;
    }

    [System.Serializable]
    public struct Item
    {
        public string contractAddress;
        public bool f2p;
        public long createdAtBlock;
        public string createdAtBlockTime;
        public RawMetadata rawMetadata;
        public string tokenId;
        public string tokenName;
        public string tokenStandard;
        public string tokenSymbol;
        public string tokenURI;
        public long updatedAtBlock;
        public string updatedAtBlockTime;
    }

    [System.Serializable]
    public struct Result
    {
        public List<Item> items;
        public Paging paging;
    }

    [System.Serializable]
    public struct Paging
    {
        public int total;
    }

    [System.Serializable]
    public struct NftsResponse
    {
        public Result result;
    }

    [System.Serializable]
    public struct Root
    {
        public UserInfo userInfo;
        public NftsResponse lands;
        public NftsResponse axies;
    }

    private string GetTokenFromCommandLineArgs()
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-token" && i + 1 < args.Length)
            {
                return args[i + 1];
            }
        }
        return null;
    }

    private void OnDestroy()
    {
        if (httpListener != null && httpListener.IsListening)
        {
            httpListener.Stop();
            httpListener.Close();
        }
    }
}
