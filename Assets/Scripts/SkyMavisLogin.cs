using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SkyMavisLogin : MonoBehaviour
{
    private string clientId = "526fa16c-b86d-4900-b510-88124de910f0";
    private string clientSecret = "qKIFvCTTEYz52C0BuSZq3yXl4bo967Xx";
    private string redirectUri = "http://localhost:3000/login/callback";
    private string authorizationEndpoint = "https://api-gateway.skymavis.com/oauth2/auth";
    private string tokenEndpoint = "https://api-gateway.skymavis.com/account/oauth2/token";
    private string userInfoEndpoint = "https://api-gateway.skymavis.com/account/userinfo";
    private string apiKey = "LIsOiTwGSx2o5awaJQAfhCy4XDaKcvCu";
    private string appId = "526fa16c-b86d-4900-b510-88124de910f0";
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
        loginButton.onClick.AddListener(OnLoginButtonClicked);

        httpListener = new HttpListener();
        httpListener.Prefixes.Add("http://localhost:3000/");
        httpListener.Start();
        ThreadPool.QueueUserWorkItem(StartHttpListener);
    }

    private void OnLoginButtonClicked()
    {
        
        //GETAUTHURL
        //SERVER
        codeVerifier = GenerateCodeVerifier();
        string codeChallenge = GenerateCodeChallenge(codeVerifier);

        string authorizationUrl =
            $"{authorizationEndpoint}?client_id={clientId}&redirect_uri={Uri.EscapeDataString(redirectUri)}&response_type=code&scope=openid&state={GenerateRandomString(32)}&code_challenge={codeChallenge}&code_challenge_method=S256";
       //
       
       
       //CIENT
        Application.OpenURL(authorizationUrl);
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
                
                //ENVIAR AUTH CODE TO BACKEND.
                
                //SERVER
                actions.Enqueue(() => StartCoroutine(HandleAuthorizationResponse(authorizationCode)));
                //SERVER
                
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

    private IEnumerator HandleAuthorizationResponse(string authorizationCode)
    {
        WWWForm form = new WWWForm();
        form.AddField("grant_type", "authorization_code");
        form.AddField("code", authorizationCode);
        form.AddField("redirect_uri", redirectUri);
        form.AddField("client_id", clientId);
        form.AddField("client_secret", clientSecret);
        form.AddField("code_verifier", codeVerifier);

        UnityWebRequest tokenRequest = UnityWebRequest.Post(tokenEndpoint, form);
        tokenRequest.SetRequestHeader("X-API-Key", apiKey);
        tokenRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded");

        yield return tokenRequest.SendWebRequest();

        if (tokenRequest.result != UnityWebRequest.Result.Success)
        {
            resultText.text = "Token request failed: " + tokenRequest.error;
            Debug.LogError("Token request failed: " + tokenRequest.error);
            Debug.LogError("Token request response: " + tokenRequest.downloadHandler.text);
            yield break;
        }

        string responseText = tokenRequest.downloadHandler.text;
        Debug.Log("Token response: " + responseText);
        var json = JsonUtility.FromJson<OAuthTokenResponse>(responseText);

        if (json != null && !string.IsNullOrEmpty(json.access_token))
        {
            accessToken = json.access_token;
            resultText.text = "Login successful! Token: " + json.access_token;
            StartCoroutine(GetUserInfo());
        }
        else
        {
            resultText.text = "Failed to obtain token.";
            Debug.LogError("Failed to obtain token. Response: " + responseText);
        }
    }

    private IEnumerator GetUserInfo()
    {
        UnityWebRequest request = UnityWebRequest.Get(userInfoEndpoint);
        request.SetRequestHeader("X-Api-Key", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + accessToken);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("User info: " + request.downloadHandler.text);
            resultText.text = "User info: " + request.downloadHandler.text;

            // Parse the JSON to extract wallet addresses
            var userInfo = JsonUtility.FromJson<UserInfoResponse>(request.downloadHandler.text);
            userWallet = userInfo.addr;
            Debug.Log("User wallet address: " + userWallet);
            accountManager.wallet = userWallet;
            accountManager.LoginAccount();
        }
        else
        {
            Debug.LogError("Error fetching user info: " + request.error);
            resultText.text = "Error fetching user info: " + request.error;
        }
    }

    private string GenerateCodeVerifier()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-._~";
        var random = new System.Random();
        var verifier = new char[128];
        for (int i = 0; i < verifier.Length; i++)
        {
            verifier[i] = chars[random.Next(chars.Length)];
        }

        return new string(verifier);
    }

    private string GenerateCodeChallenge(string codeVerifier)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Convert.ToBase64String(challengeBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }

    private string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new System.Random();
        var result = new char[length];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = chars[random.Next(chars.Length)];
        }

        return new string(result);
    }

    [System.Serializable]
    private class OAuthTokenResponse
    {
        public string access_token;
    }

    [System.Serializable]
    private class UserInfoResponse
    {
        public string addr;
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