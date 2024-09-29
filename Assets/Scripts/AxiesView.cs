using Spine.Unity;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GetAxiesExample;
using UnityEditor;

[System.Serializable]
public class AxieViewCombo
{
    public TextMeshProUGUI EnergyCount;
    public TextMeshProUGUI Attack;
    public TextMeshProUGUI Shield;
    public TextMeshProUGUI Description;
    public TextMeshProUGUI Title;
    public Image AxieClass;
    public BodyPart BodyPart;
    public VFXPlayer vfxPlayer;
    public GameObject PassiveGO;
    public GameObject ActiveGO;
}

public class AxiesView : MonoBehaviour
{
    public AxieBodyPartsManager skillList;
    public List<UIListAxieForView> axieList = new List<UIListAxieForView>();
    public List<AxieViewCombo> axieViewCombo;
    public GameObject AxieStatsContainer;
    public TextMeshProUGUI AxieHPText;
    public TextMeshProUGUI AxieMoraleText;
    public TextMeshProUGUI AxieSkillText;
    public TextMeshProUGUI AxieSpeedText;
    public GetAxiesExample.Axie lastAxieChosen;
    public SkeletonGraphic statsSkeletonGraphic;
    public Image AxieClassGraphic;
    private int currentPage = 1;
    float PagesAmount = 0;
    int maxPagesAmount = 0;
    public string selectedAxie;
    public TextMeshProUGUI pageText;
    public List<AxieClass> axieClassFilters = new List<AxieClass>();
    public List<AxieStatFilter> axieStatsFilters = new List<AxieStatFilter>();

    // General text filter InputField
    public TMP_InputField GeneralFilter;

    internal delegate void StatsFilterCleared();

    internal event StatsFilterCleared statsfilterClearedEvent;

    internal delegate void ContraryStatsFilterCleared(AxieStatFilter statsFilter);

    internal event ContraryStatsFilterCleared contraryfilterClearedEvent;

    internal delegate void AxieClassFilterCleared();

    internal event AxieClassFilterCleared axieClassfilterClearedEvent;

    private void Start()
    {
        SetAxiesUI();
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

        ResetPages();
    }

    public void RemoveAllClassFilter()
    {
        axieClassFilters.Clear();
        axieClassfilterClearedEvent.Invoke();
        ResetPages();
    }

    public void RemoveAllStatsFilter()
    {
        axieStatsFilters.Clear();
        statsfilterClearedEvent.Invoke();
        ResetPages();
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

        ResetPages();
    }

