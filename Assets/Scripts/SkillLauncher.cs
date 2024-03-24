using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using finished3;
using Spine;
using Spine.Unity;
using UnityEngine;

public class SkillLauncher : MonoBehaviour
{
    public AxieBodyPartsManager skillList;

    static public SkillLauncher Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    public IEnumerator ThrowSkill(List<AxieSkill> skills, SkeletonAnimation skeletonAnimation, AxieController target,
        AxieController self)
    {
        List<SkillAction> skillActions = new List<SkillAction>();
        for (int i = 0; i < skills.Count; i++)
        {
            Skill skill = Instantiate(skills[i].bodyPartSO.prefab).GetComponent<Skill>();

            skill.axieBodyPart = skills[i].bodyPartSO;
            skill.self = self;
            skill.@class = skills[i].bodyPartSO.bodyPartClass;
            skill.skeletonAnimation = skeletonAnimation;
            skillActions.AddRange(PerformSkill(skills[i], skill, self, target));
        }

        yield return StartCoroutine(DoSkills(skillActions));
    }

    IEnumerator DoSkills(List<SkillAction> skillActions)
    {
        float timer = 0;
        while (skillActions.Count > 0)
        {
            foreach (var skillAction in skillActions)
            {
                if (timer >= skillAction.triggerTime)
                {
                    skillAction.Action.Invoke();
                }
            }

            skillActions.RemoveAll(x => timer >= x.triggerTime);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    private bool FulfillsTriggerCondition(AxieSkill skill, Skill skillInstance, AxieController self,
        AxieController target, SkillEffect skillEffect)
    {
        IngameStats myAxieData = self.axieIngameStats;

        if (skillEffect.UseSpecialsAsTrigger)
        {
            if (skillEffect.specialActivationAgainstAxiesList.Count > 0)
            {
                if (skillEffect.specialActivationAgainstAxiesList.Any(x =>
                        x.axieClass == target.axieIngameStats.axieClass) == false)
                {
                    return false;
                }
            }

            if (skillEffect.specialActivationIfComboedWithList.Count > 0)
            {
                bool comboed = false;
                foreach (var skillToComboWith in skillEffect.specialActivationIfComboedWithList)
                {
                    if (skillToComboWith.OnlyCareAboutClassCard)
                    {
                        if (self.axieSkillController.IsClassSkillInAxie(skillToComboWith.axieClassCard, skill))
                        {
                            comboed = true;
                            break;
                        }
                    }
                    else if (self.axieSkillController.IsSkillSelectedInAxie(skillToComboWith.axieCard, skill))
                    {
                        comboed = true;
                        break;
                    }
                }

                if (!comboed)
                    return false;
            }

            if (skillEffect.specialActivationWithBodyParts.Count > 0)
            {
                bool activated = false;
                foreach (var bodyPart in skillEffect.specialActivationWithBodyParts)
                {
                    if (bodyPart.OnlyCareAboutClassCard)
                    {
                        if (self.axieBodyParts.Contains(bodyPart.axieCard))
                        {
                            activated = true;
                            break;
                        }
                    }
                    else if (self.axieSkillController.IsSkillSelectedInAxie(bodyPart.axieCard, skill))
                    {
                        activated = true;
                        break;
                    }
                }

                if (!activated)
                {
                    return false;
                }
            }

            if (skillEffect.specialActivationBasedOnAxiesInBattle.Count > 0)
            {
                bool activated = false;
                foreach (var specialActivation in skillEffect.specialActivationBasedOnAxiesInBattle)
                {
                    if (self.goodTeam.GetCharacters()
                            .Any(x => x.axieIngameStats.axieClass == specialActivation.axieClass) || self.badTeam
                            .GetCharacters().Any(x => x.axieIngameStats.axieClass == specialActivation.axieClass))
                    {
                        activated = true;
                        break;
                    }
                }

                if (!activated)
                {
                    return false;
                }
            }

            return true;
        }

        if (skillEffect.HPTresholdPercentage > 0)
        {
            if (!skillEffect.LessThan
                    ? myAxieData.currentHP < myAxieData.HP * (skillEffect.HPTresholdPercentage / 100f)
                    : myAxieData.currentHP > myAxieData.HP * (skillEffect.HPTresholdPercentage / 100f))
            {
                return false;
            }
        }

        if (skillEffect.ComboAmount > 0)
        {
            if (self.axieSkillController.GetComboAmount() < skillEffect.ComboAmount)
            {
                return false;
            }
        }

        if (skillEffect.LastAxieAliveTeam)
        {
            if (self.goodTeam.AxieAliveAmount != 1)
            {
                return false;
            }
        }

        if (skillEffect.LastAxieAliveOpponent)
        {
            if (self.badTeam.AxieAliveAmount != 1)
            {
                return false;
            }
        }

        if (skillEffect.Shielded)
        {
            if (target.axieIngameStats.currentShield <= 0)
            {
                return false;
            }
        }

        if (skillEffect.TargetIsDebuff)
        {
            if (!target.axieSkillEffectManager.IsDebuff())
            {
                return false;
            }
        }

        if (skillEffect.SelfIsDebuff)
        {
            if (!self.axieSkillEffectManager.IsDebuff())
            {
                return false;
            }
        }

        if (skillEffect.TargetIsPoisoned)
        {
            if (!target.axieSkillEffectManager.IsPoisoned())
            {
                return false;
            }
        }

        if (skillEffect.RangeTarget)
        {
            if (target.Range <= 1)
            {
                return false;
            }
        }

        if (skillEffect.SecondsOfFight > 0)
        {
            if (FightManagerSingleton.Instance.SecondsOfFight < skillEffect.SecondsOfFight)
            {
                return false;
            }
        }

        if (skillEffect.statDifferenceTrigger)
        {
            if (skillEffect.AttackStatDifference != 0)
            {
                int realAttack =
                    AxieStatCalculator.GetRealAttack(self.stats, self.axieSkillEffectManager.GetAttackBuff());
                if ((skillEffect.AttackStatDifference < 0 && realAttack >= 0) ||
                    (skillEffect.AttackStatDifference > 0 && realAttack <= 0))
                {
                    return false;
                }
            }


            if (skillEffect.SpeedStatDifference != 0)
            {
                int realSpeed =
                    AxieStatCalculator.GetRealSpeed(self.stats.speed, self.axieSkillEffectManager.GetSpeedBuff());
                if ((skillEffect.SpeedStatDifference < 0 && realSpeed >= 0) ||
                    (skillEffect.SpeedStatDifference > 0 && realSpeed <= 0))
                {
                    return false;
                }
            }

            if (skillEffect.MoraleStatDifference != 0)
            {
                int realMorale =
                    AxieStatCalculator.GetRealMorale(self.stats.morale, self.axieSkillEffectManager.GetMoraleBuff());
                if ((skillEffect.MoraleStatDifference < 0 && realMorale >= 0) ||
                    (skillEffect.MoraleStatDifference > 0 && realMorale <= 0))
                {
                    return false;
                }
            }

            if (skillEffect.CurrentHPStatDifference != 0)
            {
                if ((skillEffect.CurrentHPStatDifference < 0 &&
                     self.axieIngameStats.currentHP >= target.axieIngameStats.currentHP) ||
                    (skillEffect.CurrentHPStatDifference >= 0 &&
                     self.axieIngameStats.currentHP <= target.axieIngameStats.currentHP))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private List<AxieController> DoSkillEffect(AxieController self,
        AxieController target, SkillEffect skillEffect)
    {
        List<AxieController> statusEffectTargetList = new List<AxieController>();
        List<AxieController> targetList = new List<AxieController>();

        bool remove = false;
        bool steal = false;
        targetList.Add(self.CurrentTarget);

        switch (skillEffect.statusApplyType)
        {
            case StatusApplyType.AllAxies:
                statusEffectTargetList.AddRange(self.goodTeam.GetCharacters());
                statusEffectTargetList.AddRange(self.badTeam.GetCharacters());
                break;
            case StatusApplyType.ApplyAllied:
            case StatusApplyType.ApplyTeam:
                statusEffectTargetList.AddRange(self.goodTeam.GetCharacters());
                break;
            case StatusApplyType.ApplyEnemyTeam:
                statusEffectTargetList.AddRange(self.badTeam.GetCharacters());
                break;
            case StatusApplyType.ApplySelfAndEnemy:
                statusEffectTargetList.Add(self);
                statusEffectTargetList.Add(target);
                break;
            case StatusApplyType.ApplyAdjacentTarget:
                statusEffectTargetList.AddRange(target.GetAdjacent());
                break;
            case StatusApplyType.ApplyAdjacentTargetAndTarget:
                statusEffectTargetList.AddRange(target.GetAdjacent());
                statusEffectTargetList.Add(target);
                break;
            case StatusApplyType.ApplyAdjacentSelfAndSelf:
                statusEffectTargetList.AddRange(target.GetAdjacent());
                statusEffectTargetList.Add(self);
                break;
            case StatusApplyType.ApplySelf:
                statusEffectTargetList.Add(self);
                break;
            case StatusApplyType.ApplyTarget:
                statusEffectTargetList.Add(target);
                break;
            case StatusApplyType.StealTargetFromSelf:
                statusEffectTargetList.Add(self);
                statusEffectTargetList.Add(target);
                steal = true;
                break;
            case StatusApplyType.StealSelfFromTarget:
                statusEffectTargetList.Add(target);
                statusEffectTargetList.Add(self);
                steal = true;
                break;
            case StatusApplyType.RemoveSelf:
                statusEffectTargetList.Add(self);
                remove = true;
                break;
            case StatusApplyType.RemoveTarget:
                statusEffectTargetList.Add(target);
                remove = true;
                break;
            case StatusApplyType.RemoveSelfAndTarget:
                statusEffectTargetList.Add(self);
                statusEffectTargetList.Add(target);
                remove = true;
                break;
        }

        if (skillEffect.statusApplyTypeIsTarget)
        {
            targetList = statusEffectTargetList;
        }

        if (skillEffect.Prioritize)
        {
            if (skillEffect.lowestHP)
            {
                AxieController newTarget = statusEffectTargetList.OrderBy(x => x.axieIngameStats.currentHP).First();

                targetList.Clear();
                targetList.Add(newTarget);
                self.CurrentTarget = newTarget;
            }

            if (skillEffect.FurthestTarget)
            {
                AxieController newTarget = self.goodTeam.FindFurthestCharacter(self, statusEffectTargetList);
                targetList.Clear();
                targetList.Add(newTarget);
                if (!skillEffect.RangeAbility)
                {
                    self.CurrentTarget = newTarget;
                }
            }

            if (skillEffect.targetHighestEnergy)
            {
                AxieController newTarget = statusEffectTargetList.OrderBy(x => x.axieIngameStats.CurrentEnergy)
                    .First();

                targetList.Clear();
                targetList.Add(newTarget);
                if (!skillEffect.RangeAbility)
                {
                    self.CurrentTarget = newTarget;
                }
            }

            if (skillEffect.targetHighestSpeed)
            {
                AxieController newTarget = statusEffectTargetList.OrderBy(x =>
                        AxieStatCalculator.GetRealSpeed(target.stats.speed,
                            target.axieSkillEffectManager.GetSpeedBuff()))
                    .First();

                targetList.Clear();
                targetList.Add(newTarget);
                if (!skillEffect.RangeAbility)
                {
                    self.CurrentTarget = newTarget;
                }
            }

            if (skillEffect.targetAxieClass)
            {
                AxieController newTarget = statusEffectTargetList.First(x =>
                    target.goodTeam.GetCharacters()
                        .FirstOrDefault(y => y.axieIngameStats.axieClass == skillEffect.axieClassToTarget));
                targetList.Clear();
                targetList.Add(newTarget);
                if (!skillEffect.RangeAbility)
                {
                    self.CurrentTarget = newTarget;
                }
            }
        }

        if (skillEffect.IsOnlyBuffOrDebuff())
        {
            if (!steal)
            {
                statusEffectTargetList.ForEach(x =>
                {
                    if (remove)
                    {
                        x.RemoveAllEffects();
                    }
                    else
                    {
                        x.AddStatusEffect(skillEffect);
                    }
                });
            }
            else
            {
                AxieController stealer = statusEffectTargetList[0];
                AxieController gotStolen = statusEffectTargetList[1];

                List<SkillEffect> skillsToSteal = gotStolen.axieSkillEffectManager.GetAllBuffs();

                foreach (var effect in skillsToSteal)
                {
                    gotStolen.RemoveStatusEffect(effect);
                    stealer.AddStatusEffect(effect);
                }
            }
        }

        if (skillEffect.StealEnergyPercentage > 0)
        {
            float energyToTransfer = target.axieIngameStats.CurrentEnergy * (skillEffect.StealEnergyPercentage * 0.01f);

            target.axieIngameStats.CurrentEnergy -= energyToTransfer;
            self.axieIngameStats.CurrentEnergy += energyToTransfer;
        }

        if (skillEffect.GainShield > 0)
        {
            self.axieIngameStats.currentShield += skillEffect.GainShield;
        }

        if (skillEffect.GainHPPercentage > 0)
        {
            if (!skillEffect.HPBaseOnDamage)
            {
                foreach (var axieController in statusEffectTargetList)
                {
                    axieController.axieIngameStats.currentHP += (skillEffect.GainHPPercentage * 0.01f);
                }
            }
            //TODO: Gain HP on Hit saved for damage calculation
        }

        return targetList;
    }

    private void DamageCaluculation()
    {
    }

    private List<SkillAction> PerformSkill(AxieSkill skill, Skill skillInstance, AxieController self,
        AxieController target)
    {
        List<SkillAction> skillActions = new List<SkillAction>();


        foreach (var skillEffect in skill.bodyPartSO.skillEffects)
        {
            List<AxieController> Targets = new List<AxieController>();
            if (skillEffect.hasTriggerCondition)
            {
                if (FulfillsTriggerCondition(skill, skillInstance, self, target, skillEffect))
                {
                    Targets = DoSkillEffect(self, target, skillEffect);
                }
            }
            else
            {
                Targets = DoSkillEffect(self, target, skillEffect);
            }
        }

        return skillActions;
    }
}