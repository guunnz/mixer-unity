using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public enum TeamBuilderMenu
{
    Lands,
    Axies,
    Atia,
    Combo
}
public enum AxieStatFilter
{
    NoFilter,
    MoreMorale,
    MoreHP,
    MoreSpeed,
    MoreSkill,
    LessMorale,
    LessHP,
    LessSpeed,
    LessSkill
}
public class TeamBuilderManager : MonoBehaviour
{


    public GameObject LandsContent;
    public GameObject AxiesContent;

    public List<UIListLand> landList = new List<UIListLand>();
    public List<UIListAxie> axieList = new List<UIListAxie>();

    public GameObject AxieTeamUIObject;
    public GameObject DragToRemoveObject;
    private int currentPage = 1;
    public TextMeshProUGUI pageText;
    public TMP_InputField teamNameInputField;
    public TeamBuilderMenu currentTeamBuilderMenu;
    float PagesAmount = 0;
    int maxPagesAmount = 0;
    public GameObject TeamLandBuildingUI;
    public GameObject ComboUI;
    public FakeMapManager fakeMap;
    public FakeAxieComboManager fakeAxieComboManager;
    public FakeAxiesManager fakeAxiesManager;
    public FakeLandManager fakeLandManager;
    public GameObject CanvasTeams;
    public bool creatingNewTeam;

    public GameObject Container;

    public GameObject AxieStatsContainer;
    public TextMeshProUGUI AxieHPText;
    public TextMeshProUGUI AxieMoraleText;
    public TextMeshProUGUI AxieSkillText;
    public TextMeshProUGUI AxieSpeedText;
    public GetAxiesExample.Axie lastAxieChosen;
    public SkeletonGraphic statsSkeletonGraphic;
    public Image AxieClassGraphic;

    public List<AxieClass> axieClassFilters = new List<AxieClass>();
    public List<AxieStatFilter> axieStatsFilters = new List<AxieStatFilter>();

    internal delegate void StatsFilterCleared();

    internal event StatsFilterCleared statsfilterClearedEvent;

    internal delegate void ContraryStatsFilterCleared(AxieStatFilter statsFilter);

    internal event ContraryStatsFilterCleared contraryfilterClearedEvent;

    internal delegate void AxieClassFilterCleared();

    internal event AxieClassFilterCleared axieClassfilterClearedEvent;


    public void Edit()
    {
        fakeMap.ToggleRectangles();
        Container.SetActive(true);
        CanvasTeams.SetActive(false);
        creatingNewTeam = false;
        teamNameInputField.text = TeamManager.instance.currentTeam.TeamName;
        SetMenu(TeamBuilderMenu.Axies, true);
    }

    public void NewTeam()
    {
        CanvasTeams.SetActive(false);
        fakeLandManager.ChooseFakeLand(AccountManager.userLands.results[0].tokenId);
        teamNameInputField.text = "";
        fakeMap.ToggleRectangles();
        Container.SetActive(true);
        creatingNewTeam = true;
        fakeAxiesManager.ClearAllAxies();
        SetMenu(TeamBuilderMenu.Lands, true);
    }

    public void ToggleClassFilter(AxieClass axieClass)
    {
        if (axieClassFilters.Any(x => x == axieClass))
        {
            axieClassFilters.RemoveAll(x => x == axieClass);
        }
        else
        {
            axieClassFilters.Add(axieClass);
        }

        SetAxiesUI();
    }

    public void RemoveAllClassFilter()
    {
        axieClassFilters.Clear();
        axieClassfilterClearedEvent.Invoke();
        SetAxiesUI();
    }

    public void RemoveAllStatsFilter()
    {
        axieStatsFilters.Clear();
        statsfilterClearedEvent.Invoke();
        SetAxiesUI();
    }

    public void ToggleStatsFilter(AxieStatFilter statFilter)
    {
        AxieStatFilter contraryFilter = axieStatsFilters.FirstOrDefault(x =>
            x != statFilter && statFilter.ToString().Contains(x.ToString().Substring(4)));


        if (contraryFilter != AxieStatFilter.NoFilter)
        {
            axieStatsFilters.RemoveAll(x => x == contraryFilter);
            contraryfilterClearedEvent.Invoke(contraryFilter);
        }

        if (axieStatsFilters.Any(x => x == statFilter && axieStatsFilters.Contains(statFilter)))
        {
            axieStatsFilters.RemoveAll(x => x == statFilter);
        }
        else
        {
            axieStatsFilters.Add(statFilter);
        }

        SetAxiesUI();
    }

