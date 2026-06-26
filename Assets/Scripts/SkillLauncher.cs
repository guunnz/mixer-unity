using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using finished3;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class DamagePair
{
    public MonsterController monsterController;
    public int damage;
}

public class SpecialEffectExtras
{
    public MonsterController monsterController;
    public int extraDamage;
    public int multiplyStatusEffect = 1;
}

public class SkillLauncher : MonoBehaviour
{
    public MonsterBodyPartsManager skillList;

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

    public IEnumerator ThrowSkill(List<MonsterSkill> skills, VanillaMonsterVisual visual, MonsterController target,
        MonsterController self)
    {
        List<SkillAction> skillActions = new List<SkillAction>();
        float TotalDuration = 0;
        for (int i = 0; i < skills.Count; i++)
        {
            Skill skill = Instantiate(skills[i].bodyPartSO.prefab).GetComponent<Skill>();

            skill.monsterBodyPart = skills[i].bodyPartSO;
            if (self.monsterSkillEffectManager.IsKestreled() && skill.monsterBodyPart.bodyPart == BodyPart.Horn)
            {
                continue;
            }
            else if (self.monsterSkillEffectManager.IsHotbutted() && skill.monsterBodyPart.bodyPart == BodyPart.Mouth)
            {
                continue;
            }
            skill.self = self;
            skill.origin = self.transform;
            skill.@class = skills[i].bodyPartSO.bodyPartClass;
            skill.visual = visual;
            skill.ExtraTimerCast += (0.55f * i);
            //Debug.Log("Skill performed: " + skills[i].skillName);
            for (int b = 0; b < skills[i].bodyPartSO.skillEffects.Count(); b++)
            {
                if (Backdoor(skills[i].bodyPartSO.skillEffects[b], self))
                {
                    yield return StartCoroutine(self.monsterBehavior.GoBackdoorTarget());
                }
            }


            skillActions.AddRange(PerformSkill(skills[i], skill, self, target));
            TotalDuration += skill.totalDuration;
        }

        StartCoroutine(DoSkills(skillActions));

        yield return new WaitForSeconds(0.6f * skills.Count);
    }


