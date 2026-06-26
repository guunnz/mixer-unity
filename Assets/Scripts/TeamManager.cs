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
public class MonsterTeam
{
    public string TeamName;
    public List<GetMonstersExample.Monster> MonsterIds;
    public List<Combos> combos = new List<Combos>();
    public List<Position> position = new List<Position>();
    public string landTokenId;
    public LandType landType;
}

[System.Serializable]
public class SaveableMonsterTeamsWrapper
{
    public MonsterTeam[] monsterTeams;
}

public class TeamManager : MonoBehaviour
{
    private string teamsFilePath;
    public List<MonsterTeam> teams = new List<MonsterTeam>();
    public MonsterTeam currentTeam;
    public MonstersManager monstersManager;
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
        string json = JsonUtility.ToJson(new SaveableMonsterTeamsWrapper() { monsterTeams = teams.ToArray() }, true);

        teamsFilePath = Path.Combine(Application.persistentDataPath,
            RunManagerSingleton.instance.user_wallet_address + "monsterTeams3.json");

        File.WriteAllText(teamsFilePath, json);
    }

    public void LoadLastAccountMonsters()
    {
        StartCoroutine(ILoadTeam());
    }

    private List<GetMonstersExample.Monster> monstersIdNotLoaded = new List<GetMonstersExample.Monster>();
    private List<MonsterTeam> teamsNotLoaded = new List<MonsterTeam>();

    IEnumerator ILoadTeam()
    {
        teamsFilePath = Path.Combine(Application.persistentDataPath,
    RunManagerSingleton.instance.user_wallet_address + "monsterTeams3.json");
        teams = LoadTeams();
        if (teams == null)
        {
            Loading.instance.DisableLoading();
            teams = new List<MonsterTeam>();

            yield break;
        }

        for (int i = 0; i < teams.Count; i++)
        {
            var monsterTeam = teams[i];
            foreach (var monsterTeamMonsterId in monsterTeam.MonsterIds)
            {
                if (AccountManager.userMonsters.results.FirstOrDefault(x => x.id == monsterTeamMonsterId.id) == null)
                {
                    if (AccountManager.Instance.StartedLoading)
                    {
                        monstersIdNotLoaded.Add(monsterTeamMonsterId);
                        teamsNotLoaded.Add(monsterTeam);
                    }
                    else
                    {
                        removed = true;
                        teams.Remove(monsterTeam);
                    }
                    continue;
                }
                monsterTeamMonsterId.LoadGraphicAssets();
            }
        }

        string selectedTeamName = PlayerPrefs.GetString(PlayerPrefsValues.MonsterTeamSelected + RunManagerSingleton.instance.user_wallet_address);

        if (!string.IsNullOrEmpty(selectedTeamName))
        {
            currentTeam = teams.FirstOrDefault(x => x.TeamName == selectedTeamName);
            if (currentTeam == null)
            {
                Loading.instance.DisableLoading();
                StartCoroutine(TryToLoadAsync());
                if (removed)
                {
                    NotificationErrorManager.instance.DoNotification("Some mons that you had on one of your teams are not longer available. Teams will be deleted");
                }
                yield break;
            }

            foreach (var monster in currentTeam.MonsterIds)
            {
                while (!AccountManager.userMonsters.results.Select(y => y.id).Contains(monster.id) && AccountManager.Instance.StartedLoading)
                {
                    yield return null;
                }

                if (!AccountManager.userMonsters.results.Select(y => y.id).Contains(monster.id))
                {
                    Loading.instance.DisableLoading();
                    StartCoroutine(TryToLoadAsync());
                    if (removed)
                    {
                        NotificationErrorManager.instance.DoNotification("Some mons that you had on one of your teams are not longer available. Teams will be deleted");
                    }
                    yield break;
                }
            }
            monstersManager.ShowMenuMonsters(currentTeam);
        }

        if (removed)
        {
            NotificationErrorManager.instance.DoNotification("Some mons that you had on one of your teams are not longer available. Teams will be deleted");
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

        foreach (var monsterNotLoaded in monstersIdNotLoaded)
        {
            if (AccountManager.userMonsters.results.FirstOrDefault(x => x.id == monsterNotLoaded.id) == null)
            {
                teams.RemoveAll(x => x.MonsterIds.Select(y => y.id).Contains(monsterNotLoaded.id));
                continue;
            }

            monsterNotLoaded.LoadGraphicAssets();
        }
    }

    private List<MonsterTeam> LoadTeams()
    {
        try
        {
            if (File.Exists(teamsFilePath))
            {
                string json = File.ReadAllText(teamsFilePath);
                return JsonUtility.FromJson<SaveableMonsterTeamsWrapper>(json).monsterTeams.ToList();
            }
            else
            {
                return new List<MonsterTeam>();
            }
        }
        catch
        {
            return null;
        }
    }
}
