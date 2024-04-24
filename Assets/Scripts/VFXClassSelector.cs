using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class VFXClassSelector : MonoBehaviour
{
    public string skinName;
    private SkeletonAnimation skeletonGraphic;

    public void SetAnimation(AxieClass axieClass)
    {
        skeletonGraphic = GetComponent<SkeletonAnimation>();

        skeletonGraphic.initialSkinName = axieClass.ToString().ToLower() + skinName;

        skeletonGraphic.Initialize(true);
    }
}