    private bool Backdoor(SkillEffect skillEffect, MonsterController self)
    {
        if (skillEffect.Prioritize)
        {
            var target = self.CurrentTarget;
            if (skillEffect.lowestHP)
            {
                MonsterController newTarget = self.enemyTeam.GetAliveCharacters().OrderBy(x => x.monsterIngameStats.currentHP)
                    .First();

                self.CurrentTarget = newTarget;

                return true;
            }

            if (skillEffect.FurthestTarget)
            {
                MonsterController newTarget = self.myTeam.FindFurthestCharacter(self, self.enemyTeam.GetAliveCharacters());
                if (newTarget != null)
                {
                    self.CurrentTarget = newTarget;

                    return true;
                }

            }

            if (skillEffect.targetHighestEnergy)
            {
                MonsterController newTarget = self.enemyTeam.GetAliveCharacters()
                    .OrderBy(x => x.monsterIngameStats.CurrentEnergy)
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
                MonsterController newTarget = self.enemyTeam.GetAliveCharacters().OrderBy(x =>
                        MonsterStatCalculator.GetRealSpeed(x.stats.speed, x.monsterSkillEffectManager.GetSpeedBuff()))
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

            if (skillEffect.targetMonsterClass)
            {
                if (target.myTeam.GetAliveCharacters().Any(y =>
                        y.monsterIngameStats.monsterClass == skillEffect.monsterClassToTarget))
                {
                    MonsterController newTarget = target.myTeam.GetAliveCharacters().FirstOrDefault(y =>
                        y.monsterIngameStats.monsterClass == skillEffect.monsterClassToTarget);
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

    public IEnumerator ThrowPassive(MonsterSkill passive, VanillaMonsterVisual visual, MonsterController target,
        MonsterController self)
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
                yield return StartCoroutine(self.monsterBehavior.GoBackdoorTarget());
            }
        }

        skill.monsterBodyPart = passive.bodyPartSO;
        skill.self = self;
        skill.origin = self.transform;
        skill.@class = passive.bodyPartSO.bodyPartClass;
        skill.visual = visual;
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

    private SpecialEffectExtras HandleSpecialEffects(SkillEffect skillEffect, MonsterController target, MonsterSkill skill,
        MonsterController self)
    {
        SpecialEffectExtras effectExtras = new SpecialEffectExtras();

        if (skillEffect.specialActivationAgainstMonstersList.Count > 0)
        {
            var specialActivationAgainstMonsterClass = skillEffect.specialActivationAgainstMonstersList.FirstOrDefault(x =>
                x.monsterClass == target.monsterIngameStats.monsterClass);
            if (specialActivationAgainstMonsterClass != null)
            {
                effectExtras.extraDamage += specialActivationAgainstMonsterClass.ExtraDamage;
                effectExtras.multiplyStatusEffect += specialActivationAgainstMonsterClass.ExtraTimesStatusEffectApplied;
            }
        }

        if (skillEffect.specialActivationIfComboedWithList.Count > 0)
        {
            bool comboed = false;
            foreach (var skillToComboWith in skillEffect.specialActivationIfComboedWithList)
            {
                if (skillToComboWith.OnlyCareAboutClassCard)
                {
                    if (self.monsterSkillController.IsClassSkillInMonster(skillToComboWith.monsterClassCard, skill))
                    {
                        effectExtras.extraDamage += skillToComboWith.ExtraDamage;
                        effectExtras.multiplyStatusEffect += skillToComboWith.ExtraTimesStatusEffectApplied;
                    }
                }
                else if (self.monsterSkillController.IsSkillSelectedInMonster(skillToComboWith.monsterCard, skill))
                {
                    effectExtras.extraDamage += skillToComboWith.ExtraDamage;
                    effectExtras.multiplyStatusEffect += skillToComboWith.ExtraTimesStatusEffectApplied;
                }
            }
        }

        if (skillEffect.specialActivationBasedOnMonstersInBattle.Count > 0)
        {
            foreach (var bodyPart in skillEffect.specialActivationBasedOnMonstersInBattle)
            {
                int monsterTypeMultiplier =
                    self.badTeam.GetAliveCharacters().Count(x => x.monsterIngameStats.monsterClass == bodyPart.monsterClass) + self
                        .goodTeam.GetAliveCharacters().Count(x => x.monsterIngameStats.monsterClass == bodyPart.monsterClass);

                effectExtras.multiplyStatusEffect += bodyPart.ExtraTimesStatusEffectAppliedPerMonster * monsterTypeMultiplier;
                effectExtras.extraDamage += bodyPart.ExtraDamageAppliedPerMonster * monsterTypeMultiplier;
            }
        }

        if (skillEffect.specialActivationWithBodyParts.Count > 0)
        {
            foreach (var bodyPart in skillEffect.specialActivationWithBodyParts)
            {
                if (bodyPart.OnlyCareAboutClassCard)
                {
                    if (self.monsterBodyParts.Contains(bodyPart.monsterCard))
                    {
                        effectExtras.extraDamage += bodyPart.ExtraDamage;
                        effectExtras.multiplyStatusEffect += bodyPart.ExtraTimesStatusEffectApplied;
                    }
                }
                else if (self.monsterSkillController.IsSkillSelectedInMonster(bodyPart.monsterCard, skill))
                {
                    effectExtras.extraDamage += bodyPart.ExtraDamage;
                    effectExtras.multiplyStatusEffect += bodyPart.ExtraTimesStatusEffectApplied;
                }
            }
        }

        return effectExtras;
    }

    private bool FulfillsTriggerCondition(MonsterSkill skill, Skill skillInstance, MonsterController self,
        MonsterController target, SkillEffect skillEffect)
    {
        IngameStats myMonsterData = self.monsterIngameStats;

        if (skillEffect.UseSpecialsAsTrigger)
        {
            if (skillEffect.specialActivationAgainstMonstersList.Count > 0)
            {
                if (skillEffect.specialActivationAgainstMonstersList.Any(x =>
                        x.monsterClass == target.monsterIngameStats.monsterClass) == false)
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
                        if (self.monsterSkillController.IsClassSkillInMonster(skillToComboWith.monsterClassCard, skill))
                        {
                            comboed = true;
                            break;
                        }
                    }
                    else if (self.monsterSkillController.IsSkillSelectedInMonster(skillToComboWith.monsterCard, skill))
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
                        if (self.monsterBodyParts.Contains(bodyPart.monsterCard))
                        {
                            activated = true;
                            break;
                        }
                    }
                    else if (self.monsterSkillController.IsSkillSelectedInMonster(bodyPart.monsterCard, skill))
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

            if (skillEffect.specialActivationBasedOnMonstersInBattle.Count > 0)
            {
                bool activated = false;
                foreach (var specialActivation in skillEffect.specialActivationBasedOnMonstersInBattle)
                {
                    if (self.goodTeam.GetAliveCharacters()
                            .Any(x => x.monsterIngameStats.monsterClass == specialActivation.monsterClass) || self.badTeam
                            .GetAliveCharacters().Any(x => x.monsterIngameStats.monsterClass == specialActivation.monsterClass))
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
                    ? myMonsterData.currentHP < myMonsterData.maxHP * (skillEffect.HPTresholdPercentage / 100f)
                    : myMonsterData.currentHP > myMonsterData.maxHP * (skillEffect.HPTresholdPercentage / 100f))
            {
                return false;
            }
        }


        if (skillEffect.OnShieldBreak)
        {
            if (target.monsterIngameStats.currentShield != 0)
            {
                return false;
            }
        }

        if (skillEffect.ComboAmount > 0)
        {
            if (self.monsterSkillController.GetComboAmount() < skillEffect.ComboAmount)
            {
                return false;
            }
        }

        if (skillEffect.LastMonsterAliveTeam)
        {
            if (self.goodTeam.MonsterAliveAmount != 1)
            {
                return false;
            }
        }

        if (skillEffect.LastMonsterAliveOpponent)
        {
            if (self.badTeam.MonsterAliveAmount != 1)
            {
                return false;
            }
        }

        if (skillEffect.Shielded)
        {
            if (target.monsterIngameStats.currentShield <= 0)
            {
                return false;
            }
        }

        if (skillEffect.TargetIsDebuff)
        {
            if (!target.monsterSkillEffectManager.IsDebuff())
            {
                return false;
            }
        }

        if (skillEffect.SelfIsDebuff)
        {
            if (!self.monsterSkillEffectManager.IsDebuff())
            {
                return false;
            }
        }

        if (skillEffect.TargetIsPoisoned)
        {
            if (!target.monsterSkillEffectManager.IsPoisoned())
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
                    MonsterStatCalculator.GetRealAttack(self.stats, self.monsterSkillEffectManager.GetAttackBuff(), self.monsterSkillEffectManager.GetMoraleBuff(), self.monsterSkillEffectManager.GetSpeedBuff());
                if ((skillEffect.AttackStatDifference < 0 && realAttack >= 0) ||
                    (skillEffect.AttackStatDifference > 0 && realAttack <= 0))
                {
                    return false;
                }
            }


            if (skillEffect.SpeedStatDifference != 0)
            {
                int realSpeed =
                    MonsterStatCalculator.GetRealSpeed(self.stats.speed, self.monsterSkillEffectManager.GetSpeedBuff());
                if ((skillEffect.SpeedStatDifference < 0 && realSpeed >= 0) ||
                    (skillEffect.SpeedStatDifference > 0 && realSpeed <= 0))
                {
                    return false;
                }
            }

            if (skillEffect.MoraleStatDifference != 0)
            {
                int realMorale =
                    MonsterStatCalculator.GetRealMorale(self.stats.morale, self.monsterSkillEffectManager.GetMoraleBuff());
                if ((skillEffect.MoraleStatDifference < 0 && realMorale >= 0) ||
                    (skillEffect.MoraleStatDifference > 0 && realMorale <= 0))
                {
                    return false;
                }
            }

            if (skillEffect.CurrentHPStatDifference != 0)
            {
                if ((skillEffect.CurrentHPStatDifference < 0 &&
                     self.monsterIngameStats.currentHP >= target.monsterIngameStats.currentHP) ||
                    (skillEffect.CurrentHPStatDifference >= 0 &&
                     self.monsterIngameStats.currentHP <= target.monsterIngameStats.currentHP))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private List<MonsterController> DoSkillEffect(MonsterController self,
        MonsterController target, SkillEffect skillEffect, Skill skillInstance, SpecialEffectExtras specialEffectExtras)
    {
        try
        {
            List<MonsterController> statusEffectTargetList = new List<MonsterController>();
            List<MonsterController> targetList = new List<MonsterController>();

            bool remove = false;
            bool steal = false;
            targetList.Add(self.CurrentTarget);

            switch (skillEffect.statusApplyType)
            {
                case StatusApplyType.AllMonsters:
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
                            skillInstance.AddStatusEffectTargetPair(x.MonsterId, new[] { skillEffect }, remove);
                        }
                    });
                }
                else
                {
                    MonsterController stealer = statusEffectTargetList[0];
                    MonsterController gotStolen = statusEffectTargetList[1];

                    List<SkillEffect> skillsToSteal = gotStolen.monsterSkillEffectManager.GetAllBuffs();

                    skillInstance.AddStatusEffectTargetPair(gotStolen.MonsterId, skillsToSteal.ToArray(), true);
                    skillInstance.AddStatusEffectTargetPair(stealer.MonsterId, skillsToSteal.ToArray(), false);
                }

                if (target.monsterSkillEffectManager.IsFishSnacked() && target.myTeam != self.myTeam)
                {
                    if (skillInstance.monsterBodyPart.bodyPartClass == MonsterClass.Bird || skillInstance.monsterBodyPart.bodyPartClass == MonsterClass.Aquatic)
                    {
                        skillInstance.AddStatusEffectTargetPair(self.MonsterId, new[] {new SkillEffect()
                { statusEffect = StatusEffectEnum.Stun, Stun = true, skillDuration = 2 }  });

                        skillInstance.AddStatusEffectTargetPair(target.MonsterId, new[] {new SkillEffect()
                { statusEffect = StatusEffectEnum.FishSnack,}  }, remove: true);
                    }
                }
            }

            if (skillEffect.StealEnergyPercentage > 0)
            {
                float energyToTransfer = target.monsterIngameStats.CurrentEnergy *
                                         (skillEffect.StealEnergyPercentage * 0.01f) *
                                         specialEffectExtras.multiplyStatusEffect;

                target.monsterIngameStats.CurrentEnergy -= energyToTransfer;
                self.monsterIngameStats.CurrentEnergy += energyToTransfer;
            }

            if (skillEffect.GainEnergy > 0)
            {
                targetList.ForEach(x =>
                {
                    x.monsterIngameStats.CurrentEnergy += (x.monsterIngameStats.MaxEnergy * skillEffect.GainEnergy);
                });
            }


            if (skillInstance.monsterBodyPart.bodyPart == BodyPart.Horn)
            {
                self.monsterIngameStats.currentShield += skillInstance.monsterBodyPart.shield *
                                                      ((self.monsterSkillController.passives.ExtraArmorHelmet / 100f) == 0 ? 1 : (self.monsterSkillController.passives.ExtraArmorHelmet / 100f));
            }
            else
            {
                self.monsterIngameStats.currentShield += skillInstance.monsterBodyPart.shield;
            }

            if (skillEffect.GainShield > 0)
            {
                if (skillEffect.lowestHP)
                {
                    var monster = self.myTeam.GetCharactersAll().OrderBy(x => x.monsterIngameStats.currentHP).First();
                    monster.monsterIngameStats.currentShield += (skillEffect.GainShield);
                }
                else
                {
                    self.monsterIngameStats.currentShield += skillEffect.GainShield;
                }
            }
            if (self.monsterSkillController.passives.ExtraShieldGained > 0)
            {
                self.monsterIngameStats.currentShield += skillInstance.monsterBodyPart.shield * (self.monsterSkillController.passives.ExtraShieldGained / 100f);

            }

            if (self.monsterIngameStats.currentShield > 10000)
            {
                Debug.LogError("EXTRA SHIELD GAIN : " + self.monsterSkillController.passives.ExtraShieldGained);
                Debug.LogError("EXTRA SHIELD HELMET : " + self.monsterSkillController.passives.ExtraArmorHelmet);
                Debug.LogError("MATH1 : " + skillInstance.monsterBodyPart.shield * (self.monsterSkillController.passives.ExtraArmorHelmet / 100f));
                Debug.LogError("MATH2 : " + 1 + (self.monsterSkillController.passives.ExtraShieldGained / 100f));
                Debug.LogError("SHIELD SKILL: " + skillInstance.monsterBodyPart.shield);
                self.monsterIngameStats.currentShield = 1000;
            }
            if (skillEffect.GainHPPercentage > 0)
            {
                if (!skillEffect.HPBaseOnDamage)
                {
                    if (skillEffect.lowestHP)
                    {
                        var monster = self.myTeam.GetCharactersAll().OrderBy(x => x.monsterIngameStats.currentHP).First();
                        skillInstance.AddHealTargetPair(monster.MonsterId, (monster.monsterIngameStats.maxHP *
                                                                                   (skillEffect.GainHPPercentage *
                                                                                       0.01f)) *
                                                                               specialEffectExtras
                                                                                   .multiplyStatusEffect);
                    }
                    else
                    {
                        foreach (var monsterController in statusEffectTargetList)
                        {
                            skillInstance.AddHealTargetPair(monsterController.MonsterId, (monsterController.monsterIngameStats.maxHP *
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

    private List<DamagePair> DamageCalculation(MonsterSkill skill, Skill skillInstance, MonsterController self,
        List<MonsterController> targets, SkillEffect skillEffect, MonsterController originalTarget,
        SpecialEffectExtras specialEffectExtras, bool multiCasted = false)
    {
        List<DamagePair> damagePairList = new List<DamagePair>();
        foreach (var target in targets)
        {
            try
            {
                DamagePair dmgPair = new DamagePair();
                int damageReduction = target.monsterSkillController.passives.DamageReductionAmount + (target.monsterSkillEffectManager.GeckoStacks() * 10);
                dmgPair.monsterController = target;
                dmgPair.damage = Mathf.RoundToInt(skill.bodyPartSO.damage);



                if (skillEffect.DamageEqualsBasicAttack)
                {
                    dmgPair.damage =
                        MonsterStatCalculator.GetRealAttack(self.stats, self.monsterSkillEffectManager.GetAttackBuff(), self.monsterSkillEffectManager.GetMoraleBuff(), self.monsterSkillEffectManager.GetSpeedBuff());
                }

                if (skillEffect.ExtraDamagePercentage > 0)
                {
                    dmgPair.damage *= Mathf.RoundToInt(1f + (skillEffect.ExtraDamagePercentage * 0.01f));
                }

                if (skillEffect.HPBaseOnDamage)
                {
                    skillInstance.AddHealTargetPair(self.MonsterId,
                        dmgPair.damage * (skillEffect.GainHPPercentage * 0.01f *
                         specialEffectExtras.multiplyStatusEffect == 0 ? 1 : specialEffectExtras.multiplyStatusEffect));
                }

                dmgPair.damage *= Mathf.RoundToInt(1f + (specialEffectExtras.extraDamage * .01f));

                if (skillEffect.ShieldAsDamagePercentage > 0)
                {
                    var shield = self.monsterIngameStats.currentShield;
                    shield *= skillEffect.ShieldAsDamagePercentage * .01f;
                    dmgPair.damage += Mathf.RoundToInt(shield);
                }



                if (skill.bodyPartSO.skillEffects.Any(x => x.Lunge))
                {
                    int lungeAmount = self.monsterSkillEffectManager.LungeAmount();
                    if (lungeAmount > 0)
                    {
                        dmgPair.damage += dmgPair.damage * (int)Math.Ceiling(lungeAmount * MonsterStatCalculator.LungePercentage);
                    }
                }

                if (skill.bodyPartSO.skillEffects.Any(x => x.Trump))
                {
                    int economyAmount = self.imGood ? RunManagerSingleton.instance.netWorth : RunManagerSingleton.instance.eNetWorth;

                    dmgPair.damage += dmgPair.damage * (int)Math.Ceiling(economyAmount * MonsterStatCalculator.LungePercentage);
                }

                var extraDamageByAbilities = target.monsterSkillController.passives.ExtraDamageReceivedByAbilitiesAmount;

                if (extraDamageByAbilities > 0)
                {
                    dmgPair.damage *= Mathf.RoundToInt((extraDamageByAbilities / 100f) + 1f);
                }

                if (target.monsterSkillEffectManager.IsAromad())
                {
                    damageReduction -= 50;
                }


                dmgPair.damage -= (dmgPair.damage * (damageReduction / 100));

                if (skill.bodyPartSO.OnlyDamageShields)
                {
                    if (dmgPair.damage > Mathf.RoundToInt(target.monsterIngameStats.currentShield))
                    {
                        dmgPair.damage = Mathf.RoundToInt(target.monsterIngameStats.currentShield);
                    }
                }

                damagePairList.Add(dmgPair);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        if (self.monsterSkillEffectManager.IsJinxed())
            return damagePairList;

        //Calculate crits
        foreach (var damagePair in damagePairList)
        {
            if (damagePair.monsterController.monsterSkillController.passives.ImmuneToCriticals)
            {
                Debug.Log("Immune to crits.");
                continue;
            }
            var moraleBuff = self.monsterSkillEffectManager.GetMoraleBuff();

            if (skillEffect.AlwaysCritical)
            {
                damagePair.damage *= Mathf.RoundToInt(MonsterStatCalculator.GetCritDamage(self.stats, moraleBuff));
            }
            else if (self.Range >= 2 && self.monsterSkillController.passives.rangedAlwaysCritical)
            {
                damagePair.damage *= Mathf.RoundToInt(MonsterStatCalculator.GetCritDamage(self.stats, moraleBuff));
            }

            if (damagePair.monsterController.monsterSkillEffectManager.IsLethal() || UnityEngine.Random.Range(0, 1f) <= MonsterStatCalculator.GetCritChance(self.stats, moraleBuff))
            {
                damagePair.damage *= Mathf.RoundToInt(MonsterStatCalculator.GetCritDamage(self.stats, moraleBuff));
                damagePair.monsterController.statsManagerUI.SetCritical();
                damagePair.monsterController.monsterSkillEffectManager.RemoveStatusEffect(StatusEffectEnum.Lethal);
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

        if (skillInstance.GetMonsterAnimationAction() != null)
        {
            skillActions.Add(skillInstance.GetMonsterAnimationAction());
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
            skillInstance.AddDamageTargetPair(damagePair.monsterController.MonsterId, damagePair.damage,
                onlyShield: skillEffect.Fragile);
        }
    }

    private List<SkillAction> PerformPassive(MonsterSkill skill, Skill skillInstance, MonsterController self,
        MonsterController target, bool multiCasted = false)
    {
        List<SkillAction> skillActions = new List<SkillAction>();
        List<DamagePair> damagePairs = new List<DamagePair>();
        foreach (var skillEffect in skill.bodyPartSO.skillEffects)
        {
            SpecialEffectExtras specialEffectExtras = HandleSpecialEffects(skillEffect, target, skill, self);

            List<MonsterController> targets = new List<MonsterController>();
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

    private List<SkillAction> PerformSkill(MonsterSkill skill, Skill skillInstance, MonsterController self,
        MonsterController target, bool multiCasted = false)
    {
        List<SkillAction> skillActions = new List<SkillAction>();
        List<DamagePair> damagePairs = new List<DamagePair>();


        foreach (var skillEffect in skill.bodyPartSO.skillEffects)
        {
            if (skillEffect.isPassive)
                continue;
            SpecialEffectExtras specialEffectExtras = HandleSpecialEffects(skillEffect, target, skill, self);

            List<MonsterController> targets = new List<MonsterController>();

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
