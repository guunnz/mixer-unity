using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class AxieSkillEffectManager : MonoBehaviour
{
    public List<SkillEffect> skillEffects = new List<SkillEffect>();
    public List<SkillEffectGraphic> skillEffectGraphics = new List<SkillEffectGraphic>();
    public GameObject skillEffectGraphicPrefab;
    public Transform HorizontalLayoutGroup;
    private Transform mainCharacter;
    private int lastXAxisScale;

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

    public int GetAttackBuff()
    {
        return skillEffects.Sum(x => x.Attack);
    }

    public int GetMoraleBuff()
    {
        return skillEffects.Sum(x => x.Morale);
    }

    public int GetSpeedBuff()
    {
        return skillEffects.Sum(x => x.Speed);
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
    }

    public List<SkillEffect> GetAllSkillEffectsNotPassives()
    {
        return skillEffects.Where(x => x.isPassive == false).ToList();
    }

    public void AddStatusEffect(SkillEffect skillEffect)
    {
        SkillEffect skillEffectOnList = skillEffects.FirstOrDefault(x => x.statusEffect == skillEffect.statusEffect);

        if (skillEffectOnList != null)
        {
            switch (skillEffect.statusEffect)
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
                    skillEffectOnList.skillDuration = skillEffect.skillDuration;
                    return;
                default:
                    break;
            }
        }

        SkillEffectGraphic skillEffectGraphic =
            skillEffectGraphics.FirstOrDefault(x => x.statusEffect == skillEffect.statusEffect);

        if (skillEffectGraphic == null)
        {
            skillEffectGraphic =
                Instantiate(skillEffectGraphicPrefab, Vector3.zero, Quaternion.identity, HorizontalLayoutGroup)
                    .GetComponent<SkillEffectGraphic>();

            skillEffectGraphic.statusEffect = skillEffect.statusEffect;

            skillEffectGraphic.transform.localPosition = Vector3.zero;
            skillEffectGraphic.transform.localEulerAngles = Vector3.zero;

            skillEffectGraphics.Add(skillEffectGraphic);

            skillEffectGraphic.SetSprite(StatusManager.Instance.skillEffects.statusEffectGraphicsList
                .FirstOrDefault(x => x.statusEffectEnum == skillEffect.statusEffect)?.sprite);
        }
        else
        {
            SkillEffect skillEffectCounter = null;
            switch (skillEffect.statusEffect)
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

    public void RemoveStatusEffect(StatusEffectEnum statusEffect)
    {
        SkillEffect skillEffect = skillEffects.FirstOrDefault(x => x.statusEffect == statusEffect);
        SkillEffectGraphic skillEffectGraphic = skillEffectGraphics.FirstOrDefault(x => x.statusEffect == statusEffect);
        skillEffects.Remove(skillEffect);
        skillEffectGraphics.Remove(skillEffectGraphic);
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