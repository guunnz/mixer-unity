using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class AxieSkill
{
    public SkillName skillName;
    public BodyPart bodyPart;
    public AxieBodyPart bodyPartSO;
}

public class AxieSkillController : MonoBehaviour
{
    public List<AxieSkill> skillList = new List<AxieSkill>();

    private int comboCost;

    public int GetComboCost()
    {
        return comboCost;
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
    }

    //Set by UI
    public void SetAxieSkills(List<SkillName> skillNames, List<BodyPart> bodyParts)
    {
        skillList.Clear();

        foreach (var bodyPart in SkillLauncher.Instance.skillList.axieBodyParts
                     .Where(x => skillNames.Contains(x.skillName) && bodyParts.Contains(x.bodyPart)).ToList())
        {
            AxieSkill skill = new AxieSkill();
            skill.skillName = bodyPart.skillName;
            skill.bodyPartSO = bodyPart;
            skill.bodyPart = bodyPart.bodyPart;

            AddAndHandleSpecialCases(skill, SkillLauncher.Instance.skillList.axieBodyParts, bodyPart);
        }
    }
}