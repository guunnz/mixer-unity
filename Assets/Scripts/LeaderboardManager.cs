using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using TMPro;
using UnityEngine.Networking;
using System.Linq;
[Serializable]
public class LeaderboardDTO
{
    public string username;
    public float avg_wins;
    public int elo;
    public string axie_captain_genes;
    public string axie_captain_id;
    public string user_wallet_address;
}

[Serializable]
public class LeaderboardResponseDTO
{
    public List<LeaderboardDTO> data;
}

public class LeaderboardManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject leaderboardEntryPrefab;
    public TextMeshProUGUI loading;
    public Transform leaderboardContainer;
    private List<LeaderboardDTO> leaderboardData = new List<LeaderboardDTO>();
    private string leaderboardEndpoint = "http://34.23.94.40:8081/api/v1/leaderboard";
    public LeaderboardUIItem myData;
    private bool isLoading;

    private void OnEnable()
    {
        if (leaderboardContainer.childCount > 0)
            return;

        isLoading = true;
        StartCoroutine(LoadingCoroutine());
        loading.gameObject.SetActive(true);
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
                    Debug.Log("Retrying GET request. Attempts remaining: " + (retries - 1));
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
        isLoading = false;
        loading.gameObject.SetActive(false);
        // Deserialize the JSON response to get leaderboard data
        LeaderboardResponseDTO leaderboardResponse = JsonUtility.FromJson<LeaderboardResponseDTO>(jsonResponse);

        leaderboardData = leaderboardResponse.data.OrderByDescending(x => x.elo).ToList();

        var entryMine = leaderboardData.FirstOrDefault(x => x.user_wallet_address == RunManagerSingleton.instance.user_wallet_address);

        if (entryMine == null)
        {
            myData.gameObject.SetActive(false);
        }
        else
        {
            myData.gameObject.SetActive(true);
            myData.SetUsername(entryMine.username);
            myData.SetAvgWins(entryMine.avg_wins);
            myData.SetElo(entryMine.elo);
            myData.SetRanking(leaderboardData.IndexOf(entryMine) + 1);
            myData.SetCaptainGraphics(entryMine.axie_captain_id, entryMine.axie_captain_genes);
        }


        // Clear any existing entries
        foreach (Transform child in leaderboardContainer)
        {
            Destroy(child.gameObject);
        }

        // Create a new entry for each leaderboard item
        foreach (var entry in leaderboardData)
        {
            CreateLeaderboardEntry(entry, leaderboardData.IndexOf(entry) + 1);
        }
    }
    public IEnumerator LoadingCoroutine()
    {
        loading.text = "Loading";
        while (isLoading)
        {
            loading.text += ".";
            yield return new WaitForSeconds(0.3f);

            if (loading.text.Count(x => x == '.') >= 3)
            {
                loading.text = "Loading";
            }
        }
    }
    private void CreateLeaderboardEntry(LeaderboardDTO entry, int index)
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
            uiItem.SetRanking(index);
            uiItem.SetCaptainGraphics(entry.axie_captain_id, entry.axie_captain_genes);
        }
    }
}
