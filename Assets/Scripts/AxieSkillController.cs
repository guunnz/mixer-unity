using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class AxieSkill
{
    public SkillName skillName;
    public BodyPart bodyPart;
    public AxieBodyPart bodyPartSO;
}

[System.Serializable]
public class AxiePassives
{
    public List<AxieBodyPart> bodyPartList = new List<AxieBodyPart>();
    public List<AxieBodyPart> bodyPartListReactivation = new List<AxieBodyPart>();
    public List<AxieBodyPart> bodyPartListShieldBreak = new List<AxieBodyPart>();
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

public class AxieSkillController : MonoBehaviour
{
    public List<AxieSkill> skillList = new List<AxieSkill>();
    public List<AxieSkill> skillListNoRepeat = new List<AxieSkill>();

    public AxiePassives passives = new AxiePassives();

    public AxieController self;
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

    public List<AxieSkill> GetAxieSkills()
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
        List<AxieController> myTeam = self.goodTeam.GetAliveCharacters();
        int merryStacks = myTeam.Sum(x => x.axieSkillEffectManager.MerryStacks());
        if (merryStacks != 0 && (merryStacks >= 5))
        {
            foreach (var axieController in myTeam)
            {
                axieController.axieSkillEffectManager.RemoveStatusEffect(StatusEffectEnum.Merry);
            }

            AxieBodyPart part =
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
                            skillList.FirstOrDefault(x => x.skillName == reactivation.skillName), self.SkeletonAnim,
                            self.CurrentTarget, self));
                }
            }
        }
    }

    private void CreateTesting()
    {
        SetAxieSkills(SkillLauncher.Instance.skillList.axieBodyParts.Select(x => x.skillName).ToList(),
            SkillLauncher.Instance.skillList.axieBodyParts.Select(x => x.bodyPart).ToList());
    }

    public List<AxieSkill> GetAxieSkills(AxieSkill exception)
    {
        return skillList.Where(x => x != exception).ToList();
    }

    public bool IsSkillSelectedInAxie(SkillName skillName, AxieSkill exception)
    {
        return GetAxieSkills(exception).Any(x => x.skillName == skillName);
    }

    public bool IsClassSkillInAxie(AxieClass axieClass, AxieSkill exception)
    {
        return GetAxieSkills(exception).Any(x => x.bodyPartSO.bodyPartClass == axieClass);
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
        List<AxieBodyPart> bodyPart = skillList.Select(x => x.bodyPartSO).ToList();

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
        self.axieIngameStats.totalComboCost = comboCost;
        self.statsManagerUI.SetManaBars(comboCost);
        self.statsManagerUI.SetMana(AxieStatCalculator.GetAxieMinEnergy(self.stats) / comboCost);

    }

    public void AddAndHandleSpecialCases(AxieSkill skill, List<AxieBodyPart> bodyParts, AxieBodyPart bodyPartToExclude)
    {
        skillList.Add(skill);
        skillListNoRepeat.Add(skill);
        switch (skill.skillName)
        {
            case SkillName.Imp:
                if (bodyParts.Any(
                        x => x.isPassive == false && x.bodyPartClass == AxieClass.Beast && x != bodyPartToExclude))
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
        self.axieIngameStats.MaxEnergy = GetComboCost();
        lastActivationTimes.Clear();
        if (!passives.bodyPartList.Any(x =>
                x.skillEffects.Any(y => y.skillTriggerType == SkillTriggerType.Battlecry)))
        {
            return;
        }

        self.axieBehavior.SetAttackSpeed();



        foreach (AxieBodyPart bodyPartPassive in passives.bodyPartList)
        {
            foreach (var skillEffect in bodyPartPassive.skillEffects)
            {
                if (skillEffect.skillTriggerType != SkillTriggerType.Battlecry)
                    continue;


                StartCoroutine(
                    SkillLauncher.Instance.ThrowPassive(
                        skillList.FirstOrDefault(x => x.skillName == bodyPartPassive.skillName), self.SkeletonAnim,
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

        foreach (AxieBodyPart bodyPartPassive in passives.bodyPartList)
        {
            foreach (var skillEffect in bodyPartPassive.skillEffects)
            {
                if (skillEffect.skillTriggerType != SkillTriggerType.PassiveOnAttack)
                    continue;

                hasPassiveOnAutoattack = true;
                StartCoroutine(
                    SkillLauncher.Instance.ThrowPassive(
                        skillList.FirstOrDefault(x => x.skillName == bodyPartPassive.skillName), self.SkeletonAnim,
                        self.CurrentTarget, self));
            }
        }

        return hasPassiveOnAutoattack;
    }

    public void DamageReceived(AxieClass attackClass, float damage, AxieController attacker, bool isSkill = false)
    {
        if (!self.imGood)
            self.statsManagerUI.SpawnDamage(damage.ToString(), isSkill);
        PostBattleManager.Instance.SumDamage(attacker.AxieId.ToString(), damage, attacker.imGood);
        if (this.self.axieSkillEffectManager.IsPoisoned())
        {
            self.axieIngameStats.currentHP -=
                AxieStatCalculator.GetPoisonDamage(self.axieSkillEffectManager.PoisonStacks());
        }

        if (passives.AbilityReflectDamageAmount != 0 && isSkill)
        {
            attacker.axieIngameStats.currentHP -= damage * (passives.AbilityReflectDamageAmount / 100);
        }

        foreach (AxieBodyPart bodyPartPassive in passives.bodyPartList)
        {
            if (passives.MeleeReflectDamageAmount > 0 && LastTimeReflected <= 0)
            {
                LastTimeReflected = 0.5f;
                attacker.axieIngameStats.currentHP -= damage * (passives.MeleeReflectDamageAmount / 100f);
            }

            var currentShield = self.statsManagerUI.shieldValue;

            foreach (var skillEffect in bodyPartPassive.skillEffects)
            {
                foreach (var bodyPart in skillEffect.specialActivactionWhenReceiveDamage)
                {
                    if (bodyPart.onlyAbilities && !isSkill)
                        continue;

                    if (bodyPart.axieClass != attackClass)
                        continue;

                    if (skillEffect.OnShieldBreak && damage < self.axieIngameStats.currentShield && currentShield > 0 || skillEffect.OnShieldBreak && self.axieIngameStats.currentShield <= 0 && currentShield > 0)
                    {
                        continue;
                    }
                    if (skillEffect.RangeAbility && attacker.Range <= 1)
                    {
                        continue;
                    }


                    StartCoroutine(
                        SkillLauncher.Instance.ThrowPassive(
                            skillList.FirstOrDefault(x => x.skillName == bodyPartPassive.skillName), self.SkeletonAnim,
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
    public void SetAxieSkills(List<SkillName> skillNames, List<BodyPart> bodyParts)
    {
        comboCost = 0;
        skillList.Clear();
        skillListNoRepeat.Clear();

        passives = new AxiePassives();
        var pairedBodyParts = new List<AxieBodyPart>();

        for (int i = 0; i < skillNames.Count; i++)
        {
            pairedBodyParts.AddRange(SkillLauncher.Instance.skillList.axieBodyParts
                .Where(x => x.skillName == skillNames[i] && x.bodyPart == bodyParts[i]));
        }

        foreach (var bodyPart in pairedBodyParts)
        {
            AxieSkill skill = new AxieSkill();
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