using System.Collections;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GetMonstersExample;

[System.Serializable]
public class MonsterViewCombo
{
    public TextMeshProUGUI EnergyCount;
    public TextMeshProUGUI Attack;
    public TextMeshProUGUI Shield;
    public TextMeshProUGUI Description;
    public TextMeshProUGUI Title;
    public Image MonsterClass;
    public BodyPart BodyPart;
    public VFXPlayer vfxPlayer;
    public GameObject PassiveGO;
    public GameObject ActiveGO;
}

public class MonstersView : MonoBehaviour
{
    private const string StatsGraphicName = "Stats Monster Graphic";
    private const string LegacyStatsGraphicName = "Monster";

    public MonsterBodyPartsManager skillList;
    public List<UIListMonsterForView> monsterList = new List<UIListMonsterForView>();
    public List<MonsterViewCombo> monsterViewCombo;
    public GameObject MonsterStatsContainer;
    public TextMeshProUGUI MonsterHPText;
    public TextMeshProUGUI MonsterMoraleText;
    public TextMeshProUGUI MonsterSkillText;
    public TextMeshProUGUI MonsterSpeedText;
    public GetMonstersExample.Monster lastMonsterChosen;
    public VanillaMonsterGraphic statsMonsterGraphic;
    public Image MonsterClassGraphic;
    private int currentPage = 1;
    float PagesAmount = 0;
    int maxPagesAmount = 0;
    public string selectedMonster;
    public TextMeshProUGUI pageText;
    public List<MonsterClass> monsterClassFilters = new List<MonsterClass>();
    public List<MonsterStatFilter> monsterStatsFilters = new List<MonsterStatFilter>();

    // General text filter InputField
    public TMP_InputField GeneralFilter;

    internal delegate void StatsFilterCleared();

    internal event StatsFilterCleared statsfilterClearedEvent;

    internal delegate void ContraryStatsFilterCleared(MonsterStatFilter statsFilter);

    internal event ContraryStatsFilterCleared contraryfilterClearedEvent;

    internal delegate void MonsterClassFilterCleared();

    internal event MonsterClassFilterCleared monsterClassfilterClearedEvent;

    private void Start()
    {
        SetMonstersUI();
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
        ResetPages();
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

        ResetPages();
    }

    public void RemoveAllClassFilter()
    {
        monsterClassFilters.Clear();
        if (monsterClassfilterClearedEvent != null)
        {
            monsterClassfilterClearedEvent.Invoke();
        }
        ResetPages();
    }

    public void RemoveAllStatsFilter()
    {
        monsterStatsFilters.Clear();
        if (statsfilterClearedEvent != null)
            statsfilterClearedEvent.Invoke();
        ResetPages();
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

        ResetPages();
    }