    public List<GetAxiesExample.Axie> GetFilteredList(string generalFilter = null)
    {
        List<GetAxiesExample.Axie> axiesList = AccountManager.userAxies.results.ToList();

        // Apply class filters
        if (axieClassFilters.Count != 0)
        {
            axiesList = axiesList.Where(x => axieClassFilters.Contains(x.axieClass)).ToList();
        }

        // Apply stat filters
        if (axieStatsFilters.Count != 0)
        {
            var query = SortAxies(axiesList, axieStatsFilters);
            axiesList = query.ToList();
        }

        // Apply general text filter
        if (!string.IsNullOrEmpty(generalFilter))
        {
            var filters = generalFilter.ToLower().Split(',').Select(f => f.Trim()).ToList();
            axiesList = axiesList.Where(axie =>
                filters.Any(filter =>
                    (axie.id?.ToLower().Contains(filter) == true) ||
                    (axie.axieClass.ToString().ToLower().Contains(filter)) ||
                    (axie.bodyShape.ToString().ToLower().Contains(filter)) ||
                    (axie.parts != null && axie.parts.Any(part =>
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

        return axiesList;
    }

    private IOrderedEnumerable<GetAxiesExample.Axie> SortAxies(List<GetAxiesExample.Axie> axiesList, List<AxieStatFilter> axieStatsFilters)
    {
        IOrderedEnumerable<GetAxiesExample.Axie> query = axiesList.OrderBy(x => 0);
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

        return query;
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

    public void SetAxieSelected(GetAxiesExample.Axie axie, Sprite classSprite)
    {
        selectedAxie = axie.id;
        foreach (var item in axieList)
        {
            item.Refresh();
        }

        AxieClassGraphic.sprite = classSprite;
        statsSkeletonGraphic.skeletonDataAsset = axie.skeletonDataAsset;
        statsSkeletonGraphic.material = axie.skeletonDataAssetMaterial;
        statsSkeletonGraphic.Initialize(true);
        lastAxieChosen = axie;

        foreach (var axieUICombo in axieViewCombo)
        {
            var ability = skillList.axieBodyParts.Single(x => x.bodyPart == axieUICombo.BodyPart && x.skillName == axie.parts.FirstOrDefault(x => x.BodyPart == axieUICombo.BodyPart).SkillName);

            axieUICombo.Description.text = ability.description;
            axieUICombo.Description.GetComponent<AbilityDescriptionTooltip>().SetTooltips(ability.tooltipTypes);
            axieUICombo.Title.text = ability.skillName.ToString().Replace("_", " ");
            axieUICombo.AxieClass.sprite = classSprite;
            if (ability.isPassive)
            {
                axieUICombo.ActiveGO.SetActive(false);
                axieUICombo.PassiveGO.SetActive(true);
            }
            else
            {
                axieUICombo.ActiveGO.SetActive(true);
                axieUICombo.PassiveGO.SetActive(false);
                axieUICombo.EnergyCount.text = ability.energy.ToString();
                axieUICombo.Attack.text = ability.damage.ToString();
                axieUICombo.Shield.text = ability.shield.ToString();
            }
            StartCoroutine(SetCards(ability));
        }
    }

    IEnumerator SetCards(AxieBodyPart ability)
    {
        var axieUICombo = axieViewCombo.Single(x => x.BodyPart == ability.bodyPart);
        axieUICombo.vfxPlayer.SetUp(lastAxieChosen.skeletonDataAsset);
        yield return new WaitForFixedUpdate();
        StopAnimation(ability.bodyPart);
    }

    public void PlayAnimation(BodyPart bodyPart)
    {
        var axieUICombo = axieViewCombo.Single(x => x.BodyPart == bodyPart);
        var ability = skillList.axieBodyParts.Single(x => x.bodyPart == axieUICombo.BodyPart && x.skillName == lastAxieChosen.parts.FirstOrDefault(x => x.BodyPart == axieUICombo.BodyPart).SkillName);
        axieUICombo.vfxPlayer.PlayAxieVFX(ability.skillName, axieUICombo.BodyPart, lastAxieChosen.skeletonDataAsset);
    }

    public void StopAnimation(BodyPart bodyPart)
    {
        var axieUICombo = axieViewCombo.Single(x => x.BodyPart == bodyPart);
        axieUICombo.vfxPlayer.StopVFX();
    }

    public void SetOtherAxieSelected()
    {
        AxieStatsContainer.SetActive(false);
        for (int i = 0; i < 12; i++)
        {
            int indexToSearch = Mathf.RoundToInt(i + (12 * (currentPage - 1)));
            if (indexToSearch < AccountManager.userAxies.results.Length)
            {
                if (axieList[i].selected)
                {
                    AxieHPText.text = axieList[i].axie.stats.hp.ToString();
                    AxieSkillText.text = axieList[i].axie.stats.skill.ToString();
                    AxieMoraleText.text = axieList[i].axie.stats.morale.ToString();
                    AxieSpeedText.text = axieList[i].axie.stats.speed.ToString();
                    statsSkeletonGraphic.skeletonDataAsset = axieList[i].axie.skeletonDataAsset;
                    statsSkeletonGraphic.material = axieList[i].axie.skeletonDataAssetMaterial;
                    statsSkeletonGraphic.Initialize(true);
                    AxieStatsContainer.SetActive(true);
                    return;
                }
            }
        }
    }

    public void SetAxiesUI()
    {
        PagesAmount = AccountManager.userAxies.results.Length / 12f;
        maxPagesAmount = Mathf.CeilToInt(PagesAmount);
        pageText.text = $"Page {currentPage}-{maxPagesAmount}";
        if (lastAxieChosen == null)
        {
            if (TeamManager.instance.currentTeam != null)
            {
                lastAxieChosen = TeamManager.instance.currentTeam.AxieIds[0];
            }
        }

        AxieStatsContainer.SetActive(false);

        List<GetAxiesExample.Axie> filteredAxieList = GetFilteredList(GeneralFilter?.text);

        for (int i = 0; i < 12; i++)
        {
            axieList[i].axie = null;

            int indexToSearch = Mathf.RoundToInt(i + ((12 * (currentPage == 0 ? 0 : currentPage - 1))));
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

            axieList[i].Refresh(true);
        }

        if (string.IsNullOrEmpty(selectedAxie))
            axieList.First().SelectAxie();
    }

    private void OnDisable()
    {
        selectedAxie = "";
    }

    public void GoNextPage()
    {
        currentPage++;

        if (currentPage > maxPagesAmount)
        {
            currentPage = maxPagesAmount;
            return;
        }

        SetAxiesUI();
    }

    public void GoPreviousPage()
    {
        currentPage--;
        if (currentPage <= 0)
        {
            currentPage = 1;
            return;
        }

        SetAxiesUI();
    }

    public void ResetPages()
    {
        currentPage = 1;
        SetAxiesUI();
    }
}
