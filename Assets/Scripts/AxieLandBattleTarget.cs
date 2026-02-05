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
public class AxieUpgrades
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
public class AxieForBackend
{
    public string axie_id;
    public string genes = "";
    public Combos[] combos_values_per_round;
    public Position[] position_values_per_round;
    public int[] upgrades_values_per_round;
}

[System.Serializable]
public class AxieTeamDatabase
{
    public AxieForBackend[] axies;
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
    public string axie_captain_id;
    public string axie_captain_genes;
    public string username;
    public int played_rounds;
    public bool[] win_loss_record;

    public string[] opponents_run_id;

    public int land_type = 0;
    public AxieTeamDatabase axie_team;
}

public class AxieLandBattleTarget : MonoBehaviour
{
    private string postUrl = "https://run.api.axielandbattles.com/api/v1/run";
    private string getUrl = "https://run.api.axielandbattles.com/api/v1/run";
    private int maxRetries = 5;
    public SkyMavisLogin skymavisLogin;

    public void PostTeam(int score, List<AxieController> axies)
    {
        List<AxieForBackend> axieForBackends = new List<AxieForBackend>();

        foreach (var axie in axies)
        {
            AxieForBackend axieForBackend = new AxieForBackend();
            axieForBackend.axie_id = axie.AxieId.ToString();
            axieForBackend.genes = axie.Genes;

            axieForBackend.position_values_per_round = new[]
            {
                new Position() { row = axie.startingRow, col = axie.startingCol }
            };

            axieForBackend.upgrades_values_per_round = Array.Empty<int>();

            axieForBackend.combos_values_per_round = new[]
            {
                new Combos()
                    { combos_id = axie.axieSkillController.GetAxieSkills().Select(x => (int)x.skillName).ToArray() }
            };

            axieForBackends.Add(axieForBackend);
        }

        Run wrapper = new Run
        {
            user_wallet_address = RunManagerSingleton.instance.user_wallet_address,
            win_loss_record = RunManagerSingleton.instance.resultsBools.ToArray(),
            axie_captain_id = PlayerPrefs.GetString("Captain" + RunManagerSingleton.instance.user_wallet_address),
            axie_captain_genes = AccountManager.userAxies.results.Single(x => x.id == PlayerPrefs.GetString("Captain" + RunManagerSingleton.instance.user_wallet_address)).newGenes,
            played_rounds = score,
            username = AccountManager.username,
            opponents_run_id = new[] { RunManagerSingleton.instance.currentOpponent },
            land_type = (int)RunManagerSingleton.instance.landType,
            axie_team = new AxieTeamDatabase()
            {
                axies = axieForBackends.ToArray(),
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
        if (OfflineMode.Enabled)
        {
            RunManagerSingleton.instance.runId = "offline-" + DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return;
        }
        StartCoroutine(PostRequest(postUrl, jsonData, maxRetries));
    }

    public void PutScore(string jsonData)
    {
        if (OfflineMode.Enabled)
        {
            return;
        }
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
        if (OfflineMode.Enabled)
        {
            int score = 0;
            int.TryParse(played_rounds, out score);

            // Offline: generate an opponent from the player's currently selected team.
            // This keeps the "endpoint schema" intact while not requiring any network calls.
            var opponent = BuildOfflineOpponentFromCurrentTeam(score);
            if (opponent == null)
                return await Task.FromResult<string>(null);

            RunManagerSingleton.instance.currentOpponent = opponent.user_wallet_address;
            return await Task.FromResult(JsonConvert.SerializeObject(opponent));
        }

        string url = $"{getUrl}?played_rounds={played_rounds}&user_wallet_address={RunManagerSingleton.instance.user_wallet_address}";
        Debug.Log("requested " + url);
        return await GetRequestAsync(url, maxRetries);
    }

    private static Opponent BuildOfflineOpponentFromCurrentTeam(int score)
    {
        // Safety: score indexes arrays; ensure at least 1 element (round 0)
        int rounds = Mathf.Max(1, score + 1);

        AxieTeam team = TeamManager.instance != null ? TeamManager.instance.currentTeam : null;
        List<GetAxiesExample.Axie> teamAxies = team != null ? team.AxieIds : null;

        // Fallback: use first 5 owned axies if no team selected
        if (teamAxies == null || teamAxies.Count == 0)
        {
            teamAxies = AccountManager.userAxies?.results?.Take(5).ToList();
        }

        if (teamAxies == null || teamAxies.Count == 0)
            return null;

        // Resolve captain (prefer saved captain, else first axie)
        string wallet = RunManagerSingleton.instance != null ? RunManagerSingleton.instance.user_wallet_address : "offline";
        string captainId = PlayerPrefs.GetString("Captain" + wallet);
        var captainAxie = !string.IsNullOrEmpty(captainId)
            ? AccountManager.userAxies?.results?.FirstOrDefault(x => x != null && x.id == captainId)
            : null;
        if (captainAxie == null)
            captainAxie = AccountManager.userAxies?.results?.FirstOrDefault(x => x != null && x.id == teamAxies[0].id);
        if (captainAxie == null)
            captainAxie = teamAxies[0];

        // Build backend axies
        List<AxieForBackend> backendAxies = new List<AxieForBackend>();
        for (int i = 0; i < Mathf.Min(5, teamAxies.Count); i++)
        {
            var axieRef = teamAxies[i];
            if (axieRef == null || string.IsNullOrEmpty(axieRef.id))
                continue;

            // Ensure we can access genes + parts from the canonical owned-axie record
            var owned = AccountManager.userAxies?.results?.FirstOrDefault(x => x != null && x.id == axieRef.id) ?? axieRef;
            string genes = !string.IsNullOrEmpty(owned.newGenes) ? owned.newGenes : owned.genes;
            if (string.IsNullOrEmpty(genes))
                continue;

            // Skills available for combos: 4 body parts (horn/mouth/back/tail)
            var skills = owned.parts == null
                ? new List<SkillName>()
                : owned.parts
                    .Where(p => p != null && p.BodyPart != BodyPart.Ears && p.BodyPart != BodyPart.Eyes)
                    .Select(p => p.SkillName)
                    .Distinct()
                    .ToList();

            if (skills.Count == 0)
            {
                // As a fallback, derive skills from genes (same logic enemy builder uses)
                var partsAbilities = AxieGeneUtils.ParsePartIdsFromHex(genes);
                var partsClasses = AxieGeneUtils.GetAxiePartsClasses(genes);
                if (partsAbilities != null && partsClasses != null && partsAbilities.Count >= 6 && partsClasses.Count >= 6)
                {
                    // Indices used throughout the project: [2]=horn, [3]=mouth, [4]=back, [5]=tail
                    var horn = new GetAxiesExample.Part(partsClasses[2], "", "horn", 0, false, partsAbilities[2]);
                    var mouth = new GetAxiesExample.Part(partsClasses[3], "", "mouth", 0, false, partsAbilities[3]);
                    var back = new GetAxiesExample.Part(partsClasses[4], "", "back", 0, false, partsAbilities[4]);
                    var tail = new GetAxiesExample.Part(partsClasses[5], "", "tail", 0, false, partsAbilities[5]);
                    skills = new List<SkillName> { horn.SkillName, mouth.SkillName, back.SkillName, tail.SkillName }
                        .Distinct()
                        .ToList();
                }
            }

            // Deterministic random per-axie/per-round so it feels "varied" but reproducible.
            System.Random rng = new System.Random(AxieIdUtil.ToStableInt(owned.id) ^ (score * 7919) ^ 0x5f3759df);

            Combos[] combosPerRound = new Combos[rounds];
            Position[] posPerRound = new Position[rounds];
            for (int r = 0; r < rounds; r++)
            {
                combosPerRound[r] = new Combos
                {
                    combos_id = PickRandomCombos(skills, rng)
                };

                // Note: These row/col values are in "backend space". Enemy `Team.AddCharacter()` converts them into map coords.
                posPerRound[r] = new Position
                {
                    row = GetEnemyRow(team, i, rng),
                    col = GetEnemyCol(team, i, rng),
                };
            }

            backendAxies.Add(new AxieForBackend
            {
                axie_id = owned.id,
                genes = genes,
                combos_values_per_round = combosPerRound,
                position_values_per_round = posPerRound,
                upgrades_values_per_round = Array.Empty<int>(),
            });
        }

        if (backendAxies.Count == 0)
            return null;

        UpgradeValuesPerRound[] teamUpgradesPerRound = new UpgradeValuesPerRound[rounds];
        for (int r = 0; r < rounds; r++)
        {
            teamUpgradesPerRound[r] = new UpgradeValuesPerRound { upgrades_ids = Array.Empty<UpgradeAugument>() };
        }

        // Randomize opponent land (deterministic-ish per score/wallet) so offline doesn't feel repetitive
        LandType playerLand = RunManagerSingleton.instance != null ? RunManagerSingleton.instance.landType : LandType.savannah;
        System.Random opponentRng = new System.Random((score + 1) * 73856093 ^ AxieIdUtil.ToStableInt(wallet) ^ 0x1234abcd);
        Array landVals = Enum.GetValues(typeof(LandType));
        int landType = (int)playerLand;
        if (landVals.Length > 1)
        {
            for (int t = 0; t < 5; t++)
            {
                landType = (int)landVals.GetValue(opponentRng.Next(landVals.Length));
                if (landType != (int)playerLand) break;
            }
        }

        return new Opponent
        {
            user_wallet_address = "offline-opponent-" + Guid.NewGuid().ToString("N").Substring(0, 10),
            username = "Offline Mirror",
            axie_captain_id = captainAxie.id,
            axie_captain_genes = !string.IsNullOrEmpty(captainAxie.newGenes) ? captainAxie.newGenes : captainAxie.genes,
            land_type = landType,
            axie_team = new AxieTeamDatabase
            {
                axies = backendAxies.ToArray(),
                team_upgrades_values_per_round = teamUpgradesPerRound
            }
        };
    }

    private static int[] PickRandomCombos(List<SkillName> skills, System.Random rng)
    {
        if (skills == null || skills.Count == 0)
            return Array.Empty<int>();

        // Usually 2-4 skills per combo selection; bias toward 3-4 to keep enemy "active".
        int count = Mathf.Clamp(2 + (rng.NextDouble() < 0.6 ? 1 : 0) + (rng.NextDouble() < 0.4 ? 1 : 0), 1, skills.Count);

        // Fisher-Yates shuffle on a copy
        var pool = skills.Select(s => (int)s).ToArray();
        for (int i = pool.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        return pool.Take(count).ToArray();
    }

    // Enemy positioning: try mirroring player's saved team positions, otherwise use a safe default pattern.
    private static int GetEnemyRow(AxieTeam team, int index, System.Random rng)
    {
        if (team != null && team.position != null && index < team.position.Count)
        {
            // Team positions are stored in LOCAL map coords (x=row). Enemy placement uses: localX = Abs(backendRow - 7) => 7-backendRow.
            // To mirror across the board, set backendRow = playerLocalRow so enemy localX becomes (7 - playerLocalRow).
            int playerLocalRow = team.position[index].row;
            return Mathf.Clamp(playerLocalRow, 0, 7);
        }
        // Default ENEMY LOCAL rows near far side (>=4), then convert to backendRow: backendRow = 7 - localX
        int[] enemyLocalRows = { 6, 6, 6, 7, 7 };
        int localX = enemyLocalRows[Mathf.Clamp(index, 0, enemyLocalRows.Length - 1)];
        return Mathf.Clamp(7 - localX, 0, 7);
    }

    private static int GetEnemyCol(AxieTeam team, int index, System.Random rng)
    {
        if (team != null && team.position != null && index < team.position.Count)
        {
            // Enemy placement uses: localY = Abs(backendCol - 4). To keep the same "column" as the player,
            // set backendCol = 4 + playerLocalCol so localY == playerLocalCol.
            int playerLocalCol = team.position[index].col;
            return 4 + Mathf.Clamp(playerLocalCol, 0, 7);
        }
        // Default ENEMY LOCAL cols spread out, then convert to backendCol: backendCol = 4 + localY
        int[] enemyLocalCols = { 1, 3, 5, 2, 4 };
        int localY = enemyLocalCols[Mathf.Clamp(index, 0, enemyLocalCols.Length - 1)];
        return 4 + Mathf.Clamp(localY, 0, 7);
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