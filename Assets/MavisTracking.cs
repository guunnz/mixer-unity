using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class MavisTracking : MonoBehaviour
{
    private static MavisTracking _instance;
    public static MavisTracking Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("MavisTracking");
                _instance = go.AddComponent<MavisTracking>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private string apiUrl = "https://x.skymavis.com/track";
    private string apiKey = "9d9487ef-f95c-4da9-a8a0-d7ad37bb8c22";
    private string roninAddress;
    private string sessionID;
    private int eventOffset = 0;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeTracking(SkyMavisLogin.UserInfo userInfo)
    {
        this.roninAddress = userInfo.addr;
        sessionID = System.Guid.NewGuid().ToString();
        StartCoroutine(TrackHeartbeat());
    }

    private IEnumerator TrackHeartbeat()
    {
        while (true)
        {
            yield return TrackEvent("track", new Dictionary<string, object> { { "action", "heartbeat" } });
            yield return new WaitForSeconds(30); // Send heartbeat every 30 seconds
        }
    }

    private IEnumerator TrackEvent(string eventType, Dictionary<string, object> eventData)
    {
        string uuid = System.Guid.NewGuid().ToString();
        string timestamp = System.DateTime.UtcNow.ToString("o");

        // Prepare base data
        var baseData = new Dictionary<string, object>
        {
            { "uuid", uuid },
            { "ref", "root" },
            { "timestamp", timestamp },
            { "session_id", sessionID },
            { "offset", eventOffset++ },
            { "ronin_address", roninAddress }
        };

        foreach (var kvp in eventData)
        {
            baseData[kvp.Key] = kvp.Value;
        }

        // Event object
        var eventObject = new Dictionary<string, object>
        {
            { "type", eventType },
            { "data", baseData }
        };

        // Events array
        var eventsList = new List<Dictionary<string, object>> { eventObject };

        // Final payload with api_key included
        var payload = new Dictionary<string, object>
        {
            { "api_key", apiKey },
            { "events", eventsList }
        };

        // Serialize using Newtonsoft.Json
        string jsonPayload = JsonConvert.SerializeObject(payload);
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes(jsonPayload);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(jsonBytes);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // Authentication
        string credentials = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(apiKey + ":"));
        request.SetRequestHeader("Authorization", "Basic " + credentials);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error sending event: " + request.error + " " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("Event tracked successfully: " + eventType);
        }
    }

    public void TrackIdentify(Dictionary<string, object> userProperties)
    {
        StartCoroutine(TrackEvent("identify", new Dictionary<string, object>
        {
            { "user_properties", userProperties }
        }));
    }

    public void TrackScreen(string screenName)
    {
        StartCoroutine(TrackEvent("screen", new Dictionary<string, object>
        {
            { "screen", screenName }
        }));
    }

    public void TrackAction(string action, Dictionary<string, object> actionProperties = null)
    {
        var data = new Dictionary<string, object>
        {
            { "action", action }
        };
        if (actionProperties != null)
        {
            foreach (var kvp in actionProperties)
            {
                data[kvp.Key] = kvp.Value;
            }
        }
        StartCoroutine(TrackEvent("track", data));
    }
}
