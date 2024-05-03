using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class AxieForBackend
{
    public string axieid;
    public int[] combo = new[] { 1, 2, 3 };
    public int row = 1;
    public int col = 1;
}

public class AxieTeamIdsWrapper
{
    public AxieForBackend[] axies;
}

public class AxieLandBattleTarget : MonoBehaviour
{
    private string postUrl = "https://axielandbattles-teams-api-go.onrender.com/api/team";
    private string getUrl = "https://axielandbattles-teams-api-go.onrender.com/api/team/:score";
    private int maxRetries = 5;

    public void PostTeam(int score, List<AxieController> axies)
    {
        List<AxieForBackend> axieForBackends = new List<AxieForBackend>();

        foreach (var axie in axies)
        {
            AxieForBackend axieForBackend = new AxieForBackend();
            axieForBackend.axieid = axie.AxieId.ToString();
            axieForBackend.row = Mathf.Abs(axie.startingRow - 7);
            axieForBackend.col = Mathf.Abs(axie.startingCol - 5);
            axieForBackend.combo = axie.axieSkillController.GetAxieSkills().Select(x => (int)x.skillName).ToArray();

            axieForBackends.Add(axieForBackend);
        }

        AxieTeamIdsWrapper wrapper = new AxieTeamIdsWrapper
        {
            axies = axieForBackends.ToArray()
        };

        PostScore(JsonUtility.ToJson(wrapper), score);
    }

    // Method to post data
    public void PostScore(string jsonData, int score)
    {
        StartCoroutine(PostRequest(postUrl, jsonData, maxRetries,
            score));
    }

    IEnumerator PostRequest(string url, string jsonData, int retries, int score)
    {
        string urlWithScore = $"{url}?score={score}";

        UnityWebRequest webRequest = new UnityWebRequest(urlWithScore, "POST");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonData);
        webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return webRequest.SendWebRequest();

        if (webRequest.isNetworkError || webRequest.isHttpError)
        {
            Debug.LogError(webRequest.error);
            if (retries > 0)
            {
                Debug.Log("Retrying POST request. Attempts remaining: " + (retries - 1));
                StartCoroutine(PostRequest(url, jsonData, retries - 1, score));
            }
        }
        else
        {
            Debug.Log("Post request complete! " + webRequest.downloadHandler.text);
        }
    }


    // Method to get data as an async Task
    public async Task<string> GetScoreAsync(string score)
    {
        string url = getUrl.Replace(":score", score);
        Debug.Log("requested " + url);
        return await GetRequestAsync(url, maxRetries);
    }

    private async Task<string> GetRequestAsync(string url, int retries)
    {
        Debug.Log("requested 2 " + url);
        while (retries > 0)
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                await webRequest.SendWebRequestAsync();

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Debug.LogError(webRequest.error);
                    retries--;
                    if (retries == 0) return null;
                }
                else
                {
                    Debug.Log("Get request complete! " + webRequest.downloadHandler.text);
                    return webRequest.downloadHandler.text;
                }
            }
        }

        return null;
    }
}

public static class UnityWebRequestExtensions
{
    public static Task<UnityWebRequest> SendWebRequestAsync(this UnityWebRequest webRequest)
    {
        var completionSource = new TaskCompletionSource<UnityWebRequest>();
        webRequest.SendWebRequest().completed += operation => { completionSource.SetResult(webRequest); };
        return completionSource.Task;
    }
}