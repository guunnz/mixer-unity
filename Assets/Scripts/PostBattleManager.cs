using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AxieForPostBattle
{
    public float Healing;
    public float Damage;
    public SkeletonDataAsset skeletonDataAsset;
    public Material skeletonMaterial;
    public string axieId;
    public List<AxieBodyPart> axieParts;
}

public class PostBattleManager : MonoBehaviour
{
    public List<AxieForPostBattle> AlliedAxieForPostBattles = new List<AxieForPostBattle>();
    public List<AxieForPostBattle> EnemyAxieForPostBattles = new List<AxieForPostBattle>();
    public TextMeshProUGUI ToggleText;
    public bool DamageMode;
    public Color colorToUse = Color.red;
    public List<PostBattleAxie> AlliedPostBattleAxies = new List<PostBattleAxie>();
    public List<PostBattleAxie> EnemyPostBattleAxies = new List<PostBattleAxie>();
    static public PostBattleManager Instance;
    public GameObject Container;

    public void Show()
    {
        TooltipManagerSingleton.instance.DisableTooltip();
        Container.SetActive(true);
        LoadPostBattleAxies();
    }

    private void Awake()
    {
        Instance = this;
    }

    public void ToggleMode()
    {
        DamageMode = !DamageMode;
        if (DamageMode)
        {
            colorToUse = Color.red;
            ToggleText.text = "Change to Healing Stats";
        }
        else
        {
            colorToUse = Color.green;
            ToggleText.text = "Change to Damage Stats";
        }
        LoadPostBattleAxies();
    }

    public void SumDamage(string AxieId, float damage, bool good)
    {
        try
        {
            if (good)
            {
                AlliedAxieForPostBattles.FirstOrDefault(x => x.axieId == AxieId).Damage += damage;
            }
            else
            {
                EnemyAxieForPostBattles.FirstOrDefault(x => x.axieId == AxieId).Damage += damage;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

    }

    public void SumHealing(string AxieId, float healing, bool good)
    {
        if (good)
        {
            AlliedAxieForPostBattles.FirstOrDefault(x => x.axieId == AxieId).Healing += healing;
        }
        else
        {
            EnemyAxieForPostBattles.FirstOrDefault(x => x.axieId == AxieId).Healing += healing;
        }
    }

    public void LoadPostBattleAxies()
    {
        var alliedAxieOrderedList = AlliedAxieForPostBattles.OrderByDescending(x => DamageMode ? x.Damage : x.Healing).ToList();
        var enemyAxiesOrderedList = EnemyAxieForPostBattles.OrderByDescending(x => DamageMode ? x.Damage : x.Healing).ToList();

        var combinedList = new List<AxieForPostBattle>();

        combinedList.AddRange(alliedAxieOrderedList);
        combinedList.AddRange(enemyAxiesOrderedList);

        float maxFillAmount = enemyAxiesOrderedList.Max(x => DamageMode ? x.Damage : x.Healing);

        for (int i = 0; i < 5; i++)
        {
            var alliedAxieData = alliedAxieOrderedList[i];
            var enemyAxieData = enemyAxiesOrderedList[i];
            AlliedPostBattleAxies[i].SetPostBattleAxie(alliedAxieData.skeletonDataAsset, alliedAxieData.skeletonMaterial, colorToUse, DamageMode ? alliedAxieData.Damage / maxFillAmount : alliedAxieData.Healing / maxFillAmount, DamageMode ? alliedAxieData.Damage : alliedAxieData.Healing, alliedAxieData.axieParts);
            EnemyPostBattleAxies[i].SetPostBattleAxie(enemyAxieData.skeletonDataAsset, enemyAxieData.skeletonMaterial, colorToUse, DamageMode ? enemyAxieData.Damage / maxFillAmount : enemyAxieData.Healing / maxFillAmount, DamageMode ? enemyAxieData.Damage : enemyAxieData.Healing, enemyAxieData.axieParts);
        }
    }

    public void FillList(Team team)
    {
        if (team.isGoodTeam)
        {
            AlliedAxieForPostBattles.Clear();
            foreach (var axie in team.GetCharactersAll())
            {
                AlliedAxieForPostBattles.Add(new AxieForPostBattle()
                {
                    axieId = axie.AxieId.ToString(),
                    skeletonDataAsset = axie.skeletonDataAsset,
                    skeletonMaterial = axie.skeletonMaterial,
                    axieParts = axie.axieSkillController.skillListNoRepeat.Select(x => x.bodyPartSO).ToList()
                });
            }
            return;
        }

        EnemyAxieForPostBattles.Clear();

        foreach (var axie in team.GetCharactersAll())
        {
            EnemyAxieForPostBattles.Add(new AxieForPostBattle()
            {
                axieId = axie.AxieId.ToString(),
                skeletonDataAsset = axie.skeletonDataAsset,
                skeletonMaterial = axie.skeletonMaterial,
                axieParts = axie.axieSkillController.skillListNoRepeat.Select(x => x.bodyPartSO).ToList()
            });
        }
    }
}
