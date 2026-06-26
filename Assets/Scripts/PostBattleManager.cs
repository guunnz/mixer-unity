using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonsterForPostBattle
{
    public float Healing;
    public float Damage;
    public MonsterVisualDescriptor visualDescriptor;
    public string monsterId;
    public List<MonsterBodyPart> monsterParts;
}

public class PostBattleManager : MonoBehaviour
{
    public List<MonsterForPostBattle> AlliedMonsterForPostBattles = new List<MonsterForPostBattle>();
    public List<MonsterForPostBattle> EnemyMonsterForPostBattles = new List<MonsterForPostBattle>();
    public TextMeshProUGUI ToggleText;
    public bool DamageMode;
    public Color colorToUse = Color.red;
    public List<PostBattleMonster> AlliedPostBattleMonsters = new List<PostBattleMonster>();
    public List<PostBattleMonster> EnemyPostBattleMonsters = new List<PostBattleMonster>();
    static public PostBattleManager Instance;
    public GameObject Container;

    public void Show()
    {
        TooltipManagerSingleton.instance.DisableTooltip();
        Container.SetActive(true);
        LoadPostBattleMonsters();
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
        LoadPostBattleMonsters();
    }

    public void SumDamage(string MonsterId, float damage, bool good)
    {
        try
        {
            if (good)
            {
                AlliedMonsterForPostBattles.FirstOrDefault(x => x.monsterId == MonsterId).Damage += damage;
            }
            else
            {
                EnemyMonsterForPostBattles.FirstOrDefault(x => x.monsterId == MonsterId).Damage += damage;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }

    }

    public void SumHealing(string MonsterId, float healing, bool good)
    {
        if (good)
        {
            AlliedMonsterForPostBattles.FirstOrDefault(x => x.monsterId == MonsterId).Healing += healing;
        }
        else
        {
            EnemyMonsterForPostBattles.FirstOrDefault(x => x.monsterId == MonsterId).Healing += healing;
        }
    }

    public void LoadPostBattleMonsters()
    {
        var alliedMonsterOrderedList = AlliedMonsterForPostBattles.OrderByDescending(x => DamageMode ? x.Damage : x.Healing).ToList();
        var enemyMonstersOrderedList = EnemyMonsterForPostBattles.OrderByDescending(x => DamageMode ? x.Damage : x.Healing).ToList();

        var combinedList = new List<MonsterForPostBattle>();

        combinedList.AddRange(alliedMonsterOrderedList);
        combinedList.AddRange(enemyMonstersOrderedList);

        float maxFillAmount = enemyMonstersOrderedList.Max(x => DamageMode ? x.Damage : x.Healing);

        for (int i = 0; i < 5; i++)
        {
            var alliedMonsterData = alliedMonsterOrderedList[i];
            var enemyMonsterData = enemyMonstersOrderedList[i];
            AlliedPostBattleMonsters[i].SetPostBattleMonster(alliedMonsterData.visualDescriptor, colorToUse, DamageMode ? alliedMonsterData.Damage / maxFillAmount : alliedMonsterData.Healing / maxFillAmount, DamageMode ? alliedMonsterData.Damage : alliedMonsterData.Healing, alliedMonsterData.monsterParts);
            EnemyPostBattleMonsters[i].SetPostBattleMonster(enemyMonsterData.visualDescriptor, colorToUse, DamageMode ? enemyMonsterData.Damage / maxFillAmount : enemyMonsterData.Healing / maxFillAmount, DamageMode ? enemyMonsterData.Damage : enemyMonsterData.Healing, enemyMonsterData.monsterParts);
        }
    }

    public void FillList(Team team)
    {
        if (team.isGoodTeam)
        {
            AlliedMonsterForPostBattles.Clear();
            foreach (var monster in team.GetCharactersAll())
            {
                AlliedMonsterForPostBattles.Add(new MonsterForPostBattle()
                {
                    monsterId = monster.MonsterId.ToString(),
                    visualDescriptor = monster.visualDescriptor,
                    monsterParts = monster.monsterSkillController.skillListNoRepeat.Select(x => x.bodyPartSO).ToList()
                });
            }
            return;
        }

        EnemyMonsterForPostBattles.Clear();

        foreach (var monster in team.GetCharactersAll())
        {
            EnemyMonsterForPostBattles.Add(new MonsterForPostBattle()
            {
                monsterId = monster.MonsterId.ToString(),
                visualDescriptor = monster.visualDescriptor,
                monsterParts = monster.monsterSkillController.skillListNoRepeat.Select(x => x.bodyPartSO).ToList()
            });
        }
    }
}
