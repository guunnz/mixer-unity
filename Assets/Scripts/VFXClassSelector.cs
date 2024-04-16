using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class VFXClassSelector : MonoBehaviour
{
    public string skinName;
    private SkeletonGraphic skeletonGraphic;

    public void SetAnimation(AxieClass axieClass)
    {
        skeletonGraphic = GetComponent<SkeletonGraphic>();

        skeletonGraphic.initialSkinName = axieClass.ToString().ToLower() + skinName;

        skeletonGraphic.Initialize(true);
    }
}