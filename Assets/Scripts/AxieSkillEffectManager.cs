using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;


public class SkillEffectDuration
{
    public StatusEffectEnum effect;
    public float duration;

    public SkillEffectDuration(StatusEffectEnum effect, float duration)
    {
        this.effect = effect;
        this.duration = duration;
    }
}

public class AxieSkillEffectManager : MonoBehaviour
{
    public List<SkillEffect> skillEffects = new List<SkillEffect>();

    private List<SkillEffectDuration> skillEffectDurationList = new List<SkillEffectDuration>();

    public List<SkillEffectGraphic> skillEffectGraphics = new List<SkillEffectGraphic>();
    public GameObject skillEffectGraphicPrefab;
    public Transform HorizontalLayoutGroup;
    private Transform mainCharacter;
    private List<SkillEffectDuration> durationToRemove = new List<SkillEffectDuration>();
    private int lastXAxisScale;

    //
    public bool IsDebuff()
    {
        return skillEffects.Any(x =>
            x.Aroma || x.Chill || x.Fear || x.Fragile || x.Jinx || x.Lethal || x.Poison || x.Stun || x.Sleep ||
            x.Stench || x.Attack < 0 || x.Morale < 0 || x.Speed < 0);
    }

    public List<SkillEffect> GetAllBuffs()
    {
        return skillEffects.Where(x => x.Attack > 0 || x.Morale > 0 || x.Speed > 0).ToList();
    }

    public bool IsPoisoned()
    {
        return skillEffects.Any(x => x.Poison);
    }

    public bool IsMerry()
    {
        return skillEffects.Any(x => x.Merry);
    }

    public int PoisonStacks()
    {
        try
        {
            if (!IsPoisoned())
                return 0;

            return skillEffectGraphics.Single(x => x.statusEffect == StatusEffectEnum.Poison).Times;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return 0;
        }
    }

    public int MerryStacks()
    {
        try
        {
            if (!IsMerry())
                return 0;

            var merryGraphic = skillEffectGraphics.FirstOrDefault(x => x.statusEffect == StatusEffectEnum.Merry);
            return merryGraphic == null ? 0 : merryGraphic.Times;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return 0;
        }
    }

    public bool IsStunned()
    {
        return skillEffects.Any(x => x.Stun);
    }

    public bool IsFeared()
    {
        return skillEffects.Any(x => x.Fear);
    }

    public bool IsStenched()
    {
        return skillEffects.Any(x => x.Stench || x.statusEffect == StatusEffectEnum.Stench);
    }

    public bool IsLethal()
    {
        return skillEffects.Any(x => x.Lethal);
    }

    public bool IsJinxed()
    {
        return skillEffects.Any(x => x.Jinx);
    }

    public bool IsImmuneToDebuffs()
    {
        return skillEffects.Any(x => x.ImmuneToDebuffs);
    }

    public bool IsFragiled()
    {
        return skillEffects.Any(x => x.Fragile);
    }

    public bool IsHotbutted()
    {
        return skillEffects.Any(x => x.Hotbutt);
    }

    public bool IsHealingBlocked()
    {
        return skillEffects.Any(x => x.HealingBlock);
    }