    public List<GetAxiesExample.Axie> GetFilteredList()
    {
        List<GetAxiesExample.Axie> axiesList = AccountManager.userAxies.results.ToList();

        if (axieClassFilters.Count != 0)
        {
            axiesList = axiesList.Where(x => axieClassFilters.Contains(x.axieClass)).ToList();
        }

        IOrderedEnumerable<GetAxiesExample.Axie> query = axiesList.OrderBy(x => 0);
        if (axieStatsFilters.Count != 0)
        {
            AxieStatFilter firstFilter = axieStatsFilters.First();
            if (firstFilter.ToString().Contains("More"))
            {
                query = query.OrderByDescending(x => GetFieldValue(x.stats, firstFilter));
            }
            else
            {
                query = query.OrderBy(x => GetFieldValue(x.stats, firstFilter));
            }

            for (int i = 1; i < axieStatsFilters.Count; i++)
            {
                AxieStatFilter filter = axieStatsFilters[i];
                if (filter.ToString().Contains("More"))
                {
                    query = query.ThenByDescending(x => GetFieldValue(x.stats, filter));
                }
                else
                {
                    query = query.ThenBy(x => GetFieldValue(x.stats, filter));
                }
            }
        }

        return query.ToList();
    }

    int GetFieldValue(GetAxiesExample.Stats axieStats, AxieStatFilter filter)
    {
        // Get the filter property value
        switch (filter.ToString().Substring(4).ToLower())
        {
            case "morale":
                return axieStats.morale;
            case "skill":
                return axieStats.skill;
            case "hp":
                return axieStats.hp;
            case "speed":
                return axieStats.speed;
        }

        return 0;
    }

    public void SetAxiesMenu()
    {
        SetMenu(TeamBuilderMenu.Axies, true);
    }

    public void SetLandsMenu()
    {
        SetMenu(TeamBuilderMenu.Lands, true);
    }

    public void SetComboMenu()
    {
        if (fakeAxiesManager.instantiatedAxies.Count(x => x.renderer.enabled) == 5)
            SetMenu(TeamBuilderMenu.Combo, false);
        else
        {
            //Error Class, please choose 5 axies.
        }
    }

    public void Exit()
    {
        fakeMap.ToggleRectanglesFalse();
        CanvasTeams.SetActive(true);
        Container.SetActive(false);
    }

    public void SaveTeam()
    {
        if (string.IsNullOrEmpty(teamNameInputField.text))
        {
            NotificationErrorManager.instance.DoNotification("Please enter a name for your team.");
            return;
        }

        List<string> teamNames = TeamManager.instance.teams.Select(x => x.TeamName).ToList();
        if (teamNames.Contains(teamNameInputField.text) && creatingNewTeam)
        {
            Debug.LogError("Team name Already Exists");
            NotificationErrorManager.instance.DoNotification("Team name already Exists.");
            return;
        }

        AxieTeam newTeam = new AxieTeam();
        List<FakeAxieController> fakeAxieControllers = fakeAxiesManager.instantiatedAxies;
        newTeam.TeamName = teamNameInputField.text;
        newTeam.AxieIds = fakeAxieControllers.Select(x => new GetAxiesExample.Axie(x.axie)).ToList();

        foreach (var axie in newTeam.AxieIds)
        {
            FakeAxieController axieController = fakeAxieControllers.Single(x => x.axie.id == axie.id);
            newTeam.combos.Add(
                new Combos()
                { combos_id = axie.parts.Where(x => x.selected).Select(x => (int)x.SkillName).ToArray() }
            );
            Position pos = new Position();

            pos =
                new Position()
                {
                    col = axieController.standingOnTile.grid2DLocation.y,
                    row = axieController.standingOnTile.grid2DLocation.x
                }
                ;

            newTeam.position.Add(pos);
        }

        newTeam.landTokenId = fakeAxiesManager.fakeLandManager.currentSelectedLandId;
        newTeam.landType = fakeAxiesManager.fakeLandManager.currentLandType;
        if (!creatingNewTeam)
        {
            TeamManager.instance.teams
                [TeamManager.instance.teams.FindIndex(x => x == TeamManager.instance.currentTeam)] = newTeam;
            TeamManager.instance.currentTeam = newTeam;
        }
        else
        {
            TeamManager.instance.teams.Add(newTeam);
        }

        TeamManager.instance.SaveTeams();
        Exit();
    }

