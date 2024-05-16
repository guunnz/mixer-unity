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
    public PositionValues[] position_values_per_round;
}

[System.Serializable]
public class PositionValues
{
    public int row;
    public int col;
}

[System.Serializable]
public class ComboValuesPerRound
{
    public int[] combos_id;
}

[System.Serializable]
public class Combos
{
    public ComboValuesPerRound[] combos_values_per_round;
}

[System.Serializable]
public class AxieUpgrades
{
    public int[] upgrades_id;
}

[System.Serializable]
public class UpgradeValuesPerRound
{
    public List<UpgradeAugument> upgrade_values_per_round = new List<UpgradeAugument>();
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
    public AxieForBackend[] axies;
    public UpgradeValuesPerRound[] team_upgrades;
}

[System.Serializable]
public class Run
{
    public string user_id;
    public int score;
    public bool[] rounds;

    [FormerlySerializedAs("opponent_run_id")]
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

            axieForBackend.position.position_values_per_round = new[]
            {
                new PositionValues() { row = axie.startingRow, col = axie.startingCol }
            };

            axieForBackend.upgrades = new AxieUpgrades()
            {
                upgrades_id = RunManagerSingleton.instance.axieUpgrades
                    .Where(x => x.axieId == axie.AxieId.ToString())
                    .Select(x => (int)x.upgrade).ToArray()
            };

            axieForBackend.combos = new Combos()
            {
                combos_values_per_round = new[]
                {
                    new ComboValuesPerRound()
                        { combos_id = axie.axieSkillController.GetAxieSkills().Select(x => (int)x.skillName).ToArray() }
                }
            };

            axieForBackends.Add(axieForBackend);
        }

        Run wrapper = new Run
        {
            user_id = RunManagerSingleton.instance.userId,
            rounds = RunManagerSingleton.instance.resultsBools.ToArray(),
            score = score,
            opponents_run_id = new[] { RunManagerSingleton.instance.currentOpponent },
            land_type = (int)RunManagerSingleton.instance.landType,
            axie_team = new AxieTeamDatabase()
            {
                axies = axieForBackends.ToArray(),
                team_upgrades = new UpgradeValuesPerRound[]
                {
                    new UpgradeValuesPerRound()
                    {
                        upgrade_values_per_round =
                            RunManagerSingleton.instance.globalUpgrades[score].upgrade_values_per_round
                    }
                }
            }
        };

        if (RunManagerSingleton.instance.score == 0)
        {
            PostScore(JsonUtility.ToJson(wrapper));
        }
        else
        {
            PutScore(JsonUtility.ToJson(wrapper));
        }
    }

// Method to post data
    public void PostScore(string jsonData)
    {
        StartCoroutine(PostRequest(postUrl, jsonData, maxRetries));
    }

    public void PutScore(string jsonData)
    {
        StartCoroutine(PutRequest(postUrl, jsonData, maxRetries));
    }

    IEnumerator PostRequest(string url, string jsonData, int retries)
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
                StartCoroutine(PostRequest(url, jsonData, retries - 1));
            }
        }
        else
        {
            RunManagerSingleton.instance.runId = webRequest.downloadHandler.text.Replace("\"", "");
        }
    }

    IEnumerator PutRequest(string url, string jsonData, int retries)
    {
        UnityWebRequest webRequest = new UnityWebRequest(url + "?id=" + RunManagerSingleton.instance.runId, "PUT");
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
                StartCoroutine(PutRequest(url, jsonData, retries - 1));
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
        string url = $"{getUrl}?score={score}&user_id={RunManagerSingleton.instance.userId}";
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