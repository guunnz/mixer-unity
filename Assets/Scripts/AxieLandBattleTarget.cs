using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

[System.Serializable]
public class Position
{
    public PositionValues[] position_values;
}

[System.Serializable]
public class PositionValues
{
    public int row;
    public int col;
}

[System.Serializable]
public class Combos
{
    public int[] combos_id;
}

[System.Serializable]
public class AxieUpgrades
{
    public int[] upgrades_id;
}

[System.Serializable]
public class AxieForBackend
{
    public string axie_id;
    public Combos combos;
    public Position position;
    public AxieUpgrades upgrades;
}

[System.Serializable]
public class AxieTeamDatabase
{
    public AxieForBackend[] axie;
    public UpgradeAugument[] team_upgrades;
}

[System.Serializable]
public class Run
{
    public string user_id;
    public int score;
    public bool[] rounds;
    public string[] opponents_run_id;
    public int land_type = 0;
    public AxieTeamDatabase axie_team;
}

public class AxieLandBattleTarget : MonoBehaviour
{
    private string postUrl = "https://melodic-voice-423218-s4.ue.r.appspot.com/api/v1/run";
    private string getUrl = "https://melodic-voice-423218-s4.ue.r.appspot.com/api/v1/run";
    private int maxRetries = 5;

    public void PostTeam(int score, List<AxieController> axies)
    {
        List<AxieForBackend> axieForBackends = new List<AxieForBackend>();

        foreach (var axie in axies)
        {
            AxieForBackend axieForBackend = new AxieForBackend();
            axieForBackend.axie_id = axie.AxieId.ToString();
            axieForBackend.position = new Position();

            axieForBackend.position.position_values = new[]
            {
                new PositionValues() { row = Mathf.Abs(axie.startingRow - 7), col = Mathf.Abs(axie.startingCol - 4) }
            };

            axieForBackend.upgrades = new AxieUpgrades()
            {
                upgrades_id = RunManagerSingleton.instance.axieUpgrades
                    .Where(x => x.axieId == axie.AxieId.ToString())
                    .Select(x => (int)x.upgrade).ToArray()
            };

            axieForBackend.combos = new Combos()
                { combos_id = axie.axieSkillController.GetAxieSkills().Select(x => (int)x.skillName).ToArray() };

            axieForBackends.Add(axieForBackend);

            Run wrapper = new Run
            {
                user_id = RunManagerSingleton.instance.userId,
                rounds = RunManagerSingleton.instance.resultsBools.ToArray(),
                score = score,
                opponents_run_id = RunManagerSingleton.instance.opponents.ToArray(),
                land_type = (int)RunManagerSingleton.instance.landType,
                axie_team = new AxieTeamDatabase()
                {
                    axie = axieForBackends.ToArray(),
                    team_upgrades = RunManagerSingleton.instance.globalUpgrades.ToArray()
                }
            };

            PostScore(JsonUtility.ToJson(wrapper), score);
        }
    }

// Method to post data
    public void PostScore(string jsonData, int score)
    {
        StartCoroutine(PostRequest(postUrl, jsonData, maxRetries,
            score));
    }

    IEnumerator PostRequest(string url, string jsonData, int retries, int score)
    {
        UnityWebRequest webRequest = new UnityWebRequest(url, "POST");
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
        string url = $"{getUrl}?score={score}";
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