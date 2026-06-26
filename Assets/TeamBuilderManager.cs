using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum TeamBuilderMenu
{
    Lands,
    Monsters,
    Atia,
    Combo
}

public enum MonsterStatFilter
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
    public GameObject MonstersContent;

    public List<UIListLand> landList = new List<UIListLand>();
    public List<UIListMonster> monsterList = new List<UIListMonster>();

    public GameObject MonsterTeamUIObject;
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
    public FakeMonsterComboManager fakeMonsterComboManager;
    public FakeMonstersManager fakeMonstersManager;
    public FakeLandManager fakeLandManager;
    public GameObject CanvasTeams;
    public bool creatingNewTeam;

    public GameObject Container;

    public GameObject MonsterStatsContainer;
    public TextMeshProUGUI MonsterHPText;
    public TextMeshProUGUI MonsterMoraleText;
    public TextMeshProUGUI MonsterSkillText;
    public TextMeshProUGUI MonsterSpeedText;
    public GetMonstersExample.Monster lastMonsterChosen;
    public VanillaMonsterGraphic statsMonsterGraphic;
    public Image MonsterClassGraphic;

    public List<MonsterClass> monsterClassFilters = new List<MonsterClass>();
    public List<MonsterStatFilter> monsterStatsFilters = new List<MonsterStatFilter>();

    internal delegate void StatsFilterCleared();

    internal event StatsFilterCleared statsfilterClearedEvent;

    internal delegate void ContraryStatsFilterCleared(MonsterStatFilter statsFilter);

    internal event ContraryStatsFilterCleared contraryfilterClearedEvent;

    internal delegate void MonsterClassFilterCleared();

    internal event MonsterClassFilterCleared monsterClassfilterClearedEvent;

    public TMP_InputField GeneralFilter;

    void Start()
    {
        SetupGeneralFilterListener();
    }

    private void SetupGeneralFilterListener()
    {
        if (GeneralFilter != null)
        {
            GeneralFilter.onEndEdit.RemoveAllListeners();
            GeneralFilter.onEndEdit.AddListener(delegate { OnGeneralFilterChanged(); });
        }
    }
    private void OnGeneralFilterChanged()
    {
        SetMonstersUI();
    }

    public void Edit()
    {
        fakeMap.ToggleRectangles();
        Container.SetActive(true);
        CanvasTeams.SetActive(false);
        creatingNewTeam = false;
        teamNameInputField.text = TeamManager.instance.currentTeam.TeamName;
        SetMenu(TeamBuilderMenu.Monsters, true);
    }

    public void NewTeam()
    {
        CanvasTeams.SetActive(false);
        fakeLandManager.ChooseFakeLand(AccountManager.userLands.results[0].token_id);
        teamNameInputField.text = "";
        fakeMap.ToggleRectangles();
        Container.SetActive(true);
        creatingNewTeam = true;
        fakeMonstersManager.ClearAllMonsters();
        SetMenu(TeamBuilderMenu.Lands, true);
    }

    public void ToggleClassFilter(MonsterClass monsterClass)
    {
        if (monsterClassFilters.Any(x => x == monsterClass))
        {
            monsterClassFilters.RemoveAll(x => x == monsterClass);
        }
        else
        {
            monsterClassFilters.Add(monsterClass);
        }

        SetMonstersUI();
    }

    public void RemoveAllClassFilter()
    {
        monsterClassFilters.Clear();
        monsterClassfilterClearedEvent.Invoke();
        SetMonstersUI();
    }

    public void RemoveAllStatsFilter()
    {
        monsterStatsFilters.Clear();
        statsfilterClearedEvent.Invoke();
        SetMonstersUI();
    }

    public void ToggleStatsFilter(MonsterStatFilter statFilter)
    {
        MonsterStatFilter contraryFilter = monsterStatsFilters.FirstOrDefault(x =>
            x != statFilter && statFilter.ToString().Contains(x.ToString().Substring(4)));

        if (contraryFilter != MonsterStatFilter.NoFilter)
        {
            monsterStatsFilters.RemoveAll(x => x == contraryFilter);
            contraryfilterClearedEvent.Invoke(contraryFilter);
        }

        if (monsterStatsFilters.Any(x => x == statFilter && monsterStatsFilters.Contains(statFilter)))
        {
            monsterStatsFilters.RemoveAll(x => x == statFilter);
        }
        else
        {
            monsterStatsFilters.Add(statFilter);
        }

        SetMonstersUI();
    }

    public List<GetMonstersExample.Monster> GetFilteredList(string generalFilter = null)
    {
        List<GetMonstersExample.Monster> monstersList = AccountManager.userMonsters.results.ToList();

        // Apply existing class and stats filters
        if (monsterClassFilters.Count != 0)
        {
            monstersList = monstersList.Where(x => monsterClassFilters.Contains(x.monsterClass)).ToList();
        }

        if (monsterStatsFilters.Count != 0)
        {
            var query = SortMonsters(monstersList, monsterStatsFilters);
            monstersList = query.ToList();
        }

        // New filtering based on the GeneralFilter
        if (!string.IsNullOrEmpty(generalFilter))
        {
            var filters = generalFilter.ToLower().Split(',').Select(f => f.Trim()).ToList();
            monstersList = monstersList.Where(monster =>
                filters.Any(filter =>
                    (monster.id?.ToLower() == filter) ||
                    (monster.monsterClass.ToString().ToLower() == filter) ||
                    (monster.bodyShape.ToString().ToLower() == filter) ||
                    (monster.parts != null && monster.parts.Any(part =>
                        part != null && (
                            part.id?.ToLower() == filter ||
                            part.name?.ToLower() == filter ||
                            part.@class?.ToLower() == filter ||
                            part.type?.ToLower() == filter ||
                            part.abilityName?.ToLower() == filter ||
                            part.partClass.ToString().ToLower() == filter ||
                            part.BodyPart.ToString().ToLower() == filter ||
                            part.SkillName.ToString().ToLower() == filter
                        )
                    ))
                )
            ).ToList();
        }

        return monstersList;
    }


    private IOrderedEnumerable<GetMonstersExample.Monster> SortMonsters(List<GetMonstersExample.Monster> monstersList, List<MonsterStatFilter> monsterStatsFilters)
    {
        IOrderedEnumerable<GetMonstersExample.Monster> query = monstersList.OrderBy(x => 0);
        MonsterStatFilter firstFilter = monsterStatsFilters.First();
        if (firstFilter.ToString().Contains("More"))
        {
            query = query.OrderByDescending(x => GetFieldValue(x.stats, firstFilter));
        }
        else
        {
            query = query.OrderBy(x => GetFieldValue(x.stats, firstFilter));
        }

        for (int i = 1; i < monsterStatsFilters.Count; i++)
        {
            MonsterStatFilter filter = monsterStatsFilters[i];
            if (filter.ToString().Contains("More"))
            {
                query = query.ThenByDescending(x => GetFieldValue(x.stats, filter));
            }
            else
            {
                query = query.ThenBy(x => GetFieldValue(x.stats, filter));
            }
        }

        return query;
    }

    int GetFieldValue(GetMonstersExample.Stats monsterStats, MonsterStatFilter filter)
    {
        // Get the filter property value
        switch (filter.ToString().Substring(4).ToLower())
        {
            case "morale":
                return monsterStats.morale;
            case "skill":
                return monsterStats.skill;
            case "hp":
                return monsterStats.hp;
            case "speed":
                return monsterStats.speed;
        }

        return 0;
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
            case TeamBuilderMenu.Monsters:
                PagesAmount = AccountManager.userMonsters.results.Length / 12f;
                maxPagesAmount = Mathf.CeilToInt(PagesAmount);
                pageText.text = $"Page {currentPage}-{maxPagesAmount}";
                SetMonstersUI();
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
        fakeMonsterComboManager.LoadUI();
    }

    public void SetAtiaUI()
    {
    }

    public void SetMonsterSelected(GetMonstersExample.Monster monster, MonsterClass monsterClass)
    {
        VanillaMonsterIconUtility.ApplyClass(MonsterClassGraphic, monsterClass, GetMonsterClassGraphics());
        SetStatsGraphic(monster);
        lastMonsterChosen = monster;
    }

    public void SetMonsterStats(GetMonstersExample.Monster Monster)
    {
        MonsterStatsContainer.SetActive(false);

        MonsterHPText.text = Monster.stats.hp.ToString();
        MonsterSkillText.text = Monster.stats.skill.ToString();
        MonsterMoraleText.text = Monster.stats.morale.ToString();
        MonsterSpeedText.text = Monster.stats.speed.ToString();
        SetStatsGraphic(Monster);
        MonsterStatsContainer.SetActive(true);
    }

    public void DisableMonsterStats()
    {
        MonsterStatsContainer.SetActive(false);
    }

    private List<MonsterClassGraphic> GetMonsterClassGraphics()
    {
        foreach (UIListMonster item in monsterList)
        {
            if (item != null && item.monsterClassGraphics != null && item.monsterClassGraphics.Count > 0)
                return item.monsterClassGraphics;
        }

        return null;
    }

    public void SetMonstersUI()
    {
        TeamLandBuildingUI.SetActive(true);
        ComboUI.SetActive(false);
        LandsContent.SetActive(false);
        MonstersContent.SetActive(true);
        if (lastMonsterChosen == null)
        {
            if (TeamManager.instance.currentTeam != null)
            {
                lastMonsterChosen = TeamManager.instance.currentTeam.MonsterIds[0];
            }
        }

        MonsterStatsContainer.SetActive(false);

        List<GetMonstersExample.Monster> filteredMonsterList = GetFilteredList(GeneralFilter.text);

        for (int i = 0; i < 12; i++)
        {
            monsterList[i].monster = null;

            int indexToSearch = Mathf.RoundToInt(i + (12 * (currentPage - 1)));
            if (indexToSearch < filteredMonsterList.Count)
            {
                GetMonstersExample.Monster monster = filteredMonsterList[indexToSearch];

                monsterList[i].monster = monster;
                if (lastMonsterChosen != null && monster.id == lastMonsterChosen.id)
                {
                    MonsterHPText.text = monster.stats.hp.ToString();
                    MonsterSkillText.text = monster.stats.skill.ToString();
                    MonsterMoraleText.text = monster.stats.morale.ToString();
                    MonsterSpeedText.text = monster.stats.speed.ToString();
                    SetStatsGraphic(monsterList[i].monster);
                    MonsterStatsContainer.SetActive(true);
                }
            }

            monsterList[i].Refresh();
        }
    }

    public void SetLandUI()
    {
        TeamLandBuildingUI.SetActive(true);
        ComboUI.SetActive(false);
        LandsContent.SetActive(true);
        MonstersContent.SetActive(false);
        for (int i = 0; i < 12; i++)
        {
            landList[i].land = null;

            if (i < AccountManager.userLands.results.Length)
            {
                GetMonstersExample.Land land = AccountManager.userLands.results[Mathf.RoundToInt(i * currentPage)];

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

    public void SetMonstersMenu()
    {
        SetMenu(TeamBuilderMenu.Monsters, true);
    }

    public void SetLandsMenu()
    {
        SetMenu(TeamBuilderMenu.Lands, true);
    }

    public void SetComboMenu()
    {
        if (fakeMonstersManager.instantiatedMonsters.Count(x => x.visible) == 5)
            SetMenu(TeamBuilderMenu.Combo, false);
        else
        {
            NotificationErrorManager.instance.DoNotification("Please select 5 mons first");
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

        MonsterTeam newTeam = new MonsterTeam();
        List<FakeMonsterController> fakeMonsterControllers = fakeMonstersManager.instantiatedMonsters;
        newTeam.TeamName = teamNameInputField.text;
        newTeam.MonsterIds = fakeMonsterControllers.Select(x => new GetMonstersExample.Monster(x.monster)).ToList();

        foreach (var monster in newTeam.MonsterIds)
        {
            FakeMonsterController monsterController = fakeMonsterControllers.Single(x => x.monster.id == monster.id);
            newTeam.combos.Add(
                new Combos()
                { combos_id = monster.parts.Where(x => x.selected).Select(x => (int)x.SkillName).ToArray() }
            );
            Position pos = new Position();

            pos =
                new Position()
                {
                    col = monsterController.standingOnTile.grid2DLocation.y,
                    row = monsterController.standingOnTile.grid2DLocation.x
                }
                ;

            newTeam.position.Add(pos);
        }

        newTeam.landTokenId = fakeMonstersManager.fakeLandManager.currentSelectedLandId;
        newTeam.landType = fakeMonstersManager.fakeLandManager.currentLandType;
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

    private void SetStatsGraphic(GetMonstersExample.Monster monster)
    {
        VanillaMonsterGraphic graphic = EnsureStatsGraphic();
        graphic.SetMonster(monster);
        graphic.Initialize(true);
    }

    private VanillaMonsterGraphic EnsureStatsGraphic()
    {
        statsMonsterGraphic = VanillaMonsterGraphic.EnsureCenteredChild(MonsterStatsContainer.transform, statsMonsterGraphic, "Stats Monster Graphic");
        return statsMonsterGraphic;
    }

}
