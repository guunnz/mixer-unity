using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using static SkyMavisLogin;


[System.Serializable]
public class Position
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
public class MonsterUpgrades
{
    public int[] upgrades_id;
}

[System.Serializable]
public class UpgradeValuesPerRound
{
    public UpgradeAugument[] upgrades_ids;
}


[System.Serializable]
public class UpgradeValuesPerRoundList
{
    public List<UpgradeAugument> team_upgrades_values_per_round;


    public UpgradeAugument[] ToUpgradeValuesPerRoundArray()
    {
        return team_upgrades_values_per_round.ToArray();
    }
}

[System.Serializable]
public class MonsterForBackend
{
    public string monster_id;
    public string genes = "";
    public Combos[] combos_values_per_round;
    public Position[] position_values_per_round;
    public int[] upgrades_values_per_round;
}

[System.Serializable]
public class MonsterTeamDatabase
{
    public MonsterForBackend[] monsters;
    public UpgradeValuesPerRound[] team_upgrades_values_per_round;

    public List<UpgradeValuesPerRound> ToUpgradeValuesPerRoundArray()
    {
        return team_upgrades_values_per_round.ToList();
    }
}

[System.Serializable]
public class TeamUpgrades
{
}

[System.Serializable]
public class Run
{
    public string user_wallet_address;
    public string monster_captain_id;
    public string monster_captain_genes;
    public string username;
    public int played_rounds;
    public bool[] win_loss_record;

    public string[] opponents_run_id;

    public int land_type = 0;
    public MonsterTeamDatabase monster_team;
}

public class MonsterLandBattleTarget : MonoBehaviour
{
    private string postUrl = "https://run.api.monsterlandbattles.com/api/v1/run";
    private string getUrl = "https://run.api.monsterlandbattles.com/api/v1/run";
    private int maxRetries = 5;
    public SkyMavisLogin skymavisLogin;

    public void PostTeam(int score, List<MonsterController> monsters)
    {
        if (LocalGameSession.IsActive)
        {
            RunManagerSingleton.instance.runId = LocalGameSession.RunId;
            return;
        }

        List<MonsterForBackend> monsterForBackends = new List<MonsterForBackend>();

        foreach (var monster in monsters)
        {
            MonsterForBackend monsterForBackend = new MonsterForBackend();
            monsterForBackend.monster_id = monster.MonsterId.ToString();
            monsterForBackend.genes = monster.Genes;

            monsterForBackend.position_values_per_round = new[]
            {
                new Position() { row = monster.startingRow, col = monster.startingCol }
            };

            monsterForBackend.upgrades_values_per_round = Array.Empty<int>();

            monsterForBackend.combos_values_per_round = new[]
            {
                new Combos()
                    { combos_id = monster.monsterSkillController.GetMonsterSkills().Select(x => (int)x.skillName).ToArray() }
            };

            monsterForBackends.Add(monsterForBackend);
        }

        Run wrapper = new Run
        {
            user_wallet_address = RunManagerSingleton.instance.user_wallet_address,
            win_loss_record = RunManagerSingleton.instance.resultsBools.ToArray(),
            monster_captain_id = PlayerPrefs.GetString("Captain" + RunManagerSingleton.instance.user_wallet_address),
            monster_captain_genes = AccountManager.userMonsters.results.Single(x => x.id == PlayerPrefs.GetString("Captain" + RunManagerSingleton.instance.user_wallet_address)).newGenes,
            played_rounds = score,
            username = AccountManager.username,
            opponents_run_id = new[] { RunManagerSingleton.instance.currentOpponent },
            land_type = (int)RunManagerSingleton.instance.landType,
            monster_team = new MonsterTeamDatabase()
            {
                monsters = monsterForBackends.ToArray(),
                team_upgrades_values_per_round = new UpgradeValuesPerRound[]
                {
                    new UpgradeValuesPerRound()
                    {
                        upgrades_ids = RunManagerSingleton.instance.globalUpgrades[score]
                            .ToUpgradeValuesPerRoundArray()
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
    IEnumerator PostRequest(string url, string jsonData, int retries)
    {
        if (LocalGameSession.IsActive)
            yield break;

        UnityWebRequest webRequest = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonData);
        webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        if (string.IsNullOrEmpty(GetTokenFromCommandLineArgs()))
        {
            var authToken = JsonConvert.DeserializeObject<AuthToken>(PlayerPrefs.GetString("Auth"));

            if (authToken.IsExpired())
            {
                StartCoroutine(skymavisLogin.RefreshToken(3, true));
            }
        }

        webRequest.SetRequestHeader("access-token", skymavisLogin.authToken.AccessToken);

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
        if (LocalGameSession.IsActive)
            yield break;

        UnityWebRequest webRequest = new UnityWebRequest(url + "?id=" + RunManagerSingleton.instance.runId, "PUT");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonData);
        webRequest.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        webRequest.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");

        var authToken = JsonConvert.DeserializeObject<AuthToken>(PlayerPrefs.GetString("Auth"));

        if (authToken.IsExpired())
        {
            StartCoroutine(skymavisLogin.RefreshToken(3, true));
        }

        webRequest.SetRequestHeader("access-token", skymavisLogin.authToken.AccessToken);

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
    public async Task<string> GetScoreAsync(string played_rounds)
    {
        if (LocalGameSession.IsActive)
        {
            int.TryParse(played_rounds, out int score);
            return LocalGameSession.GetOpponentJson(score);
        }

        string url = $"{getUrl}?played_rounds={played_rounds}&user_wallet_address={RunManagerSingleton.instance.user_wallet_address}";
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
                    if (retries == 0)
                    {
                        return null;
                    }
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
