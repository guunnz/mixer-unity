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
    public List<Combos> combos = new List<Combos>();
    public List<Position> position = new List<Position>();
    public string landTokenId;
    public LandType landType;
}

[System.Serializable]
public class SaveableAxieTeamsWrapper
{
    public AxieTeam[] axieTeams;
}

public class TeamManager : MonoBehaviour
{
    private string teamsFilePath;
    public List<AxieTeam> teams = new List<AxieTeam>();
    public AxieTeam currentTeam;
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

    public void SaveTeams()
    {
        string json = JsonUtility.ToJson(new SaveableAxieTeamsWrapper() { axieTeams = teams.ToArray() }, true);

        teamsFilePath = Path.Combine(Application.persistentDataPath,
            RunManagerSingleton.instance.user_wallet_address + "axieTeams3.json");

        File.WriteAllText(teamsFilePath, json);
    }

    public void LoadLastAccountAxies()
    {
        teamsFilePath = Path.Combine(Application.persistentDataPath,
            RunManagerSingleton.instance.user_wallet_address + "axieTeams3.json");
        teams = LoadTeams();
        if (teams == null)
        {
            teams = new List<AxieTeam>();
            return;
        }

        foreach (var axieTeam in teams)
        {
            foreach (var axieTeamAxieId in axieTeam.AxieIds)
            {
                axieTeamAxieId.LoadGraphicAssets();
            }
        }

        string selectedTeamName = PlayerPrefs.GetString(PlayerPrefsValues.AxieTeamSelected);

        if (!string.IsNullOrEmpty(selectedTeamName))
        {
            currentTeam = teams.Single(x => x.TeamName == selectedTeamName);
            axiesManager.ShowMenuAxies(currentTeam);
        }
    }

    private List<AxieTeam> LoadTeams()
    {
        try
        {
            if (File.Exists(teamsFilePath))
            {
                string json = File.ReadAllText(teamsFilePath);
                return JsonUtility.FromJson<SaveableAxieTeamsWrapper>(json).axieTeams.ToList();
            }
            else
            {
                return new List<AxieTeam>();
            }
        }
        catch
        {
            return null;
        }
    }
}