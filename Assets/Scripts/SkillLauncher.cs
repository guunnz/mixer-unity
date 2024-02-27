using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CharacterInfo = finished3.CharacterInfo;

public class SkillLauncher : MonoBehaviour
{
    public AxieBodyPartsManager skillList;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ThrowSkill(SkillName.Anemone, AxieClass.Aquatic, BodyPart.Horn,
                FindObjectsByType<CharacterInfo>(FindObjectsSortMode.None)[0].transform,
                FindObjectsByType<CharacterInfo>(FindObjectsSortMode.None)[1].transform);
        }
    }

    public void ThrowSkill(SkillName skillName, AxieClass axieClass, BodyPart axiebodyPart, Transform target,
        Transform origin)
    {
        AxieBodyPart part = skillList.axieBodyParts.Single(x => x.skillName == skillName && x.bodyPart == axiebodyPart);


        Skill skill = Instantiate(part.prefab).GetComponent<Skill>();

        skill.bodyPart = axiebodyPart;
        skill.target = target;
        skill.origin = origin;
        skill.@class = axieClass;
    }
}