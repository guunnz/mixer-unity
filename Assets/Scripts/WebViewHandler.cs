using UnityEngine;

public class WebViewHandler : MonoBehaviour
{
    WebViewObject webViewObject;

    void Start()
    {
        webViewObject = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        webViewObject.Init((msg) =>
        {
            Debug.Log("WebView message: " + msg);
        });

        webViewObject.LoadURL("http://example.com/");
        webViewObject.SetVisibility(true);
    }
}
