using Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static enemies.GetMonstersEnemies;
using static GetMonstersExample;

[System.Serializable]

public class MonsterForTesting
{
    public GetMonstersExample.Stats monsterStats;
    public bool UseMonsterStats;
    public List<SkillName> abilitiesToUse;
    public List<BodyPart> specialBodyPartForAbilityIfNeeded = new List<BodyPart>()
    {
        BodyPart.None,
        BodyPart.None,
    };
    public List<StatusEffectEnum> startsWithBuffDebuff = new List<StatusEffectEnum>();
    public float DebuffsDuration = 1;
}


public class TestTool : MonoBehaviour
{
    public int CoinsAmount = 10;
    public int BattleTimerStartIn = 0;

    public List<MonsterForTesting> allyMonstersForTesting = new List<MonsterForTesting>();
    public List<MonsterForTesting> enemyMonstersForTesting = new List<MonsterForTesting>();

    static public TestTool Instance;

    public Team enemyTeam;
    public Team goodTeam;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetCoins();
    }

    public void SetCoins()

    {
        RunManagerSingleton.instance.coins = CoinsAmount;
        RunManagerSingleton.instance.coinsText.text = CoinsAmount.ToString();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            MorphAlliedMonsters();
        }
    }

    public List<MonsterEnemy> GetEnemyMonsters(List<MonsterEnemy> monsterList)
    {
        if (enemyMonstersForTesting.Count == 0)
        {
            return monsterList;
        }

        int index = 0;
        foreach (var monster in monsterList)
        {
            if (index >= allyMonstersForTesting.Count)
            {
                break;
            }
            var monsterForTesting = enemyMonstersForTesting[index];

            if (!monsterForTesting.UseMonsterStats)
            {
                monster.stats = monsterForTesting.monsterStats;
            }

            var MonsterParts = new List<GetMonstersExample.Part>();
            monster.cursedMeta = new Dictionary<string, string>();
            int indexAb = 0;
            foreach (var abilityToUse in monsterForTesting.abilitiesToUse)
            {
                string bodyPart = monsterForTesting.specialBodyPartForAbilityIfNeeded.Count > indexAb ? monsterForTesting.specialBodyPartForAbilityIfNeeded[indexAb].ToString() : "";
                if (bodyPart == BodyPart.None.ToString())
                {
                    bodyPart = "";
                }
                Part part = PartFinder.GetOriginalPartIdTesting(abilityToUse.ToString(), bodyPart);
                string abilityId = string.Join("-", part.ability_id.Split('-').Where((s, i) => i != 1));
                MonsterParts.Add(new GetMonstersExample.Part(part.class_type, part.name, part.part_type, indexAb, true, abilityId));
                monster.cursedMeta.Add(part.part_type, abilityId);
                indexAb++;
            }

            monster.Parts = MonsterParts;
            index++;
        }

        return monsterList;
    }

    public void MorphAlliedMonsters()
    {
        if (allyMonstersForTesting.Count == 0)
        {
            return;
        }

        var monsterList = goodTeam.GetCharactersAll();

        int index = 0;
        foreach (var monster in monsterList)
        {
            if (index >= allyMonstersForTesting.Count)
            {
                break;
            }
            var monsterForTesting = allyMonstersForTesting[index];
            if (!monsterForTesting.UseMonsterStats)
            {
                monster.stats = monsterForTesting.monsterStats;
            }
            var MonsterParts = new List<GetMonstersExample.Part>();
            Dictionary<string, string> cursedMeta = new Dictionary<string, string>();
            int indexAb = 0;
            List<SkillName> newSkillNames = new List<SkillName>();
            List<BodyPart> newBodyParts = new List<BodyPart>();
            foreach (var abilityToUse in monsterForTesting.abilitiesToUse)
            {
                string bodyPart = monsterForTesting.specialBodyPartForAbilityIfNeeded.Count > indexAb ? monsterForTesting.specialBodyPartForAbilityIfNeeded[indexAb].ToString() : "";

                if (bodyPart == BodyPart.None.ToString())
                {
                    bodyPart = "";
                }
                Part part = PartFinder.GetOriginalPartIdTesting(abilityToUse.ToString(), bodyPart);
                string abilityId = string.Join("-", part.ability_id.Split('-').Where((s, i) => i != 1));
                MonsterParts.Add(new GetMonstersExample.Part(part.class_type, part.name, part.part_type, indexAb, true, abilityId));
                cursedMeta.Add(part.part_type, abilityId);
                newSkillNames.Add(abilityToUse);
                newBodyParts.Add((BodyPart)Enum.Parse(typeof(BodyPart), part.part_type, true));
                indexAb++;
            }
            monster.monsterSkillController.SetMonsterSkills(newSkillNames, newBodyParts);
            MonsterSpawner.Instance.SimpleProcessCursedMixer(monster.MonsterId.ToString(), monster.Genes, false, cursedMeta, monster);
            index++;

            monster.UpdateStats();
        }
    }

    public void SetEnemyMonstersStatuses()
    {
        int index = 0;
        foreach (var character in enemyTeam.GetCharactersAll())
        {
            if (enemyMonstersForTesting.Count == 0)
                return;
            if (index >= enemyMonstersForTesting.Count)
            {
                character.stats.hp = 0;
            }
            character.UpdateStats();
            Debug.Log(character.monsterIngameStats.currentHP);
            if (enemyTeam.GetCharactersAll().Count <= index || enemyMonstersForTesting.Count <= index)
            {
                continue;
            }
            var monsterForTesting = enemyMonstersForTesting[index];

            foreach (var buff in monsterForTesting.startsWithBuffDebuff)
            {
                character.AddStatusEffect(new SkillEffect { statusEffect = buff, skillDuration = monsterForTesting.DebuffsDuration });
            }
            index++;
        }

        FightManagerSingleton.Instance.SecondsOfFight = BattleTimerStartIn;
    }

    public void SetAllyMonstersStatuses()
    {
        int index = 0;
        foreach (var character in goodTeam.GetCharactersAll())
        {
            if (allyMonstersForTesting.Count == 0)
                return;
            if (index >= allyMonstersForTesting.Count)
            {
                character.stats.hp = 0;
            }
            character.UpdateStats();

            Debug.Log(character.monsterIngameStats.currentHP);
            if (goodTeam.GetCharactersAll().Count <= index || allyMonstersForTesting.Count <= index)
            {
                continue;
            }
            if (allyMonstersForTesting.Count == 0)
            {
                continue;
            }

            var monsterForTesting = allyMonstersForTesting[index];

            foreach (var buff in monsterForTesting.startsWithBuffDebuff)
            {
                character.AddStatusEffect(new SkillEffect { statusEffect = buff, skillDuration = monsterForTesting.DebuffsDuration });
            }
            index++;
        }
    }
}
