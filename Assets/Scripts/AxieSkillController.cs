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

public class AxiePassives
{
    public List<AxieBodyPart> bodyPartList = new List<AxieBodyPart>();
    public bool ImmuneToCriticals;
    public bool AutoattacksIgnoreShield;
    public float AutoattackIncrease;
    public int DamageReductionAmount;
    public int RangedReflectDamageAmount;
    public int MeleeReflectDamageAmount;
}

public class AxieSkillController : MonoBehaviour
{
    public List<AxieSkill> skillList = new List<AxieSkill>();

    public AxiePassives passives = new AxiePassives();

    public AxieController self;

    private int comboCost;

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
        int womboCount = bodyPart.Count(x => x.wombo);
        int activeSkillsInCombo = bodyPart.Count(x => !x.isPassive);
        int energySpent = (int)bodyPart.Sum(x => x.energy);
        bool isWombo = womboCount < activeSkillsInCombo;

        if (isWombo)
        {
            energySpent -= (int)bodyPart.Where(x => x.wombo).Sum(x => x.energy);
        }

        comboCost = energySpent;
    }

    public void AddAndHandleSpecialCases(AxieSkill skill, List<AxieBodyPart> bodyParts, AxieBodyPart bodyPartToExclude)
    {
        skillList.Add(skill);
        passives = new AxiePassives();
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

        foreach (var axieBodyPart in bodyParts)
        {
            if (axieBodyPart.isPassive)
            {
                AxiePassives passives = new AxiePassives();
                List<SkillEffect> skillEffects = axieBodyPart.skillEffects.ToList();
                foreach (var skillEffect in skillEffects)
                {
                    if (skillEffect.InmuneToCriticalStrike)
                    {
                        passives.ImmuneToCriticals = true;
                    }

                    passives.MeleeReflectDamageAmount += skillEffect.MeleeReflect;
                    passives.RangedReflectDamageAmount += skillEffect.RangedReflect;
                    passives.DamageReductionAmount += skillEffect.ReduceDamagePercentage;
                    passives.AutoattackIncrease += skillEffect.skillTriggerType == SkillTriggerType.PassiveOnAttack
                        ? axieBodyPart.damage
                        : 0f;

                    passives.bodyPartList.Add(axieBodyPart);
                }
            }
        }
    }

    public void OnAutoAttack(float damage)
    {
        if (passives.bodyPartList.Any(x =>
                x.skillEffects.Any(y => y.skillTriggerType == SkillTriggerType.PassiveOnAttack)))
        {
            return;
        }

        foreach (AxieBodyPart bodyPartPassive in passives.bodyPartList)
        {
            foreach (var skillEffect in bodyPartPassive.skillEffects)
            {
                if (skillEffect.skillTriggerType != SkillTriggerType.PassiveOnAttack)
                    return;

                StartCoroutine(
                    SkillLauncher.Instance.ThrowPassive(
                        skillList.FirstOrDefault(x => x.skillName == bodyPartPassive.skillName), self.SkeletonAnim,
                        self.CurrentTarget, self));
            }
        }
    }

    public void DamageReceived(AxieClass attackClass, float damage, AxieController attacker, bool isSkill = false)
    {
        if (this.self.axieSkillEffectManager.IsPoisoned())
        {
            self.axieIngameStats.currentHP -=
                AxieStatCalculator.GetPoisonDamage(self.axieSkillEffectManager.PoisonStacks());
        }

        foreach (AxieBodyPart bodyPartPassive in passives.bodyPartList)
        {
            if (passives.RangedReflectDamageAmount > 0)
            {
                Debug.Log("Reflected Ranged to: " + attacker.AxieId + " / Damage: " +
                          damage * passives.RangedReflectDamageAmount / 100f);
            }

            if (passives.MeleeReflectDamageAmount > 0)
            {
                Debug.Log("Reflected melee to: " + attacker.AxieId + " / Damage: " +
                          damage * passives.MeleeReflectDamageAmount / 100f);
            }

            foreach (var skillEffect in bodyPartPassive.skillEffects)
            {
                foreach (var bodyPart in skillEffect.specialActivactionWhenReceiveDamage)
                {
                    if (bodyPart.onlyAbilities && !isSkill)
                        continue;

                    if (bodyPart.axieClass != attackClass)
                        continue;

                    StartCoroutine(
                        SkillLauncher.Instance.ThrowPassive(
                            skillList.FirstOrDefault(x => x.skillName == bodyPartPassive.skillName), self.SkeletonAnim,
                            attacker, self));
                }
            }
        }
    }

    //Set by UI
    public void SetAxieSkills(List<SkillName> skillNames, List<BodyPart> bodyParts)
    {
        skillList.Clear();

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

            AddAndHandleSpecialCases(skill, pairedBodyParts, bodyPart);
        }
    }
}