    public List<GetMonstersExample.Monster> GetFilteredList(string generalFilter = null)
    {
        List<GetMonstersExample.Monster> monstersList = AccountManager.userMonsters.results.ToList();

        // Apply class filters
        if (monsterClassFilters.Count != 0)
        {
            monstersList = monstersList.Where(x => monsterClassFilters.Contains(x.monsterClass)).ToList();
        }

        // Apply stat filters
        if (monsterStatsFilters.Count != 0)
        {
            var query = SortMonsters(monstersList, monsterStatsFilters);
            monstersList = query.ToList();
        }

        // Apply general text filter
        if (!string.IsNullOrEmpty(generalFilter))
        {
            var filters = generalFilter.ToLower().Split(',').Select(f => f.Trim()).ToList();
            monstersList = monstersList.Where(monster =>
                filters.Any(filter =>
                    (monster.id?.ToLower().Contains(filter) == true) ||
                    (monster.monsterClass.ToString().ToLower().Contains(filter)) ||
                    (monster.bodyShape.ToString().ToLower().Contains(filter)) ||
                    (monster.parts != null && monster.parts.Any(part =>
                        part != null && (
                            part.id?.ToLower().Contains(filter) == true ||
                            part.name?.ToLower().Contains(filter) == true ||
                            part.@class?.ToLower().Contains(filter) == true ||
                            part.type?.ToLower().Contains(filter) == true ||
                            part.abilityName?.ToLower().Contains(filter) == true ||
                            part.partClass.ToString().ToLower().Contains(filter) ||
                            part.BodyPart.ToString().ToLower().Contains(filter) ||
                            part.SkillName.ToString().ToLower().Contains(filter)
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
        foreach (UIListMonsterForView item in monsterList)
        {
            if (item != null && item.monsterClassGraphics != null && item.monsterClassGraphics.Count > 0)
                return item.monsterClassGraphics;
        }

        return null;
    }

    public void SetMonsterSelected(GetMonstersExample.Monster monster, MonsterClass monsterClass)
    {
        selectedMonster = monster.id;
        foreach (var item in monsterList)
        {
            item.Refresh();
        }

        VanillaMonsterIconUtility.ApplyClass(MonsterClassGraphic, monsterClass, GetMonsterClassGraphics());
        SetStatsGraphic(monster);
        lastMonsterChosen = monster;

        foreach (var monsterUICombo in monsterViewCombo)
        {
            var ability = skillList.monsterBodyParts.Single(x => x.bodyPart == monsterUICombo.BodyPart && x.skillName == monster.parts.FirstOrDefault(x => x.BodyPart == monsterUICombo.BodyPart).SkillName);

            monsterUICombo.Description.text = ability.description;
            monsterUICombo.Description.GetComponent<AbilityDescriptionTooltip>().SetTooltips(ability.tooltipTypes);
            monsterUICombo.Title.text = MonsterGeneUtils.SpaceCamelCase(ability.skillName.ToString().Replace("_", " "));
            VanillaMonsterIconUtility.ApplyClass(monsterUICombo.MonsterClass, monsterClass, GetMonsterClassGraphics());
            if (ability.isPassive)
            {
                monsterUICombo.ActiveGO.SetActive(false);
                monsterUICombo.PassiveGO.SetActive(true);
            }
            else
            {
                monsterUICombo.ActiveGO.SetActive(true);
                monsterUICombo.PassiveGO.SetActive(false);
                monsterUICombo.EnergyCount.text = ability.energy.ToString();
                monsterUICombo.Attack.text = ability.damage.ToString();
                monsterUICombo.Shield.text = ability.shield.ToString();
            }
            StartCoroutine(SetCards(ability));
        }
    }

    IEnumerator SetCards(MonsterBodyPart ability)
    {
        var monsterUICombo = monsterViewCombo.Single(x => x.BodyPart == ability.bodyPart);
        monsterUICombo.vfxPlayer.SetUp(lastMonsterChosen.visualDescriptor ?? MonsterVisualDescriptor.FromMonster(lastMonsterChosen));
        yield return new WaitForFixedUpdate();
        StopAnimation(ability.bodyPart);
    }

    public void PlayAnimation(BodyPart bodyPart)
    {
        var monsterUICombo = monsterViewCombo.Single(x => x.BodyPart == bodyPart);
        var ability = skillList.monsterBodyParts.Single(x => x.bodyPart == monsterUICombo.BodyPart && x.skillName == lastMonsterChosen.parts.FirstOrDefault(x => x.BodyPart == monsterUICombo.BodyPart).SkillName);
        monsterUICombo.vfxPlayer.PlayMonsterVFX(ability.skillName, monsterUICombo.BodyPart, lastMonsterChosen.visualDescriptor ?? MonsterVisualDescriptor.FromMonster(lastMonsterChosen));
    }

    public void StopAnimation(BodyPart bodyPart)
    {
        var monsterUICombo = monsterViewCombo.Single(x => x.BodyPart == bodyPart);
        monsterUICombo.vfxPlayer.StopVFX();
    }

    public void SetOtherMonsterSelected()
    {
        MonsterStatsContainer.SetActive(false);
        for (int i = 0; i < 12; i++)
        {
            int indexToSearch = Mathf.RoundToInt(i + (12 * (currentPage - 1)));
            if (indexToSearch < AccountManager.userMonsters.results.Length)
            {
                if (monsterList[i].selected)
                {
                    MonsterHPText.text = monsterList[i].monster.stats.hp.ToString();
                    MonsterSkillText.text = monsterList[i].monster.stats.skill.ToString();
                    MonsterMoraleText.text = monsterList[i].monster.stats.morale.ToString();
                    MonsterSpeedText.text = monsterList[i].monster.stats.speed.ToString();
                    SetStatsGraphic(monsterList[i].monster);
                    MonsterStatsContainer.SetActive(true);
                    return;
                }
            }
        }
    }

    public void SetMonstersUI()
    {
        PagesAmount = AccountManager.userMonsters.results.Length / 12f;
        maxPagesAmount = Mathf.CeilToInt(PagesAmount);
        pageText.text = $"Page {currentPage}-{maxPagesAmount}";
        if (lastMonsterChosen == null)
        {
            if (TeamManager.instance.currentTeam != null)
            {
                lastMonsterChosen = TeamManager.instance.currentTeam.MonsterIds[0];
            }
        }

        MonsterStatsContainer.SetActive(false);

        List<GetMonstersExample.Monster> filteredMonsterList = GetFilteredList(GeneralFilter?.text);

        for (int i = 0; i < 12; i++)
        {
            monsterList[i].monster = null;

            int indexToSearch = Mathf.RoundToInt(i + ((12 * (currentPage == 0 ? 0 : currentPage - 1))));
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

            monsterList[i].Refresh(true);
        }

        if (string.IsNullOrEmpty(selectedMonster))
            monsterList.First().SelectMonster();
    }

    private void OnDisable()
    {
        selectedMonster = "";
    }

    public void GoNextPage()
    {
        currentPage++;

        if (currentPage > maxPagesAmount)
        {
            currentPage = maxPagesAmount;
            return;
        }

        SetMonstersUI();
    }

    public void GoPreviousPage()
    {
        currentPage--;
        if (currentPage <= 0)
        {
            currentPage = 1;
            return;
        }

        SetMonstersUI();
    }

    public void SelectMonsterById(string monsterId)
    {
        // Remove all filters
        RemoveAllClassFilter();
        RemoveAllStatsFilter();
        GeneralFilter.text = string.Empty;

        // Navigate through the pages to find the Monster
        for (currentPage = 1; currentPage <= maxPagesAmount; currentPage++)
        {
            SetMonstersUI();
            var targetMonster = monsterList.FirstOrDefault(monsterView => monsterView.monster != null && monsterView.monster.id == monsterId);

            if (targetMonster != null)
            {
                targetMonster.SelectMonster();
                break;
            }
        }
    }


    public void ResetPages()
    {
        currentPage = 1;
        SetMonstersUI();
    }

    private void SetStatsGraphic(GetMonstersExample.Monster monster)
    {
        VanillaMonsterGraphic graphic = EnsureStatsGraphic();
        graphic.SetMonster(monster);
        graphic.Initialize(true);
    }

    private VanillaMonsterGraphic EnsureStatsGraphic()
    {
        statsMonsterGraphic = VanillaMonsterGraphic.EnsureExistingChildOrCentered(
            MonsterStatsContainer.transform,
            statsMonsterGraphic,
            StatsGraphicName,
            LegacyStatsGraphicName);
        return statsMonsterGraphic;
    }
}
