using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class MonsterSkill
{
    public SkillName skillName;
    public BodyPart bodyPart;
    public MonsterBodyPart bodyPartSO;
}

[System.Serializable]
public class MonsterPassives
{
    public List<MonsterBodyPart> bodyPartList = new List<MonsterBodyPart>();
    public List<MonsterBodyPart> bodyPartListReactivation = new List<MonsterBodyPart>();
    public List<MonsterBodyPart> bodyPartListShieldBreak = new List<MonsterBodyPart>();
    public bool ImmuneToCriticals;
    public bool PotatoLeaf;
    public int HealOnDamageDealt;
    public bool AutoattacksIgnoreShield;
    public float AutoattackIncrease;
    public int DamageReductionAmount;
    public int MeleeReflectDamageAmount;
    public int AbilityReflectDamageAmount;
    public int ExtraDamageReceivedByAbilitiesAmount;
    public int ExtraArmorHelmet;
    public int ExtraShieldGained;
    public bool hasReactivations;
    public bool MerryActivated = false;
    public bool hasMerry = false;
    public bool rangedAlwaysCritical = false;
    public bool bloodmoonImmune;
}

public class MonsterSkillController : MonoBehaviour
{
    public List<MonsterSkill> skillList = new List<MonsterSkill>();
    public List<MonsterSkill> skillListNoRepeat = new List<MonsterSkill>();

    public MonsterPassives passives = new MonsterPassives();

    public MonsterController self;
    private float LastTimeReflected = 0.5f;

    private int comboCost;

    private Dictionary<string, float> lastActivationTimes = new Dictionary<string, float>();

    

    public int GetComboCost()
    {
        return comboCost == 0 ? 1 : comboCost;
    }

    public bool IgnoresShieldOnAttack()
    {
        return passives.AutoattacksIgnoreShield;
    }

    public List<MonsterSkill> GetMonsterSkills()
    {
        return skillList;
    }

    public void Update()
    {
        LastTimeReflected -= Time.deltaTime;
        if (self != null && self.goodTeam != null)
        {
            if (self.goodTeam.battleStarted)
            {
                Reactivations();
            }
        }
    }

    private void MerryBehavior()
    {
        List<MonsterController> myTeam = self.goodTeam.GetAliveCharacters();
        int merryStacks = myTeam.Sum(x => x.monsterSkillEffectManager.MerryStacks());
        if (merryStacks != 0 && (merryStacks >= 5))
        {
            foreach (var monsterController in myTeam)
            {
                monsterController.monsterSkillEffectManager.RemoveStatusEffect(StatusEffectEnum.Merry);
            }

            MonsterBodyPart part =
                passives.bodyPartListReactivation.First(x => x.skillName == SkillName.Merry);

            self.goodTeam.ChimeraSpawned = true;

            Chimera chimera = Instantiate(part.extraPrefabToInstantiate, this.transform.parent)
                .GetComponent<Chimera>();

            chimera.transform.position = self.imGood ? new Vector3(0, 2, 0) : new Vector3(7, 2, 0);
            chimera.transform.localScale = self.imGood ? new Vector3(-.1f, .1f, .1f) : new Vector3(.1f, .1f, .1f);

            chimera.chimeraTeam = self.goodTeam;
        }
    }

    private void Reactivations()
    {
        if (passives.hasReactivations)
        {
            if (passives.hasMerry)
            {
                MerryBehavior();
            }

            float secondsOfFight = FightManagerSingleton.Instance.SecondsOfFight;

            foreach (var reactivation in passives.bodyPartListReactivation)
            {
                int reactivationInterval = reactivation.skillEffects.First().ReactivateEffectEveryXSeconds;
                if (secondsOfFight >= 1f && (int)Mathf.Floor(secondsOfFight) % reactivationInterval == 0)
                {
                    string SkillName = reactivation.skillName.ToString();

                    // Check if the skill was already activated at this time
                    if (lastActivationTimes.ContainsKey(SkillName) &&
                        Mathf.Floor(secondsOfFight) <= lastActivationTimes[SkillName])
                    {
                        continue; // Skip if already activated at this second
                    }

                    // Update the last activation time
                    lastActivationTimes[SkillName] = Mathf.Floor(secondsOfFight);

                    StartCoroutine(
                        SkillLauncher.Instance.ThrowPassive(
                            skillList.FirstOrDefault(x => x.skillName == reactivation.skillName), self.Visual,
                            self.CurrentTarget, self));
                }
            }
        }
    }

