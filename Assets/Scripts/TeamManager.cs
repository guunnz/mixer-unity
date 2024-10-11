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

    bool removed = false;

    public void SaveTeams()
    {
        TooltipManagerSingleton.instance.DisableTooltip();
        string json = JsonUtility.ToJson(new SaveableAxieTeamsWrapper() { axieTeams = teams.ToArray() }, true);

        teamsFilePath = Path.Combine(Application.persistentDataPath,
            RunManagerSingleton.instance.user_wallet_address + "axieTeams3.json");

        File.WriteAllText(teamsFilePath, json);
    }

    public void LoadLastAccountAxies()
    {
        StartCoroutine(ILoadTeam());
    }

    private List<GetAxiesExample.Axie> axiesIdNotLoaded = new List<GetAxiesExample.Axie>();
    private List<AxieTeam> teamsNotLoaded = new List<AxieTeam>();

    IEnumerator ILoadTeam()
    {
        teamsFilePath = Path.Combine(Application.persistentDataPath,
    RunManagerSingleton.instance.user_wallet_address + "axieTeams3.json");
        teams = LoadTeams();
        if (teams == null)
        {
            Loading.instance.DisableLoading();
            teams = new List<AxieTeam>();

            yield break;
        }

        for (int i = 0; i < teams.Count; i++)
        {
            var axieTeam = teams[i];
            foreach (var axieTeamAxieId in axieTeam.AxieIds)
            {
                if (AccountManager.userAxies.results.FirstOrDefault(x => x.id == axieTeamAxieId.id) == null)
                {
                    if (AccountManager.Instance.StartedLoading)
                    {
                        axiesIdNotLoaded.Add(axieTeamAxieId);
                        teamsNotLoaded.Add(axieTeam);
                    }
                    else
                    {
                        removed = true;
                        teams.Remove(axieTeam);
                    }
                    continue;
                }
                axieTeamAxieId.LoadGraphicAssets();
            }
        }

        string selectedTeamName = PlayerPrefs.GetString(PlayerPrefsValues.AxieTeamSelected + RunManagerSingleton.instance.user_wallet_address);

        if (!string.IsNullOrEmpty(selectedTeamName))
        {
            currentTeam = teams.FirstOrDefault(x => x.TeamName == selectedTeamName);
            if (currentTeam == null)
            {
                Loading.instance.DisableLoading();
                StartCoroutine(TryToLoadAsync());
                if (removed)
                {
                    NotificationErrorManager.instance.DoNotification("Some axies that you had on one of your teams are not longer available. Teams will be deleted");
                }
                yield break;
            }

            foreach (var axie in currentTeam.AxieIds)
            {
                while (!AccountManager.userAxies.results.Select(y => y.id).Contains(axie.id) && AccountManager.Instance.StartedLoading)
                {
                    yield return null;
                }

                if (!AccountManager.userAxies.results.Select(y => y.id).Contains(axie.id))
                {
                    Loading.instance.DisableLoading();
                    StartCoroutine(TryToLoadAsync());
                    if (removed)
                    {
                        NotificationErrorManager.instance.DoNotification("Some axies that you had on one of your teams are not longer available. Teams will be deleted");
                    }
                    yield break;
                }
            }
            axiesManager.ShowMenuAxies(currentTeam);
        }

        if (removed)
        {
            NotificationErrorManager.instance.DoNotification("Some axies that you had on one of your teams are not longer available. Teams will be deleted");
        }

        Loading.instance.DisableLoading();
        StartCoroutine(TryToLoadAsync());
    }

    IEnumerator TryToLoadAsync()
    {
        while (AccountManager.Instance.StartedLoading)
        {
            yield return null;
        }

        foreach (var axieNotLoaded in axiesIdNotLoaded)
        {
            if (AccountManager.userAxies.results.FirstOrDefault(x => x.id == axieNotLoaded.id) == null)
            {
                teams.RemoveAll(x => x.AxieIds.Select(y => y.id).Contains(axieNotLoaded.id));
                continue;
            }

            axieNotLoaded.LoadGraphicAssets();
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