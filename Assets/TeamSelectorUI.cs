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
        yield return new WaitForSeconds(0.2f);
        CreateAxiesUI();
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

    public void CreateAxiesUI()
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
            TeamName.text = TeamManager.instance.currentTeam.TeamName;
        }

        for (int i = 0; i < TeamItems.Count; i++)
        {
            if (i < TeamManager.instance.teams.Count)
            {
                AxieTeam team = TeamManager.instance.teams[i];

                TeamItems[i].gameObject.SetActive(true);
                TeamItems[i].SetTeamGraphics(team);

                if (team.TeamName == TeamManager.instance.currentTeam.TeamName)
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
    }

    public void DeleteTeam()
    {
        TeamManager.instance.teams.Remove(TeamManager.instance.currentTeam);

        if (TeamManager.instance.teams.Count > 0)
        {
            TeamManager.instance.currentTeam = TeamManager.instance.teams[0];

            PlayerPrefs.SetString(PlayerPrefsValues.AxieTeamSelected, TeamManager.instance.teams[0].TeamName);
        }
        else
        {
            TeamManager.instance.currentTeam = null;
            PlayerPrefs.SetString(PlayerPrefsValues.AxieTeamSelected, "");
        }

        TeamManager.instance.SaveTeams();

        CreateAxiesUI();
    }

    public void SelectTeam()
    {
        PlayerPrefs.SetString(PlayerPrefsValues.AxieTeamSelected, TeamManager.instance.currentTeam?.TeamName);
        TeamManager.instance.axiesManager.ShowMenuAxies(TeamManager.instance.currentTeam);
        Exit();
    }
}