    public void SetMenu(TeamBuilderMenu teamBuilderMenu, bool ResetPage = false)
    {
        if (ResetPage)
        {
            currentPage = 1;
        }

        currentTeamBuilderMenu = teamBuilderMenu;
        switch (teamBuilderMenu)
        {
            case TeamBuilderMenu.Atia:
                SetAtiaUI();
                break;
            case TeamBuilderMenu.Axies:
                PagesAmount = AccountManager.userAxies.results.Length / 12f;
                maxPagesAmount = Mathf.CeilToInt(PagesAmount);
                pageText.text = $"Page {currentPage}-{maxPagesAmount}";
                SetAxiesUI();
                break;
            case TeamBuilderMenu.Combo:
                SetComboUI();
                break;
            case TeamBuilderMenu.Lands:
                PagesAmount = AccountManager.userLands.results.Length / 12f;
                maxPagesAmount = Mathf.CeilToInt(PagesAmount);
                pageText.text = $"Page {currentPage}-{maxPagesAmount}";
                SetLandUI();
                break;
        }
    }

    public void SetComboUI()
    {
        TeamLandBuildingUI.SetActive(false);
        ComboUI.SetActive(true);
        fakeAxieComboManager.LoadUI();
    }

    public void SetAtiaUI()
    {
    }

    public void SetAxieSelected(GetAxiesExample.Axie axie, Sprite classSprite)
    {
        AxieClassGraphic.sprite = classSprite;
        statsSkeletonGraphic.skeletonDataAsset = axie.skeletonDataAsset;
        statsSkeletonGraphic.material = axie.skeletonDataAssetMaterial;
        statsSkeletonGraphic.Initialize(true);
        lastAxieChosen = axie;
    }

    public void SetAxieStats(GetAxiesExample.Axie Axie)
    {
        AxieStatsContainer.SetActive(false);


        AxieHPText.text = Axie.stats.hp.ToString();
        AxieSkillText.text = Axie.stats.skill.ToString();
        AxieMoraleText.text = Axie.stats.morale.ToString();
        AxieSpeedText.text = Axie.stats.speed.ToString();
        statsSkeletonGraphic.skeletonDataAsset = Axie.skeletonDataAsset;
        statsSkeletonGraphic.material = Axie.skeletonDataAssetMaterial;
        statsSkeletonGraphic.Initialize(true);
        AxieStatsContainer.SetActive(true);
    }

    public void DisableAxieStats()
    {
        AxieStatsContainer.SetActive(false);
    }

    public void SetAxiesUI()
    {
        TeamLandBuildingUI.SetActive(true);
        ComboUI.SetActive(false);
        LandsContent.SetActive(false);
        AxiesContent.SetActive(true);
        if (lastAxieChosen == null)
        {
            if (TeamManager.instance.currentTeam != null)
            {
                lastAxieChosen = TeamManager.instance.currentTeam.AxieIds[0];
            }
        }

        AxieStatsContainer.SetActive(false);

        List<GetAxiesExample.Axie> filteredAxieList = GetFilteredList();

        for (int i = 0; i < 12; i++)
        {
            axieList[i].axie = null;

            int indexToSearch = Mathf.RoundToInt(i + (12 * (currentPage - 1)));
            if (indexToSearch < filteredAxieList.Count)
            {
                GetAxiesExample.Axie axie = filteredAxieList[indexToSearch];

                axieList[i].axie = axie;
                if (lastAxieChosen != null && axie.id == lastAxieChosen.id)
                {
                    AxieHPText.text = axie.stats.hp.ToString();
                    AxieSkillText.text = axie.stats.skill.ToString();
                    AxieMoraleText.text = axie.stats.morale.ToString();
                    AxieSpeedText.text = axie.stats.speed.ToString();
                    statsSkeletonGraphic.skeletonDataAsset = axieList[i].axie.skeletonDataAsset;
                    statsSkeletonGraphic.material = axieList[i].axie.skeletonDataAssetMaterial;
                    statsSkeletonGraphic.Initialize(true);
                    AxieStatsContainer.SetActive(true);
                }
            }


            axieList[i].Refresh();
        }
    }

    public void SetLandUI()
    {
        TeamLandBuildingUI.SetActive(true);
        ComboUI.SetActive(false);
        LandsContent.SetActive(true);
        AxiesContent.SetActive(false);
        for (int i = 0; i < 12; i++)
        {
            landList[i].land = null;

            if (i < AccountManager.userLands.results.Length)
            {
                GetAxiesExample.Land land = AccountManager.userLands.results[Mathf.RoundToInt(i * currentPage)];

                landList[i].land = land;
            }

            landList[i].Refresh();
        }
    }

    public void GoNextPage()
    {
        currentPage++;

        if (currentPage > maxPagesAmount)
        {
            currentPage = maxPagesAmount;
            return;
        }

        SetMenu(currentTeamBuilderMenu, false);
    }

    public void GoPreviousPage()
    {
        currentPage--;
        if (currentPage <= 0)
        {
            currentPage = 1;
            return;
        }

        SetMenu(currentTeamBuilderMenu, false);
    }
}