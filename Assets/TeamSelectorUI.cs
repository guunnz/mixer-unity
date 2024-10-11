using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TeamSelectorUI : MonoBehaviour
{
    public GameObject FakeUI;
    public GameObject RealAxies;
    public GameObject RealLand;
    public TextMeshProUGUI LoadingTeams;
    public List<TeamItemUI> TeamItems;
    public TextMeshProUGUI TeamName;

    private void OnEnable()
    {
        RealAxies.SetActive(false);
        RealLand.SetActive(false);
        FakeUI.SetActive(true);
        StartCoroutine(LoadAxies());
    }

    IEnumerator LoadAxies()
    {
        LoadingTeams.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        StartCoroutine(LoadingCoroutine());
        StartCoroutine(CreateAxiesUI());
    }

    public void Exit()
    {
        RealAxies.SetActive(true);
        RealLand.SetActive(true);
        FakeUI.SetActive(false);
    }

    public void RefreshUI()
    {
        for (int i = 0; i < TeamItems.Count; i++)
        {
            if (i < TeamManager.instance.teams.Count)
            {
                AxieTeam team = TeamManager.instance.teams[i];

                if (team.TeamName == TeamManager.instance.currentTeam.TeamName)
                {
                    TeamName.text = team.TeamName;
                }
                else
                {
                    TeamItems[i].SelectTeam(false, team);
                }
            }
        }
    }
    bool removed = false;
    public IEnumerator CreateAxiesUI()
    {
        if (TeamManager.instance.teams.Count == 1)
        {
            TeamManager.instance.currentTeam = TeamManager.instance.teams[0];
        }
        else if (TeamManager.instance.teams.Count == 0)
        {
            TeamName.text = "";
            FakeAxiesManager.instance.ClearAllAxies();
        }
        else
        {
            if (TeamManager.instance.currentTeam != null)
            {

                TeamName.text = TeamManager.instance.currentTeam.TeamName;
            }

        }

        for (int i = 0; i < TeamItems.Count; i++)
        {
            if (i < TeamManager.instance.teams.Count)
            {
                AxieTeam team = TeamManager.instance.teams[i];

                if (!AccountManager.userLands.results.Any(x => x.LandTypeEnum == team.landType) || AccountManager.userLands.results.Where(x => x.LandTypeEnum == team.landType).All(x => x.locked))
                {
                    removed = true;
                    continue;
                }

                bool nullAxie = false;

                foreach (var axie in team.AxieIds)
                {
                    while (!AccountManager.userAxies.results.Any(x => x.id == axie.id) && AccountManager.Instance.StartedLoading)
                    {
                        yield return null;
                    }

                    while (AccountManager.userAxies.results.Any(x => x.skeletonDataAssetMaterial == null) && !AccountManager.Instance.StartedLoading)
                    {
                        yield return null;
                    }

                    if (!AccountManager.userAxies.results.Any(x => x.id == axie.id))
                    {
                        nullAxie = true;
                    }
                }

                if (nullAxie)
                {
                    continue;
                }

                TeamItems[i].gameObject.SetActive(true);
                TeamItems[i].SetTeamGraphics(team);

                if (TeamManager.instance.currentTeam != null && team.TeamName == TeamManager.instance.currentTeam.TeamName)
                {
                    TeamName.text = team.TeamName;
                    TeamItems[i].SelectTeam(true, team);
                }
                else
                {
                    TeamItems[i].SelectTeam(false, team);
                }
            }
            else
            {
                TeamItems[i].gameObject.SetActive(false);
            }
        }

        LoadingTeams.gameObject.SetActive(false);

        if (removed)
        {
            NotificationErrorManager.instance.DoNotification("One of the lands that you had on one of your teams are not longer available. Teams with that land will be deleted");
        }
    }

    public IEnumerator LoadingCoroutine()
    {
        LoadingTeams.text = "Loading Teams";
        while (LoadingTeams.gameObject.activeSelf)
        {
            LoadingTeams.text += ".";
            yield return new WaitForSeconds(0.2f);

            if (LoadingTeams.text.Count(x => x == '.') >= 3)
            {
                LoadingTeams.text = "Loading Teams";
            }
        }
    }
    public void DeleteTeam()
    {
        TeamManager.instance.teams.Remove(TeamManager.instance.currentTeam);

        if (TeamManager.instance.teams.Count > 0)
        {
            TeamManager.instance.currentTeam = TeamManager.instance.teams[0];

            PlayerPrefs.SetString(PlayerPrefsValues.AxieTeamSelected + RunManagerSingleton.instance.user_wallet_address, TeamManager.instance.teams[0].TeamName);
        }
        else
        {
            TeamManager.instance.currentTeam = null;
            PlayerPrefs.SetString(PlayerPrefsValues.AxieTeamSelected + RunManagerSingleton.instance.user_wallet_address, "");
        }

        TeamManager.instance.SaveTeams();

        StartCoroutine(CreateAxiesUI());
    }

    public void SelectTeam()
    {
        TooltipManagerSingleton.instance.DisableTooltip();
        PlayerPrefs.SetString(PlayerPrefsValues.AxieTeamSelected + RunManagerSingleton.instance.user_wallet_address, TeamManager.instance.currentTeam?.TeamName);
        TeamManager.instance.axiesManager.ShowMenuAxies(TeamManager.instance.currentTeam);
        Exit();
    }
}