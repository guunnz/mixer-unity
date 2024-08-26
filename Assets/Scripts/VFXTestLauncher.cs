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
    private Coroutine playingCoroutine;
    public Coroutine playSkillCoroutine;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (playSkillCoroutine != null)
            {
                StopCoroutine(playSkillCoroutine);
            }
            playSkillCoroutine = StartCoroutine(PlaySkill());
        }
    }

    public IEnumerator PlaySkill(SkillName skillName = SkillName.Chubby, BodyPart bodyPart = BodyPart.Eyes)
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
            playingCoroutine = null;
        }

        if (skillName != SkillName.Chubby)
        {
            skillToCast = skillName;
        }

        AxieBodyPart part;
        if (bodyPart != BodyPart.Eyes)
        {
            part = skillList.axieBodyParts.FirstOrDefault(x => x.skillName == skillToCast && x.bodyPart == bodyPart);
        }
        else
        {

            part = skillList.axieBodyParts.FirstOrDefault(x => x.skillName == skillToCast);
        }


        Skill skill = Instantiate(part.prefab).GetComponent<Skill>();

        skill.debug = true;
        skill.axieBodyPart = part;
        skill.target = target.transform;
        skill.origin = castFrom.transform;
        skill.@class = part.bodyPartClass;
        skill.skeletonAnimation = castFrom;


        playingCoroutine = StartCoroutine(skill.LaunchSkillTest(true));

        while (playingCoroutine != null)
        {
            yield return new WaitForSeconds(skill.totalDuration + 0.5f);
            skill = Instantiate(part.prefab).GetComponent<Skill>();

            skill.debug = true;
            skill.axieBodyPart = part;
            skill.target = target.transform;
            skill.origin = castFrom.transform;
            skill.@class = part.bodyPartClass;
            skill.skeletonAnimation = castFrom;
            playingCoroutine = StartCoroutine(skill.LaunchSkillTest(true));
        }
    }
}