    private void CreateTesting()
    {
        SetMonsterSkills(SkillLauncher.Instance.skillList.monsterBodyParts.Select(x => x.skillName).ToList(),
            SkillLauncher.Instance.skillList.monsterBodyParts.Select(x => x.bodyPart).ToList());
    }

    public List<MonsterSkill> GetMonsterSkills(MonsterSkill exception)
    {
        return skillList.Where(x => x != exception).ToList();
    }

    public bool IsSkillSelectedInMonster(SkillName skillName, MonsterSkill exception)
    {
        return GetMonsterSkills(exception).Any(x => x.skillName == skillName);
    }

    public bool IsClassSkillInMonster(MonsterClass monsterClass, MonsterSkill exception)
    {
        return GetMonsterSkills(exception).Any(x => x.bodyPartSO.bodyPartClass == monsterClass);
    }

    private void Start()
    {
        CalculateSkillCost();
    }

    public int GetComboAmount()
    {
        return skillList.Count(x => !x.bodyPartSO.isPassive);
    }

    private void CalculateSkillCost()
    {
        List<MonsterBodyPart> bodyPart = skillList.Select(x => x.bodyPartSO).ToList();

        if (bodyPart.Count(x => x.skillName == SkillName.Imp) > 1)
        {
            bodyPart.Remove(bodyPart.FirstOrDefault(x => x.skillName == SkillName.Imp));
        }
        int womboCount = bodyPart.Count(x => x.wombo);
        int activeSkillsInCombo = bodyPart.Count(x => !x.isPassive);
        int energySpent = (int)bodyPart.Sum(x => x.energy);
        bool isWombo = womboCount < activeSkillsInCombo;



        if (isWombo)
        {
            energySpent -= (int)bodyPart.Where(x => x.wombo).Sum(x => x.energy);
        }

        comboCost = energySpent;
        StartCoroutine(SetEnergyBars());
    }

    IEnumerator SetEnergyBars()
    {

        while (self == null)
            yield return null;
        self.monsterIngameStats.totalComboCost = comboCost;
        self.statsManagerUI.SetManaBars(comboCost);
        self.statsManagerUI.SetMana(MonsterStatCalculator.GetMonsterMinEnergy(self.stats) / comboCost);

    }

    public void AddAndHandleSpecialCases(MonsterSkill skill, List<MonsterBodyPart> bodyParts, MonsterBodyPart bodyPartToExclude)
    {
        skillList.Add(skill);
        skillListNoRepeat.Add(skill);
        switch (skill.skillName)
        {
            case SkillName.Imp:
                if (bodyParts.Any(
                        x => x.isPassive == false && x.bodyPartClass == MonsterClass.Beast && x != bodyPartToExclude))
                {
                    skillList.Add(skill);
                }

                break;
            default:
                break;
        }
    }

    public void OnBattleStart()
    {
        self.monsterIngameStats.MaxEnergy = GetComboCost();
        lastActivationTimes.Clear();
        if (!passives.bodyPartList.Any(x =>
                x.skillEffects.Any(y => y.skillTriggerType == SkillTriggerType.Battlecry)))
        {
            return;
        }

        self.monsterBehavior.SetAttackSpeed();



        foreach (MonsterBodyPart bodyPartPassive in passives.bodyPartList)
        {
            foreach (var skillEffect in bodyPartPassive.skillEffects)
            {
                if (skillEffect.skillTriggerType != SkillTriggerType.Battlecry)
                    continue;


                StartCoroutine(
                    SkillLauncher.Instance.ThrowPassive(
                        skillList.FirstOrDefault(x => x.skillName == bodyPartPassive.skillName), self.Visual,
                        self.CurrentTarget, self));
            }
        }
    }

    public bool OnAutoAttack()
    {
        if (!passives.bodyPartList.Any(x =>
                x.skillEffects.Any(y => y.skillTriggerType == SkillTriggerType.PassiveOnAttack)))
        {
            return false;
        }

        bool hasPassiveOnAutoattack = false;

        foreach (MonsterBodyPart bodyPartPassive in passives.bodyPartList)
        {
            foreach (var skillEffect in bodyPartPassive.skillEffects)
            {
                if (skillEffect.skillTriggerType != SkillTriggerType.PassiveOnAttack)
                    continue;

                hasPassiveOnAutoattack = true;
                StartCoroutine(
                    SkillLauncher.Instance.ThrowPassive(
                        skillList.FirstOrDefault(x => x.skillName == bodyPartPassive.skillName), self.Visual,
                        self.CurrentTarget, self));
            }
        }

        return hasPassiveOnAutoattack;
    }

