using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamSelectorUI : MonoBehaviour
{
    public GameObject FakeUI;
    public GameObject RealAxies;
    public GameObject RealLand;
    public List<TeamItemUI> TeamItems;

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

    public void CreateAxiesUI()
    {
        if (TeamManager.instance.teams.Count == 1)
        {
            TeamManager.instance.currentTeam = TeamManager.instance.teams[0];
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

    public void SelectTeam()
    {
        PlayerPrefs.SetString(PlayerPrefsValues.AxieTeamSelected, TeamManager.instance.currentTeam.TeamName);
        TeamManager.instance.axiesManager.ShowMenuAxies(TeamManager.instance.currentTeam);
        Exit();
    }
}