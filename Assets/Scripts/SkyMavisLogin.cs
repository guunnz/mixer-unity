using AxieCore.SimpleJSON;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Numerics;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SkyMavisLogin : MonoBehaviour
{
    private string redirectUri = "http://localhost:3000/login/callback";
    private string authorizationEndpoint = "http://34.138.180.95/api/v1/auth/url";
    private string userInfoEndpoint = "http://34.138.180.95/api/v1/auth/login";
    private string refreshUserInfoEndpoint = "http://34.138.180.95/api/v1/auth/refresh";
    private string NFTsUserInfoEndpoint = "http://34.138.180.95/api/v1/user/nfts";

    private AuthToken authToken;

    public Button loginButton;
    public Text resultText;
    public AccountManager accountManager;
    private HttpListener httpListener;
    private string codeVerifier;
    private string accessToken;
    private string userWallet;
    private ConcurrentQueue<Action> actions = new ConcurrentQueue<Action>();

    private void Start()
    {
        PartFinder.LoadFromResources();
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
                StartCoroutine(GetNFTS());
            }
        }
    }

    private void OnLoginButtonClicked()
    {
        StartCoroutine(LogIn());
    }

    public IEnumerator LogIn(int retries = 5)
    {
        httpListener = new HttpListener();
        httpListener.Prefixes.Add("http://localhost:3000/");
        httpListener.Start();
        ThreadPool.QueueUserWorkItem(StartHttpListener);

        using (UnityWebRequest www = new UnityWebRequest(authorizationEndpoint, "GET"))
        {
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            www.downloadHandler = dH;
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.Send();

            if (www.isNetworkError)
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
                Debug.Log(www.ToString());
                Debug.Log(www.downloadHandler.text);
                string authorizationUrl = JsonUtility.FromJson<AuthorizationJSON>(www.downloadHandler.text).authUrl;
                Application.OpenURL(authorizationUrl);
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

    private void Update()
    {
        while (actions.TryDequeue(out var action))
        {
            action.Invoke();
        }
    }

    private IEnumerator HandleAuthorizationResponse(string authorizationCode, int retries = 5)
    {
        UnityWebRequest webRequest = new UnityWebRequest(userInfoEndpoint, "POST");
        webRequest.SetRequestHeader("auth_code", authorizationCode);
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

    private IEnumerator GetNFTS(int retries = 5)
    {
        UnityWebRequest webRequest = new UnityWebRequest(NFTsUserInfoEndpoint, "GET");
        webRequest.SetRequestHeader("access_token", authToken.AccessToken);
        DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
        webRequest.downloadHandler = dH;
        yield return webRequest.SendWebRequest();

        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.LogError(webRequest.error);
            if (retries > 0)
            {
                Debug.Log("Retrying POST request. Attempts remaining: " + (retries - 1));
                StartCoroutine(GetNFTS(retries - 1));
            }
        }

        string userInfo = webRequest.downloadHandler.text;

        if (!string.IsNullOrEmpty(userInfo))
        {
            GetUserInfo(userInfo);
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
                Debug.Log("Retrying POST request. Attempts remaining: " + (retries - 1));
                StartCoroutine(RefreshToken(retries - 1));
            }
        }

        authToken = JsonUtility.FromJson<AuthToken>(webRequest.downloadHandler.text);

        if (getNfts)
        {
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
            // Check if the current UTC time is greater than the issue time plus the seconds valid.
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
        public NftsResponse nftsResponse;
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