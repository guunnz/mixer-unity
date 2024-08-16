using AxieCore.AxieMixer;
using Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static enemies.GetAxiesEnemies;
using static GetAxiesExample;

[System.Serializable]

public class AxieForTesting
{
    public GetAxiesExample.Stats axieStats;
    public bool UseAxieStats;
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

    public List<AxieForTesting> allyAxiesForTesting = new List<AxieForTesting>();
    public List<AxieForTesting> enemyAxiesForTesting = new List<AxieForTesting>();

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
            MorphAlliedAxies();
        }
    }

    public List<AxieEnemy> GetEnemyAxies(List<AxieEnemy> axieList)
    {
        if (enemyAxiesForTesting.Count == 0)
        {
            return axieList;
        }

        int index = 0;
        foreach (var axie in axieList)
        {
            if (index >= allyAxiesForTesting.Count)
            {
                break;
            }
            var axieForTesting = enemyAxiesForTesting[index];

            if (!axieForTesting.UseAxieStats)
            {
                axie.stats = axieForTesting.axieStats;
            }

            var AxieParts = new List<GetAxiesExample.Part>();
            axie.cursedMeta = new Dictionary<string, string>();
            int indexAb = 0;
            foreach (var abilityToUse in axieForTesting.abilitiesToUse)
            {
                string bodyPart = axieForTesting.specialBodyPartForAbilityIfNeeded.Count > indexAb ? axieForTesting.specialBodyPartForAbilityIfNeeded[indexAb].ToString() : "";
                if (bodyPart == BodyPart.None.ToString())
                {
                    bodyPart = "";
                }
                Part part = PartFinder.GetOriginalPartIdTesting(abilityToUse.ToString(), bodyPart);
                string abilityId = string.Join("-", part.ability_id.Split('-').Where((s, i) => i != 1));
                AxieParts.Add(new GetAxiesExample.Part(part.class_type, part.name, part.part_type, indexAb, true, abilityId));
                axie.cursedMeta.Add(part.part_type, abilityId);
                indexAb++;
            }

            axie.Parts = AxieParts;
            index++;
        }

        return axieList;
    }

    public void MorphAlliedAxies()
    {
        if (allyAxiesForTesting.Count == 0)
        {
            return;
        }

        var axieList = goodTeam.GetCharactersAll();

        int index = 0;
        foreach (var axie in axieList)
        {
            if (index >= allyAxiesForTesting.Count)
            {
                break;
            }
            var axieForTesting = allyAxiesForTesting[index];
            if (!axieForTesting.UseAxieStats)
            {
                axie.stats = axieForTesting.axieStats;
            }
            var AxieParts = new List<GetAxiesExample.Part>();
            Dictionary<string, string> cursedMeta = new Dictionary<string, string>();
            int indexAb = 0;
            List<SkillName> newSkillNames = new List<SkillName>();
            List<BodyPart> newBodyParts = new List<BodyPart>();
            foreach (var abilityToUse in axieForTesting.abilitiesToUse)
            {
                string bodyPart = axieForTesting.specialBodyPartForAbilityIfNeeded.Count > indexAb ? axieForTesting.specialBodyPartForAbilityIfNeeded[indexAb].ToString() : "";

                if (bodyPart == BodyPart.None.ToString())
                {
                    bodyPart = "";
                }
                Part part = PartFinder.GetOriginalPartIdTesting(abilityToUse.ToString(), bodyPart);
                string abilityId = string.Join("-", part.ability_id.Split('-').Where((s, i) => i != 1));
                AxieParts.Add(new GetAxiesExample.Part(part.class_type, part.name, part.part_type, indexAb, true, abilityId));
                cursedMeta.Add(part.part_type, abilityId);
                newSkillNames.Add(abilityToUse);
                newBodyParts.Add((BodyPart)Enum.Parse(typeof(BodyPart), part.part_type, true));
                indexAb++;
            }
            axie.axieSkillController.SetAxieSkills(newSkillNames, newBodyParts);
            AxieSpawner.Instance.SimpleProcessCursedMixer(axie.AxieId.ToString(), axie.Genes, false, cursedMeta, axie);
            index++;

            axie.UpdateStats();
        }
    }

    public void SetEnemyAxiesStatuses()
    {
        int index = 0;
        foreach (var character in enemyTeam.GetCharactersAll())
        {
            if (enemyAxiesForTesting.Count == 0)
                return;
            if (index >= enemyAxiesForTesting.Count)
            {
                character.stats.hp = 0;
            }
            character.UpdateStats();
            Debug.Log(character.axieIngameStats.currentHP);
            if (enemyTeam.GetCharactersAll().Count <= index || enemyAxiesForTesting.Count <= index)
            {
                continue;
            }
            var axieForTesting = enemyAxiesForTesting[index];

            foreach (var buff in axieForTesting.startsWithBuffDebuff)
            {
                character.AddStatusEffect(new SkillEffect { statusEffect = buff, skillDuration = axieForTesting.DebuffsDuration });
            }
            index++;
        }

        FightManagerSingleton.Instance.SecondsOfFight = BattleTimerStartIn;
    }

    public void SetAllyAxiesStatuses()
    {
        int index = 0;
        foreach (var character in goodTeam.GetCharactersAll())
        {
            if (allyAxiesForTesting.Count == 0)
                return;
            if (index >= allyAxiesForTesting.Count)
            {
                character.stats.hp = 0;
            }
            character.UpdateStats();

            Debug.Log(character.axieIngameStats.currentHP);
            if (goodTeam.GetCharactersAll().Count <= index || allyAxiesForTesting.Count <= index)
            {
                continue;
            }
            if (allyAxiesForTesting.Count == 0)
            {
                continue;
            }

            var axieForTesting = allyAxiesForTesting[index];

            foreach (var buff in axieForTesting.startsWithBuffDebuff)
            {
                character.AddStatusEffect(new SkillEffect { statusEffect = buff, skillDuration = axieForTesting.DebuffsDuration });
            }
            index++;
        }
    }
}