    public bool IsKestreled()
    {
        try
        {
            return skillEffects.Any(x => x.Kestrel);

        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public int GeckoStacks()
    {
        int timesSet = skillEffects.FirstOrDefault(x => x.Gecko)?.timesSet ?? 0;

        return timesSet;
    }

    public bool IsFishSnacked()
    {
        return skillEffects.Any(x => x.FishSnack);
    }

    public bool IsGravelanted()
    {
        return skillEffects.Any(x => x.GravelAnt);
    }


    public int LungeAmount()
    {
        int timesSet = skillEffects.FirstOrDefault(x => x.Lunge)?.timesSet ?? 0;

        return timesSet;
    }

    public bool IsAromad()
    {
        return skillEffects.Any(x => x.Aroma);
    }

    public bool IsChilled()
    {
        return skillEffects.Any(x => x.Chill);
    }

    public int GetAttackBuff()
    {
        var attackPlus = skillEffects.FirstOrDefault(x => x.Attack > 0);
        var attackMinus = skillEffects.FirstOrDefault(x => x.Attack < 0);
        int attack = 0;
        if (attackPlus != null)
        {
            attack = attackPlus.timesSet;
        }
        if (attackMinus != null)
        {
            attack = -attackMinus.timesSet;
        }
        return attack;
    }

    public int GetMoraleBuff()
    {
        var moralePlus = skillEffects.FirstOrDefault(x => x.Morale > 0);
        var moraleMinus = skillEffects.FirstOrDefault(x => x.Morale < 0);
        int morale = 0;
        if (moralePlus != null)
        {
            morale = moralePlus.timesSet;
        }
        if (moraleMinus != null)
        {
            morale = -moraleMinus.timesSet;
        }
        return morale;
    }

    public int GetSpeedBuff()
    {
        var speedPlus = skillEffects.FirstOrDefault(x => x.Speed > 0);
        var speedMinus = skillEffects.FirstOrDefault(x => x.Speed < 0);
        int speed = 0;
        if (speedPlus != null)
        {
            speed = speedPlus.timesSet;
        }
        if (speedMinus != null)
        {
            speed = -speedMinus.timesSet;
        }
        return speed;
    }

    private void Start()
    {
        BoneFollower boneFollower = this.gameObject.AddComponent<BoneFollower>();

        if (boneFollower.skeletonRenderer == null)
            boneFollower.skeletonRenderer = transform.parent.GetComponent<SkeletonRenderer>();

        boneFollower.boneName = "@body";
        boneFollower.followBoneRotation = false;
        boneFollower.followXYPosition = true;
        boneFollower.followZPosition = false;
        boneFollower.followSkeletonFlip = false;
        boneFollower.yOffset = 4.5f;

        mainCharacter = transform.parent.parent;
        int scaleWished = mainCharacter.transform.localScale.x < 0 ? -1 : 1;

        this.transform.localScale =
            new Vector3(scaleWished, this.transform.localScale.y, this.transform.localScale.z);
        lastXAxisScale = scaleWished;
    }


    private void Update()
    {
        int scaleWished = mainCharacter.transform.localScale.x < 0 ? -1 : 1;
        if (lastXAxisScale != scaleWished)
        {
            this.transform.localScale =
                new Vector3(scaleWished, this.transform.localScale.y, this.transform.localScale.z);
            lastXAxisScale = scaleWished;
        }

        ManageDurations();
    }

    private void ManageDurations()
    {
        foreach (var skillEffectPair in skillEffectDurationList)
        {
            skillEffectPair.duration -= Time.deltaTime;

            if (skillEffectPair.duration <= 0)
            {
                durationToRemove.Add(skillEffectPair);
                RemoveStatusEffect(skillEffectPair.effect);
            }
        }

        if (durationToRemove.Count > 0)
        {
            skillEffectDurationList.RemoveAll(x => durationToRemove.Contains(x));
        }
    }

    public List<SkillEffect> GetAllSkillEffectsNotPassives()
    {
        return skillEffects.Where(x => x.isPassive == false).ToList();
    }

    public bool StatusEffectIsBuff(StatusEffectEnum statusEffect)
    {
        return statusEffect == StatusEffectEnum.SpeedPositive || statusEffect == StatusEffectEnum.AttackPositive ||
               statusEffect == StatusEffectEnum.MoralePositive;
    }

    public void AddStatusEffect(SkillEffect skillEffect)
    {
        StatusEffectEnum statusEffect = skillEffect.statusEffect;
        SkillEffect clone = (SkillEffect)skillEffect.Clone();
        if (statusEffect == StatusEffectEnum.None)
        {
            Debug.LogWarning("Setting random effect");
            statusEffect = (StatusEffectEnum)UnityEngine.Random.Range(1, (int)StatusEffectEnum.Merry);
            if (skillEffect.RandomEffectIsBuff)
            {
                while (statusEffect == StatusEffectEnum.Sleep || statusEffect == StatusEffectEnum.Fragile ||
                       !StatusEffectIsBuff(statusEffect))
                {
                    statusEffect = (StatusEffectEnum)UnityEngine.Random.Range(1, (int)StatusEffectEnum.Merry);
                }
            }
            else if (skillEffect.RandomEffectIsDebuff)
            {
                while (statusEffect == StatusEffectEnum.Sleep || statusEffect == StatusEffectEnum.Fragile ||
                       StatusEffectIsBuff(statusEffect))
                {
                    statusEffect = (StatusEffectEnum)UnityEngine.Random.Range(1, (int)StatusEffectEnum.Merry);
                }
            }
            else
            {
                while (statusEffect == StatusEffectEnum.Sleep || statusEffect == StatusEffectEnum.Fragile)
                {
                    statusEffect = (StatusEffectEnum)UnityEngine.Random.Range(1, (int)StatusEffectEnum.Merry);
                }
            }


            clone.statusEffect = statusEffect;

            switch (statusEffect)
            {
                case StatusEffectEnum.Aroma:
                    clone.Aroma = true;
                    break;
                case StatusEffectEnum.Chill:
                    clone.Chill = true;
                    break;
                case StatusEffectEnum.Fear:
                    clone.Fear = true;
                    break;
                case StatusEffectEnum.Stench:
                    clone.Stench = true;
                    break;
                case StatusEffectEnum.Fragile:
                    clone.Fragile = true;
                    break;
                case StatusEffectEnum.Jinx:
                    clone.Jinx = true;
                    break;
                case StatusEffectEnum.Lethal:
                    clone.Lethal = true;
                    break;
                case StatusEffectEnum.Stun:
                    clone.Stun = true;
                    break;
                case StatusEffectEnum.Poison:
                    clone.Poison = true;
                    clone.PoisonStack = 1;
                    break;
                case StatusEffectEnum.AttackPositive:
                    clone.Attack = 1;
                    break;
                case StatusEffectEnum.SpeedPositive:
                    clone.Speed = 1;
                    break;
                case StatusEffectEnum.MoralePositive:
                    clone.Morale = 1;
                    break;
                case StatusEffectEnum.AttackNegative:
                    clone.Attack = -1;
                    break;
                case StatusEffectEnum.SpeedNegative:
                    clone.Speed = -1;
                    break;
                case StatusEffectEnum.MoraleNegative:
                    clone.Morale = -1;
                    break;
                default:
                    // If the statusEffect does not match any of the cases above,
                    // there's nothing to set, so we can return or break here.
                    return;
            }
        }

        SkillEffect skillEffectOnList = skillEffects.FirstOrDefault(x => x.statusEffect == statusEffect);

        if (IsImmuneToDebuffs() && skillEffectOnList.IsDebuff())
        {
            return;
        }
        if (skillEffectOnList != null)
        {
            switch (statusEffect)
            {
                case StatusEffectEnum.Aroma:
                case StatusEffectEnum.Chill:
                case StatusEffectEnum.Fear:
                case StatusEffectEnum.Stench:
                case StatusEffectEnum.Fragile:
                case StatusEffectEnum.Jinx:
                case StatusEffectEnum.Sleep:
                case StatusEffectEnum.Lethal:
                case StatusEffectEnum.Stun:
                case StatusEffectEnum.Kestrel:
                case StatusEffectEnum.HotButt:
                case StatusEffectEnum.CannotUseMeleeAbility:
                case StatusEffectEnum.HealingBlock:
                    skillEffectOnList.skillDuration = skillEffect.skillDuration == 0 ? 1 : skillEffect.skillDuration;
                    return;
                default:
                    break;
            }
        }

        skillEffects.Add(clone);


        SetSkillEffectDuration(clone, statusEffect);

        SkillEffectGraphic skillEffectGraphic =
            skillEffectGraphics.FirstOrDefault(x => x.statusEffect == statusEffect);
        SkillEffect skillEffectCounter = null;
        switch (statusEffect)
        {
            case StatusEffectEnum.AttackNegative:
                skillEffectCounter =
                    skillEffects.FirstOrDefault(x => x.statusEffect == StatusEffectEnum.AttackPositive);
                break;
            case StatusEffectEnum.AttackPositive:
                skillEffectCounter =
                    skillEffects.FirstOrDefault(x => x.statusEffect == StatusEffectEnum.AttackNegative);
                break;
            case StatusEffectEnum.MoraleNegative:
                skillEffectCounter =
                    skillEffects.FirstOrDefault(x => x.statusEffect == StatusEffectEnum.MoralePositive);
                break;
            case StatusEffectEnum.MoralePositive:
                skillEffectCounter =
                    skillEffects.FirstOrDefault(x => x.statusEffect == StatusEffectEnum.MoraleNegative);
                break;
            case StatusEffectEnum.SpeedNegative:
                skillEffectCounter =
                    skillEffects.FirstOrDefault(x => x.statusEffect == StatusEffectEnum.SpeedPositive);
                break;
            case StatusEffectEnum.SpeedPositive:
                skillEffectCounter =
                    skillEffects.FirstOrDefault(x => x.statusEffect == StatusEffectEnum.SpeedNegative);
                break;
            default:
                break;
        }

        if (skillEffectGraphic == null && skillEffectCounter == null)
        {
            skillEffectGraphic =
                Instantiate(skillEffectGraphicPrefab, Vector3.zero, Quaternion.identity, HorizontalLayoutGroup)
                    .GetComponent<SkillEffectGraphic>();

            skillEffectGraphic.statusEffect = statusEffect;

            skillEffectGraphic.transform.localPosition = Vector3.zero;
            skillEffectGraphic.transform.localEulerAngles = Vector3.zero;

            skillEffectGraphics.Add(skillEffectGraphic);

            skillEffectGraphic.SetSprite(StatusManager.Instance.skillEffects.statusEffectGraphicsList
                .FirstOrDefault(x => x.statusEffectEnum == statusEffect)?.sprite);
        }
        else
        {
            if (skillEffectCounter == null)
            {
                if (skillEffectOnList != null)
                {
                    skillEffectOnList.timesSet++;
                }

                skillEffectGraphic.IncreaseNumber(1);
            }
            else
            {
                skillEffectCounter.timesSet--;

                if (skillEffectCounter.timesSet <= 0)
                {
                    RemoveStatusEffect(skillEffectCounter.statusEffect);
                }
            }
        }
    }

    private void SetSkillEffectDuration(SkillEffect skillEffect, StatusEffectEnum statusEffectEnum)
    {
        float duration = skillEffect.skillDuration == 0
            ? statusEffectEnum == StatusEffectEnum.Stench || statusEffectEnum == StatusEffectEnum.Chill ? 5 : 999999
            : skillEffect.skillDuration;

        if (statusEffectEnum == StatusEffectEnum.Stun || statusEffectEnum == StatusEffectEnum.Fear)
        {
            duration = 1;
        }
        else if (skillEffect.IsOnlyInfiniteBuffsOrDebuff())
        {
            duration = 999999;
        }

        if (skillEffectDurationList.Any(x => x.effect == statusEffectEnum))
        {
            skillEffectDurationList.Single(x => x.effect == statusEffectEnum).duration = duration;
        }
        else
        {
            skillEffectDurationList.Add(new SkillEffectDuration(statusEffectEnum, duration));
        }
    }

    public void RemoveStatusEffect(StatusEffectEnum statusEffect)
    {
        SkillEffectGraphic skillEffectGraphic = skillEffectGraphics.FirstOrDefault(x => x.statusEffect == statusEffect);
        skillEffects.RemoveAll(x => x.statusEffect == statusEffect);
        skillEffectGraphics.RemoveAll(x => x.statusEffect == statusEffect);
        if (skillEffectGraphic != null) Destroy(skillEffectGraphic.gameObject);
    }

    public void RemoveAllEffects()
    {
        skillEffects.RemoveAll(x => !x.isPassive);
        foreach (var skillEffectGraphic in skillEffectGraphics)
        {
            Destroy(skillEffectGraphic.gameObject);
        }

        skillEffectGraphics.Clear();
    }
}