    public void DamageReceived(MonsterClass attackClass, float damage, MonsterController attacker, bool isSkill = false)
    {
        if (!self.imGood)
            self.statsManagerUI.SpawnDamage(damage.ToString(), isSkill);
        PostBattleManager.Instance.SumDamage(attacker.MonsterId.ToString(), damage, attacker.imGood);
        if (this.self.monsterSkillEffectManager.IsPoisoned())
        {
            self.monsterIngameStats.currentHP -=
                MonsterStatCalculator.GetPoisonDamage(self.monsterSkillEffectManager.PoisonStacks());
        }

        if (passives.AbilityReflectDamageAmount != 0 && isSkill)
        {
            attacker.monsterIngameStats.currentHP -= damage * (passives.AbilityReflectDamageAmount / 100);
        }

        foreach (MonsterBodyPart bodyPartPassive in passives.bodyPartList)
        {
            if (passives.MeleeReflectDamageAmount > 0 && LastTimeReflected <= 0)
            {
                LastTimeReflected = 0.5f;
                attacker.monsterIngameStats.currentHP -= damage * (passives.MeleeReflectDamageAmount / 100f);
            }

            var currentShield = self.statsManagerUI.shieldValue;

            foreach (var skillEffect in bodyPartPassive.skillEffects)
            {
                foreach (var bodyPart in skillEffect.specialActivactionWhenReceiveDamage)
                {
                    if (bodyPart.onlyAbilities && !isSkill)
                        continue;

                    if (bodyPart.monsterClass != attackClass)
                        continue;

                    if (skillEffect.OnShieldBreak && damage < self.monsterIngameStats.currentShield && currentShield > 0 || skillEffect.OnShieldBreak && self.monsterIngameStats.currentShield <= 0 && currentShield > 0)
                    {
                        continue;
                    }
                    if (skillEffect.RangeAbility && attacker.Range <= 1)
                    {
                        continue;
                    }


                    StartCoroutine(
                        SkillLauncher.Instance.ThrowPassive(
                            skillList.FirstOrDefault(x => x.skillName == bodyPartPassive.skillName), self.Visual,
                            attacker, self));

                    if (skillEffect.OnShieldBreak)
                    {
                        break;
                    }
                }
            }
        }
    }

    //Set by UI
    public void SetMonsterSkills(List<SkillName> skillNames, List<BodyPart> bodyParts)
    {
        comboCost = 0;
        skillList.Clear();
        skillListNoRepeat.Clear();

        passives = new MonsterPassives();
        var pairedBodyParts = new List<MonsterBodyPart>();

        for (int i = 0; i < skillNames.Count; i++)
        {
            pairedBodyParts.AddRange(SkillLauncher.Instance.skillList.monsterBodyParts
                .Where(x => x.skillName == skillNames[i] && x.bodyPart == bodyParts[i]));
        }

        foreach (var bodyPart in pairedBodyParts)
        {
            MonsterSkill skill = new MonsterSkill();
            skill.skillName = bodyPart.skillName;
            skill.bodyPartSO = bodyPart;
            skill.bodyPart = bodyPart.bodyPart;

            if (skill.bodyPartSO.isPassive)
            {
                List<SkillEffect> skillEffects = skill.bodyPartSO.skillEffects.ToList();
                foreach (var skillEffect in skillEffects)
                {
                    if (skillEffect.InmuneToCriticalStrike)
                    {
                        passives.ImmuneToCriticals = true;
                    }

                    if (skillEffect.ReactivateEffectEveryXSeconds > 0)
                    {
                        passives.hasReactivations = true;
                        passives.bodyPartListReactivation.Add(skill.bodyPartSO);

                        if (skill.skillName == SkillName.Merry)
                        {
                            passives.hasMerry = true;
                        }
                    }

                    passives.MeleeReflectDamageAmount += skillEffect.MeleeReflect;
                    passives.AbilityReflectDamageAmount += skillEffect.AbilityReflect;

                    if (!skillEffect.PotatoLeaf)
                        passives.DamageReductionAmount += skillEffect.ReduceDamagePercentage;
                    else
                    {
                        passives.PotatoLeaf = true;
                    }

                    passives.AutoattacksIgnoreShield = skillEffect.IgnoresShield;
                    passives.AutoattackIncrease += skillEffect.skillTriggerType == SkillTriggerType.PassiveOnAttack
                        ? skill.bodyPartSO.damage
                        : 0f;

                    passives.bodyPartList.Add(skill.bodyPartSO);
                }
            }


            AddAndHandleSpecialCases(skill, pairedBodyParts, bodyPart);
        }

        CalculateSkillCost();
    }
}
