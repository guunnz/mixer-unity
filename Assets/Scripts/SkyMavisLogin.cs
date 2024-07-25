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
    private string authorizationEndpoint = "http://34.73.111.101/api/v1/auth";
    private string userInfoEndpoint = "http://34.73.111.101/api/v1/user/nfts";

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
        StartCoroutine(LogIn());
    }

    public IEnumerator LogIn(int retries = 5)
    {
        UnityWebRequest webRequest = new UnityWebRequest(authorizationEndpoint, "GET");

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

        string authorizationUrl = webRequest.downloadHandler.text.Replace("\"", "");
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

    private IEnumerator HandleAuthorizationResponse(string authorizationCode, int retries = 5)
    {
        UnityWebRequest webRequest = new UnityWebRequest(userInfoEndpoint, "GET");
        webRequest.SetRequestHeader("auth_code", authorizationCode);
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

        string userInfo = webRequest.downloadHandler.text.Replace("\"", "");


        if (!string.IsNullOrEmpty(userInfo))
        {
            GetUserInfo(userInfo);
        }
        else
        {
            Debug.LogError("Login Failed" + " " + webRequest.downloadHandler.text);
        }
    }

    private void GetUserInfo(string userInfoString)
    {
        Debug.Log("User info: " + userInfoString);

        // Parse the JSON to extract wallet addresses
        var userInfo = JsonUtility.FromJson<UserInfoResponse>(userInfoString);
        userWallet = userInfo.addr;
        Debug.Log("User wallet address: " + userWallet);
        accountManager.wallet = userWallet;
        accountManager.LoginAccount();
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