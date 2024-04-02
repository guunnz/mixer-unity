using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;

public class VFXTestLauncher : MonoBehaviour
{
    public SkillName skillToCast;
    public SkeletonAnimation castFrom;
    public SkeletonAnimation target;
    public AxieBodyPartsManager skillList;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AxieBodyPart part =
                skillList.axieBodyParts.FirstOrDefault(x => x.skillName == skillToCast);
            
            Skill skill = Instantiate(part.prefab).GetComponent<Skill>();

            skill.debug = true;
            skill.axieBodyPart = part;
            skill.target = target.transform;
            skill.origin = castFrom.transform;
            skill.@class = part.bodyPartClass;
            skill.skeletonAnimation = castFrom;

            StartCoroutine(skill.LaunchSkillTest());
        }
    }
}
