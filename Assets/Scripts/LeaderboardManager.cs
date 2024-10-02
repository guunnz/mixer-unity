using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Game;
using System.Collections;
using static SkyMavisLogin;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using SimpleGraphQL;

[Serializable]
public class LeaderboardDTO
{
    public string username;
    public int avg_wins;
    public int elo;
    public string axie_captain_genes;
    public string captain_id;
}

[Serializable]
public class LeaderboardResponseDTO
{
    public List<LeaderboardDTO> data;
    public DateTime last_sync;
}

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject leaderboardEntryPrefab;
    public Transform leaderboardContainer;
    private List<LeaderboardDTO> leaderboardData = new List<LeaderboardDTO>();
    private string leaderboardEndpoint;

    private void OnEnable()
    {
        StartCoroutine(GetLeaderboard());
    }

    public IEnumerator GetLeaderboard(int retries = 5)
    {
        using (UnityWebRequest www = new UnityWebRequest(leaderboardEndpoint, "GET"))
        {
            Debug.Log("REQ LINK: " + leaderboardEndpoint);
            DownloadHandlerBuffer dH = new DownloadHandlerBuffer();
            www.downloadHandler = dH;
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogError(www.error);
                if (retries > 0)
                {
                    Debug.Log("Retrying POST request. Attempts remaining: " + (retries - 1));
                    StartCoroutine(GetLeaderboard(retries - 1));
                }
            }
            else
            {
                PopulateLeaderboard(www.downloadHandler.text);
            }
        }

    }

    public void PopulateLeaderboard(string jsonResponse)
    {
        // Deserialize the JSON response to get leaderboard data
        LeaderboardResponseDTO leaderboardResponse = JsonUtility.FromJson<LeaderboardResponseDTO>(jsonResponse);
        leaderboardData = leaderboardResponse.data;

        // Clear any existing entries
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }

        // Create a new entry for each leaderboard item
        foreach (var entry in leaderboardData)
        {
            CreateLeaderboardEntry(entry);
        }
    }

    private void CreateLeaderboardEntry(LeaderboardDTO entry)
    {
        // Instantiate the prefab and set parent
        GameObject newEntry = Instantiate(leaderboardEntryPrefab, leaderboardContainer);

        // Assuming the prefab has a UIItem script attached to it
        LeaderboardUIItem uiItem = newEntry.GetComponent<LeaderboardUIItem>();

        // Set the UI fields in the UIItem class
        if (uiItem != null)
        {
            uiItem.SetUsername(entry.username);
            uiItem.SetAvgWins(entry.avg_wins);
            uiItem.SetElo(entry.elo);
            uiItem.SetCaptainGraphics(entry.captain_id, entry.axie_captain_genes);
        }
    }
}
