using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public float ThrowSkill(SkillName skillName, AxieClass axieClass, BodyPart axiebodyPart, Transform target,
        Transform origin, SkeletonAnimation skeletonAnimation, AxieController opponent, AxieController self)
    {
        AxieBodyPart part =
            skillList.axieBodyParts.FirstOrDefault(x => x.skillName == skillName && x.bodyPart == axiebodyPart);

        if (part == null || part.prefab == null)
        {
            Debug.Log(skillName + " " + axiebodyPart);
            return 0.5f;
        }
        
        

        Skill skill = Instantiate(part.prefab).GetComponent<Skill>();

        skill.axieBodyPart = part;
        skill.target = target;
        skill.origin = origin;
        skill.self = self;
        skill.@class = axieClass;
        skill.skeletonAnimation = skeletonAnimation;
        skill.opponent = opponent;

        return skill.totalDuration;
    }
}