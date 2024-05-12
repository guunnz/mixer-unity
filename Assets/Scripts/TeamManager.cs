using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System.Text;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class AxieTeam
{
    public string TeamName;
    public List<GetAxiesExample.Axie> AxieIds;
    public List<Combos> combos;
    public List<Position> position;
    public string landTokenId;
}

public class TeamManager : MonoBehaviour
{
    private string teamsFilePath;
    private List<AxieTeam> teams;
    private AxieTeam currentTeam;
    public AxiesManager axiesManager;
    static public TeamManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    public void LoadLastAccountAxies()
    {
        teamsFilePath = Path.Combine(Application.persistentDataPath,
            RunManagerSingleton.instance.userId + "axieTeams.json");
        teams = LoadTeams();
        string selectedTeamName = PlayerPrefs.GetString(PlayerPrefsValues.AxieTeamSelected);

        if (!string.IsNullOrEmpty(selectedTeamName))
        {
            currentTeam = teams.Single(x => x.TeamName == selectedTeamName);
            axiesManager.ShowMenuAxies(currentTeam);
        }
    }

    public void ExitView()
    {
    }

    public void CreateAxiesUI()
    {
        //CreateAxiesUI with teams;
    }

    public void SaveTeam(string teamName, List<GetAxiesExample.Axie> teamAxies, string landTokenId)
    {
        List<string> teamNames = teams.Select(x => x.TeamName).ToList();
        if (teamNames.Contains(teamName))
        {
            Debug.LogError("Team Already Exists");
            //NotificationClass.Instance.DoError("Team Already Exists");
        }

        AxieTeam newTeam = new AxieTeam();
        newTeam.TeamName = teamName;
        newTeam.AxieIds = teamAxies;
        newTeam.landTokenId = landTokenId;
        teams.Add(newTeam);

        Debug.LogError("Team Saved Successful");
        CreateAxiesUI();
        ExitView();
        //NotificationClass.Instance.DoSuccess("Team Already Exists");
    }

    public void SelectTeam(int teamIndex)
    {
        currentTeam = teams[teamIndex];
        PlayerPrefs.SetString(PlayerPrefsValues.AxieTeamSelected, currentTeam.TeamName);
        axiesManager.ShowMenuAxies(currentTeam);
    }

    private List<AxieTeam> LoadTeams()
    {
        if (File.Exists(teamsFilePath))
        {
            string json = File.ReadAllText(teamsFilePath);
            return JsonUtility.FromJson<List<AxieTeam>>(json);
        }
        else
        {
            return new List<AxieTeam>();
        }
    }
}