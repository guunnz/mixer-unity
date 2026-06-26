using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VFXTestLauncher : MonoBehaviour
{
    public SkillName skillToCast;
    public VanillaMonsterVisual castFrom;
    public VanillaMonsterVisual target;
    public MonsterBodyPartsManager skillList;
    private Coroutine playingCoroutine;
    private GameObject currentSkillObject;
    public Coroutine playSkillCoroutine;

    private void Awake()
    {
        ResolveSkillList();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopSkillPreview();

            playSkillCoroutine = StartCoroutine(PlaySkill());
        }
    }

    public IEnumerator PlaySkill(SkillName skillName = SkillName.Chubby, BodyPart bodyPart = BodyPart.Eyes)
    {
        EnsureActors();
        StopCurrentSkill();

        if (skillName != SkillName.Chubby)
        {
            skillToCast = skillName;
        }

        ResolveSkillList();
        if (skillList == null)
            yield break;

        MonsterBodyPart part;
        if (bodyPart != BodyPart.Eyes)
        {
            part = skillList.monsterBodyParts.FirstOrDefault(x => x.skillName == skillToCast && x.bodyPart == bodyPart);
        }
        else
        {

            part = skillList.monsterBodyParts.FirstOrDefault(x => x.skillName == skillToCast);
        }

        if (part == null || part.prefab == null)
            yield break;

        Skill skill = CreatePreviewSkill(part);
        if (skill == null)
            yield break;

        playingCoroutine = StartCoroutine(skill.LaunchSkillTest(true));

        while (playingCoroutine != null)
        {
            yield return new WaitForSeconds(skill.totalDuration + 0.5f);
            if (part.prefab == null)
                yield break;

            skill = CreatePreviewSkill(part);
            if (skill == null)
                yield break;
            playingCoroutine = StartCoroutine(skill.LaunchSkillTest(true));
        }
    }

    public void SetActors(VanillaMonsterVisual castActor, VanillaMonsterVisual targetActor)
    {
        castFrom = castActor;
        target = targetActor;
    }

    public void StopSkillPreview()
    {
        if (playSkillCoroutine != null)
        {
            StopCoroutine(playSkillCoroutine);
            playSkillCoroutine = null;
        }

        StopCurrentSkill();
    }

    private Skill CreatePreviewSkill(MonsterBodyPart part)
    {
        StopCurrentSkill();

        currentSkillObject = Instantiate(part.prefab);
        Skill skill = currentSkillObject.GetComponent<Skill>();
        if (skill == null)
        {
            Destroy(currentSkillObject);
            currentSkillObject = null;
            return null;
        }

        skill.debug = true;
        skill.monsterBodyPart = part;
        skill.target = target.transform;
        skill.origin = castFrom.transform;
        skill.@class = part.bodyPartClass;
        skill.visual = castFrom;
        return skill;
    }

    private void StopCurrentSkill()
    {
        if (playingCoroutine != null)
        {
            StopCoroutine(playingCoroutine);
            playingCoroutine = null;
        }

        if (currentSkillObject != null)
        {
            Destroy(currentSkillObject);
            currentSkillObject = null;
        }
    }

    private void EnsureActors()
    {
        if (castFrom == null)
        {
            castFrom = VanillaMonsterVisual.Create(transform, MonsterVisualDescriptor.Default(MonsterClass.Beast));
            castFrom.transform.localPosition = new Vector3(-1.5f, 0f, 0f);
        }

        if (target == null)
        {
            target = VanillaMonsterVisual.Create(transform, MonsterVisualDescriptor.Default(MonsterClass.Plant));
            target.transform.localPosition = new Vector3(1.5f, 0f, 0f);
        }
    }

    private void ResolveSkillList()
    {
        if (skillList != null)
            return;

        if (SkillLauncher.Instance != null && SkillLauncher.Instance.skillList != null)
        {
            skillList = SkillLauncher.Instance.skillList;
            return;
        }

        MonstersView monstersView = FindObjectOfType<MonstersView>(true);
        if (monstersView != null && monstersView.skillList != null)
        {
            skillList = monstersView.skillList;
            return;
        }

        AbilitiesManager abilitiesManager = FindObjectOfType<AbilitiesManager>(true);
        if (abilitiesManager != null && abilitiesManager.skillList != null)
        {
            skillList = abilitiesManager.skillList;
            return;
        }

        foreach (VFXTestLauncher launcher in FindObjectsOfType<VFXTestLauncher>(true))
        {
            if (launcher != this && launcher.skillList != null)
            {
                skillList = launcher.skillList;
                return;
            }
        }
    }
}
