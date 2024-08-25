using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class VFXPlayer : MonoBehaviour
{
    public VFXTestLauncher launcher;
    public SkeletonAnimation myAxie;
    public SkeletonAnimation Olek;
    public Image PlayImage;
    public void PlayAxieVFX(SkillName skillName, BodyPart bodyPart, SkeletonDataAsset skeletonDataAsset)
    {
        myAxie.skeletonDataAsset = skeletonDataAsset;
        myAxie.Initialize(true);
        if (launcher.playSkillCoroutine != null)
        {
            launcher.StopCoroutine(launcher.playSkillCoroutine);
            launcher.playSkillCoroutine = null;
        }
        PlayImage.DOColor(new Color(1, 1, 1, 0), 0.5f);
        myAxie.enabled = true;
        Olek.enabled = true;
        launcher.playSkillCoroutine = StartCoroutine(launcher.PlaySkill(skillName, bodyPart));
    }

    public void SetUp(SkeletonDataAsset skeletonDataAsset)
    {
        myAxie.skeletonDataAsset = skeletonDataAsset;
        myAxie.Initialize(true);
        myAxie.enabled = true;
        Olek.enabled = true;
    }

    public void StopVFX()
    {
        PlayImage.DOColor(new Color(1, 1, 1, 0.5f), 0.5f);
        myAxie.enabled = false;
        Olek.enabled = false;
        if (launcher.playSkillCoroutine != null)
        {
          
            launcher.StopCoroutine(launcher.playSkillCoroutine);
            launcher.playSkillCoroutine = null;
        }

    }
}
