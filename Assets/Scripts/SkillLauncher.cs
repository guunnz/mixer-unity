using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using finished3;
using Spine;
using Spine.Unity;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DamagePair
{
    public AxieController axieController;
    public int damage;
}

public class SpecialEffectExtras
{
    public AxieController axieController;
    public int extraDamage;
    public int multiplyStatusEffect = 1;
}

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
        float TotalDuration = 0;
        for (int i = 0; i < skills.Count; i++)
        {
            Skill skill = Instantiate(skills[i].bodyPartSO.prefab).GetComponent<Skill>();

            skill.axieBodyPart = skills[i].bodyPartSO;
            if (self.axieSkillEffectManager.IsKestreled() && skill.axieBodyPart.bodyPart == BodyPart.Horn)
            {
                continue;
            }
            else if (self.axieSkillEffectManager.IsHotbutted() && skill.axieBodyPart.bodyPart == BodyPart.Mouth)
            {
                continue;
            }
            skill.self = self;
            skill.origin = self.transform;
            skill.@class = skills[i].bodyPartSO.bodyPartClass;
            skill.skeletonAnimation = skeletonAnimation;
            skill.ExtraTimerCast += (0.55f * i);
            //Debug.Log("Skill performed: " + skills[i].skillName);
            for (int b = 0; b < skills[i].bodyPartSO.skillEffects.Count(); b++)
            {
                if (Backdoor(skills[i].bodyPartSO.skillEffects[b], self))
                {
                    yield return StartCoroutine(self.axieBehavior.GoBackdoorTarget());
                }
            }


            skillActions.AddRange(PerformSkill(skills[i], skill, self, target));
            TotalDuration += skill.totalDuration;
        }

        StartCoroutine(DoSkills(skillActions));

        yield return new WaitForSeconds(0.6f * skills.Count);
    }


    private bool Backdoor(SkillEffect skillEffect, AxieController self)
    {
        if (skillEffect.Prioritize)
        {
            var target = self.CurrentTarget;
            if (skillEffect.lowestHP)
            {
                AxieController newTarget = self.enemyTeam.GetAliveCharacters().OrderBy(x => x.axieIngameStats.currentHP)
                    .First();

                self.CurrentTarget = newTarget;

                return true;
            }

            if (skillEffect.FurthestTarget)
            {
                AxieController newTarget = self.myTeam.FindFurthestCharacter(self, self.enemyTeam.GetAliveCharacters());
                if (newTarget != null)
                {
                    self.CurrentTarget = newTarget;

                    return true;
                }

            }

            if (skillEffect.targetHighestEnergy)
            {
                AxieController newTarget = self.enemyTeam.GetAliveCharacters()
                    .OrderBy(x => x.axieIngameStats.CurrentEnergy)
                    .FirstOrDefault();
                if (newTarget != null)
                {
                    if (!skillEffect.RangeAbility)
                    {
                        self.CurrentTarget = newTarget;

                        return true;
                    }
                }

            }

            if (skillEffect.targetHighestSpeed)
            {
                AxieController newTarget = self.enemyTeam.GetAliveCharacters().OrderBy(x =>
                        AxieStatCalculator.GetRealSpeed(x.stats.speed, x.axieSkillEffectManager.GetSpeedBuff()))
                    .FirstOrDefault();
                if (newTarget != null)
                {
                    if (!skillEffect.RangeAbility)
                    {
                        self.CurrentTarget = newTarget;

                        return true;
                    }
                }

            }

            if (skillEffect.targetAxieClass)
            {
                if (target.myTeam.GetAliveCharacters().Any(y =>
                        y.axieIngameStats.axieClass == skillEffect.axieClassToTarget))
                {
                    AxieController newTarget = target.myTeam.GetAliveCharacters().FirstOrDefault(y =>
                        y.axieIngameStats.axieClass == skillEffect.axieClassToTarget);
                    if (newTarget != null)
                    {
                        if (!skillEffect.RangeAbility)
                        {
                            self.CurrentTarget = newTarget;

                            return true;
                        }
                    }
                }

            }
        }
        return false;
    }

    public IEnumerator ThrowPassive(AxieSkill passive, SkeletonAnimation skeletonAnimation, AxieController target,
        AxieController self)
    {
        List<SkillAction> skillActions = new List<SkillAction>();

        Skill skill = Instantiate(passive.bodyPartSO.prefab).GetComponent<Skill>();

        bool targetWasNull = false;
        while (self.CurrentTarget == null || target == null)
        {
            targetWasNull = true;
            yield return new WaitForFixedUpdate();
        }

        if (targetWasNull)
        {
            target = self.CurrentTarget;
        }


        for (int b = 0; b < passive.bodyPartSO.skillEffects.Count(); b++)
        {
            if (Backdoor(passive.bodyPartSO.skillEffects[b], self))
            {
                yield return StartCoroutine(self.axieBehavior.GoBackdoorTarget());
            }
        }

        skill.axieBodyPart = passive.bodyPartSO;
        skill.self = self;
        skill.origin = self.transform;
        skill.@class = passive.bodyPartSO.bodyPartClass;
        skill.skeletonAnimation = skeletonAnimation;
        skill.ExtraTimerCast++;
        skillActions.AddRange(PerformPassive(passive, skill, self, target));


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

    private SpecialEffectExtras HandleSpecialEffects(SkillEffect skillEffect, AxieController target, AxieSkill skill,
        AxieController self)
    {
        SpecialEffectExtras effectExtras = new SpecialEffectExtras();

        if (skillEffect.specialActivationAgainstAxiesList.Count > 0)
        {
            var specialActivationAgainstAxieClass = skillEffect.specialActivationAgainstAxiesList.FirstOrDefault(x =>
                x.axieClass == target.axieIngameStats.axieClass);
            if (specialActivationAgainstAxieClass != null)
            {
                effectExtras.extraDamage += specialActivationAgainstAxieClass.ExtraDamage;
                effectExtras.multiplyStatusEffect += specialActivationAgainstAxieClass.ExtraTimesStatusEffectApplied;
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
                        effectExtras.extraDamage += skillToComboWith.ExtraDamage;
                        effectExtras.multiplyStatusEffect += skillToComboWith.ExtraTimesStatusEffectApplied;
                    }
                }
                else if (self.axieSkillController.IsSkillSelectedInAxie(skillToComboWith.axieCard, skill))
                {
                    effectExtras.extraDamage += skillToComboWith.ExtraDamage;
                    effectExtras.multiplyStatusEffect += skillToComboWith.ExtraTimesStatusEffectApplied;
                }
            }
        }

        if (skillEffect.specialActivationBasedOnAxiesInBattle.Count > 0)
        {
            foreach (var bodyPart in skillEffect.specialActivationBasedOnAxiesInBattle)
            {
                int axieTypeMultiplier =
                    self.badTeam.GetAliveCharacters().Count(x => x.axieIngameStats.axieClass == bodyPart.axieClass) + self
                        .goodTeam.GetAliveCharacters().Count(x => x.axieIngameStats.axieClass == bodyPart.axieClass);

                effectExtras.multiplyStatusEffect += bodyPart.ExtraTimesStatusEffectAppliedPerAxie * axieTypeMultiplier;
                effectExtras.extraDamage += bodyPart.ExtraDamageAppliedPerAxie * axieTypeMultiplier;
            }
        }

        if (skillEffect.specialActivationWithBodyParts.Count > 0)
        {
            foreach (var bodyPart in skillEffect.specialActivationWithBodyParts)
            {
                if (bodyPart.OnlyCareAboutClassCard)
                {
                    if (self.axieBodyParts.Contains(bodyPart.axieCard))
                    {
                        effectExtras.extraDamage += bodyPart.ExtraDamage;
                        effectExtras.multiplyStatusEffect += bodyPart.ExtraTimesStatusEffectApplied;
                    }
                }
                else if (self.axieSkillController.IsSkillSelectedInAxie(bodyPart.axieCard, skill))
                {
                    effectExtras.extraDamage += bodyPart.ExtraDamage;
                    effectExtras.multiplyStatusEffect += bodyPart.ExtraTimesStatusEffectApplied;
                }
            }
        }

        return effectExtras;
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
                    if (self.goodTeam.GetAliveCharacters()
                            .Any(x => x.axieIngameStats.axieClass == specialActivation.axieClass) || self.badTeam
                            .GetAliveCharacters().Any(x => x.axieIngameStats.axieClass == specialActivation.axieClass))
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
                    ? myAxieData.currentHP < myAxieData.maxHP * (skillEffect.HPTresholdPercentage / 100f)
                    : myAxieData.currentHP > myAxieData.maxHP * (skillEffect.HPTresholdPercentage / 100f))
            {
                return false;
            }
        }


        if (skillEffect.OnShieldBreak)
        {
            if (target.axieIngameStats.currentShield != 0)
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
                    AxieStatCalculator.GetRealAttack(self.stats, self.axieSkillEffectManager.GetAttackBuff(), self.axieSkillEffectManager.GetMoraleBuff(), self.axieSkillEffectManager.GetSpeedBuff());
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
        AxieController target, SkillEffect skillEffect, Skill skillInstance, SpecialEffectExtras specialEffectExtras)
    {
        try
        {
            List<AxieController> statusEffectTargetList = new List<AxieController>();
            List<AxieController> targetList = new List<AxieController>();

            bool remove = false;
            bool steal = false;
            targetList.Add(self.CurrentTarget);

            switch (skillEffect.statusApplyType)
            {
                case StatusApplyType.AllAxies:
                    statusEffectTargetList.AddRange(self.goodTeam.GetAliveCharacters());
                    statusEffectTargetList.AddRange(self.badTeam.GetAliveCharacters());
                    break;
                case StatusApplyType.ApplyAllied:
                case StatusApplyType.ApplyTeam:
                    statusEffectTargetList.AddRange(self.goodTeam.GetAliveCharacters());
                    break;
                case StatusApplyType.ApplyEnemyTeam:
                    statusEffectTargetList.AddRange(self.badTeam.GetAliveCharacters());
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
                    statusEffectTargetList.AddRange(self.GetAdjacent());
                    statusEffectTargetList.Add(self);
                    break;
                case StatusApplyType.ApplyAdjacentSelf:
                    statusEffectTargetList.AddRange(self.GetAdjacent());
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

            statusEffectTargetList.RemoveAll(x => x == null);

            if (skillEffect.statusApplyTypeIsTarget)
            {
                targetList = statusEffectTargetList;
            }

            skillInstance.statusEffectTargetList = statusEffectTargetList;


            if (skillEffect.IsOnlyBuffOrDebuff())
            {
                if (!steal)
                {
                    statusEffectTargetList.ForEach(x =>
                    {
                        for (int i = 0; i < specialEffectExtras.multiplyStatusEffect; i++)
                        {
                            skillInstance.AddStatusEffectTargetPair(x.AxieId, new[] { skillEffect }, remove);
                        }
                    });
                }
                else
                {
                    AxieController stealer = statusEffectTargetList[0];
                    AxieController gotStolen = statusEffectTargetList[1];

                    List<SkillEffect> skillsToSteal = gotStolen.axieSkillEffectManager.GetAllBuffs();

                    skillInstance.AddStatusEffectTargetPair(gotStolen.AxieId, skillsToSteal.ToArray(), true);
                    skillInstance.AddStatusEffectTargetPair(stealer.AxieId, skillsToSteal.ToArray(), false);
                }

                if (target.axieSkillEffectManager.IsFishSnacked() && target.myTeam != self.myTeam)
                {
                    if (skillInstance.axieBodyPart.bodyPartClass == AxieClass.Bird || skillInstance.axieBodyPart.bodyPartClass == AxieClass.Aquatic)
                    {
                        skillInstance.AddStatusEffectTargetPair(self.AxieId, new[] {new SkillEffect()
                { statusEffect = StatusEffectEnum.Stun, Stun = true, skillDuration = 2 }  });

                        skillInstance.AddStatusEffectTargetPair(target.AxieId, new[] {new SkillEffect()
                { statusEffect = StatusEffectEnum.FishSnack,}  }, remove: true);
                    }
                }
            }

            if (skillEffect.StealEnergyPercentage > 0)
            {
                float energyToTransfer = target.axieIngameStats.CurrentEnergy *
                                         (skillEffect.StealEnergyPercentage * 0.01f) *
                                         specialEffectExtras.multiplyStatusEffect;

                target.axieIngameStats.CurrentEnergy -= energyToTransfer;
                self.axieIngameStats.CurrentEnergy += energyToTransfer;
            }

            if (skillEffect.GainEnergy > 0)
            {
                targetList.ForEach(x =>
                {
                    x.axieIngameStats.CurrentEnergy += (x.axieIngameStats.MaxEnergy * skillEffect.GainEnergy);
                });
            }


            if (skillInstance.axieBodyPart.bodyPart == BodyPart.Horn)
            {
                self.axieIngameStats.currentShield += skillInstance.axieBodyPart.shield *
                                                      ((self.axieSkillController.passives.ExtraArmorHelmet / 100f) == 0 ? 1 : (self.axieSkillController.passives.ExtraArmorHelmet / 100f));
            }
            else
            {
                self.axieIngameStats.currentShield += skillInstance.axieBodyPart.shield;
            }

            if (skillEffect.GainShield > 0)
            {
                if (skillEffect.lowestHP)
                {
                    var axie = self.myTeam.GetCharactersAll().OrderBy(x => x.axieIngameStats.currentHP).First();
                    axie.axieIngameStats.currentShield += (skillEffect.GainShield);
                }
                else
                {
                    self.axieIngameStats.currentShield += skillEffect.GainShield;
                }
            }
            if (self.axieSkillController.passives.ExtraShieldGained > 0)
            {
                self.axieIngameStats.currentShield += skillInstance.axieBodyPart.shield * (self.axieSkillController.passives.ExtraShieldGained / 100f);

            }

            if (self.axieIngameStats.currentShield > 10000)
            {
                Debug.LogError("EXTRA SHIELD GAIN : " + self.axieSkillController.passives.ExtraShieldGained);
                Debug.LogError("EXTRA SHIELD HELMET : " + self.axieSkillController.passives.ExtraArmorHelmet);
                Debug.LogError("MATH1 : " + skillInstance.axieBodyPart.shield * (self.axieSkillController.passives.ExtraArmorHelmet / 100f));
                Debug.LogError("MATH2 : " + 1 + (self.axieSkillController.passives.ExtraShieldGained / 100f));
                Debug.LogError("SHIELD SKILL: " + skillInstance.axieBodyPart.shield);
                self.axieIngameStats.currentShield = 1000;
            }
            if (skillEffect.GainHPPercentage > 0)
            {
                if (!skillEffect.HPBaseOnDamage)
                {
                    if (skillEffect.lowestHP)
                    {
                        var axie = self.myTeam.GetCharactersAll().OrderBy(x => x.axieIngameStats.currentHP).First();
                        skillInstance.AddHealTargetPair(axie.AxieId, (axie.axieIngameStats.maxHP *
                                                                                   (skillEffect.GainHPPercentage *
                                                                                       0.01f)) *
                                                                               specialEffectExtras
                                                                                   .multiplyStatusEffect);
                    }
                    else
                    {
                        foreach (var axieController in statusEffectTargetList)
                        {
                            skillInstance.AddHealTargetPair(axieController.AxieId, (axieController.axieIngameStats.maxHP *
                                                                                       (skillEffect.GainHPPercentage *
                                                                                           0.01f)) *
                                                                                   specialEffectExtras
                                                                                       .multiplyStatusEffect);
                        }
                    }

                }
            }

            return targetList;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message + " " + skillInstance.name);
            return null;
        }
    }

    private List<DamagePair> DamageCalculation(AxieSkill skill, Skill skillInstance, AxieController self,
        List<AxieController> targets, SkillEffect skillEffect, AxieController originalTarget,
        SpecialEffectExtras specialEffectExtras, bool multiCasted = false)
    {
        List<DamagePair> damagePairList = new List<DamagePair>();
        foreach (var target in targets)
        {
            try
            {
                DamagePair dmgPair = new DamagePair();
                int damageReduction = target.axieSkillController.passives.DamageReductionAmount + (target.axieSkillEffectManager.GeckoStacks() * 10);
                dmgPair.axieController = target;
                dmgPair.damage = Mathf.RoundToInt(skill.bodyPartSO.damage);



                if (skillEffect.DamageEqualsBasicAttack)
                {
                    dmgPair.damage =
                        AxieStatCalculator.GetRealAttack(self.stats, self.axieSkillEffectManager.GetAttackBuff(), self.axieSkillEffectManager.GetMoraleBuff(), self.axieSkillEffectManager.GetSpeedBuff());
                }

                if (skillEffect.ExtraDamagePercentage > 0)
                {
                    dmgPair.damage *= Mathf.RoundToInt(1f + (skillEffect.ExtraDamagePercentage * 0.01f));
                }

                if (skillEffect.HPBaseOnDamage)
                {
                    skillInstance.AddHealTargetPair(self.AxieId,
                        dmgPair.damage * (skillEffect.GainHPPercentage * 0.01f *
                         specialEffectExtras.multiplyStatusEffect == 0 ? 1 : specialEffectExtras.multiplyStatusEffect));
                }

                dmgPair.damage *= Mathf.RoundToInt(1f + (specialEffectExtras.extraDamage * .01f));

                if (skillEffect.ShieldAsDamagePercentage > 0)
                {
                    var shield = self.axieIngameStats.currentShield;
                    shield *= skillEffect.ShieldAsDamagePercentage * .01f;
                    dmgPair.damage += Mathf.RoundToInt(shield);
                }



                if (skill.bodyPartSO.skillEffects.Any(x => x.Lunge))
                {
                    int lungeAmount = self.axieSkillEffectManager.LungeAmount();
                    if (lungeAmount > 0)
                    {
                        dmgPair.damage += dmgPair.damage * (int)Math.Ceiling(lungeAmount * AxieStatCalculator.LungePercentage);
                    }
                }

                if (skill.bodyPartSO.skillEffects.Any(x => x.Trump))
                {
                    int economyAmount = self.imGood ? RunManagerSingleton.instance.netWorth : RunManagerSingleton.instance.eNetWorth;

                    dmgPair.damage += dmgPair.damage * (int)Math.Ceiling(economyAmount * AxieStatCalculator.LungePercentage);
                }

                var extraDamageByAbilities = target.axieSkillController.passives.ExtraDamageReceivedByAbilitiesAmount;

                if (extraDamageByAbilities > 0)
                {
                    dmgPair.damage *= Mathf.RoundToInt((extraDamageByAbilities / 100f) + 1f);
                }

                if (target.axieSkillEffectManager.IsAromad())
                {
                    damageReduction -= 50;
                }


                dmgPair.damage -= (dmgPair.damage * (damageReduction / 100));

                if (skill.bodyPartSO.OnlyDamageShields)
                {
                    if (dmgPair.damage > Mathf.RoundToInt(target.axieIngameStats.currentShield))
                    {
                        dmgPair.damage = Mathf.RoundToInt(target.axieIngameStats.currentShield);
                    }
                }

                damagePairList.Add(dmgPair);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        if (self.axieSkillEffectManager.IsJinxed())
            return damagePairList;

        //Calculate crits
        foreach (var damagePair in damagePairList)
        {
            if (damagePair.axieController.axieSkillController.passives.ImmuneToCriticals)
            {
                Debug.Log("Immune to crits.");
                continue;
            }
            var moraleBuff = self.axieSkillEffectManager.GetMoraleBuff();

            if (skillEffect.AlwaysCritical)
            {
                damagePair.damage *= Mathf.RoundToInt(AxieStatCalculator.GetCritDamage(self.stats, moraleBuff));
            }
            else if (self.Range >= 2 && self.axieSkillController.passives.rangedAlwaysCritical)
            {
                damagePair.damage *= Mathf.RoundToInt(AxieStatCalculator.GetCritDamage(self.stats, moraleBuff));
            }

            if (damagePair.axieController.axieSkillEffectManager.IsLethal() || UnityEngine.Random.Range(0, 1f) <= AxieStatCalculator.GetCritChance(self.stats, moraleBuff))
            {
                damagePair.damage *= Mathf.RoundToInt(AxieStatCalculator.GetCritDamage(self.stats, moraleBuff));
                damagePair.axieController.statsManagerUI.SetCritical();
                damagePair.axieController.axieSkillEffectManager.RemoveStatusEffect(StatusEffectEnum.Lethal);
            }
        }


        return damagePairList;
    }

    private void BuildSkillActions(Skill skillInstance, ref List<SkillAction> skillActions)
    {
        if (skillInstance.GetHealAction() != null)
        {
            skillActions.Add(skillInstance.GetHealAction());
        }

        if (skillInstance.GetDealDamageAction() != null)
        {
            skillActions.Add(skillInstance.GetDealDamageAction());
        }

        if (skillInstance.GetStatusEffectAction() != null)
        {
            skillActions.Add(skillInstance.GetStatusEffectAction());
        }

        if (skillInstance.GetAxieAnimationAction() != null)
        {
            skillActions.Add(skillInstance.GetAxieAnimationAction());
        }

        if (skillInstance.GetAllVFXActions() != null)
        {
            skillActions.AddRange(skillInstance.GetAllVFXActions());
        }

        skillActions.Add(skillInstance.GetDestroyAction());
    }

    private void PerformDamage(Skill skillInstance, List<DamagePair> damagePairs, SkillEffect skillEffect)
    {
        foreach (var damagePair in damagePairs)
        {
            skillInstance.AddDamageTargetPair(damagePair.axieController.AxieId, damagePair.damage,
                onlyShield: skillEffect.Fragile);
        }
    }

    private List<SkillAction> PerformPassive(AxieSkill skill, Skill skillInstance, AxieController self,
        AxieController target, bool multiCasted = false)
    {
        List<SkillAction> skillActions = new List<SkillAction>();
        List<DamagePair> damagePairs = new List<DamagePair>();
        foreach (var skillEffect in skill.bodyPartSO.skillEffects)
        {
            SpecialEffectExtras specialEffectExtras = HandleSpecialEffects(skillEffect, target, skill, self);

            List<AxieController> targets = new List<AxieController>();
            if (skillEffect.hasTriggerCondition)
            {
                if (FulfillsTriggerCondition(skill, skillInstance, self, target, skillEffect))
                {
                    targets = DoSkillEffect(self, target, skillEffect, skillInstance, specialEffectExtras);
                    if (!multiCasted && skillEffect.MultiCastTimes > 0)
                    {
                        for (int i = 0; i < skillEffect.MultiCastTimes; i++)
                        {
                            PerformSkill(skill, skillInstance, self, target, true);
                        }
                    }
                }
                else
                {
                    targets.Add(target);
                }

                if (skill.bodyPartSO.damage > 0)
                {
                    damagePairs = DamageCalculation(skill, skillInstance, self, targets, skillEffect, target,
                        specialEffectExtras, multiCasted);
                }

                skillInstance.targetList = targets;
            }
            else
            {
                targets = DoSkillEffect(self, target, skillEffect, skillInstance, specialEffectExtras);
                skillInstance.targetList = targets;

                if (!multiCasted && skillEffect.MultiCastTimes > 0)
                {
                    for (int i = 0; i < skillEffect.MultiCastTimes; i++)
                    {
                        PerformSkill(skill, skillInstance, self, target, true);
                    }
                }

                if (skill.bodyPartSO.damage > 0)
                {
                    damagePairs = DamageCalculation(skill, skillInstance, self, targets, skillEffect, target,
                        specialEffectExtras, multiCasted);
                }
            }

            if (damagePairs.Count > 0)
            {
                PerformDamage(skillInstance, damagePairs, skillEffect);
            }

            BuildSkillActions(skillInstance, ref skillActions);
        }

        return skillActions;
    }

    private List<SkillAction> PerformSkill(AxieSkill skill, Skill skillInstance, AxieController self,
        AxieController target, bool multiCasted = false)
    {
        List<SkillAction> skillActions = new List<SkillAction>();
        List<DamagePair> damagePairs = new List<DamagePair>();


        foreach (var skillEffect in skill.bodyPartSO.skillEffects)
        {
            if (skillEffect.isPassive)
                continue;
            SpecialEffectExtras specialEffectExtras = HandleSpecialEffects(skillEffect, target, skill, self);

            List<AxieController> targets = new List<AxieController>();

            if (skillEffect.hasTriggerCondition)
            {
                if (FulfillsTriggerCondition(skill, skillInstance, self, target, skillEffect))
                {
                    targets = DoSkillEffect(self, target, skillEffect, skillInstance, specialEffectExtras);
                    if (!multiCasted && skillEffect.MultiCastTimes > 0)
                    {
                        for (int i = 0; i < skillEffect.MultiCastTimes; i++)
                        {
                            PerformSkill(skill, skillInstance, self, target, true);
                        }
                    }
                }

                skillInstance.targetList = targets;
                damagePairs = DamageCalculation(skill, skillInstance, self, targets, skillEffect, target,
                    specialEffectExtras, multiCasted);
            }
            else
            {
                if (skillEffect.isPassive)
                    continue;


                targets = DoSkillEffect(self, target, skillEffect, skillInstance, specialEffectExtras);
                skillInstance.targetList = targets;


                if (!multiCasted && skillEffect.MultiCastTimes > 0)
                {
                    for (int i = 0; i < skillEffect.MultiCastTimes; i++)
                    {
                        PerformSkill(skill, skillInstance, self, target, true);
                    }
                }

                damagePairs = DamageCalculation(skill, skillInstance, self, targets, skillEffect, target,
                    specialEffectExtras, multiCasted);
            }

            PerformDamage(skillInstance, damagePairs, skillEffect);
            BuildSkillActions(skillInstance, ref skillActions);
        }

        return skillActions;
    }
}