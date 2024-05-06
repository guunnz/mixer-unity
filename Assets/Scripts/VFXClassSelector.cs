using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class VFXClassSelector : MonoBehaviour
{
    public string skinName;
    private SkeletonAnimation skeletonGraphic;
    public bool AquaticIsAqua = false;

    public void SetAnimation(AxieClass axieClass)
    {
        skeletonGraphic = GetComponent<SkeletonAnimation>();

        string skinNameString = axieClass.ToString().ToLower() + skinName;

        if (AquaticIsAqua)
        {
            skinNameString = skinNameString.Replace("aquatic", "aqua");
        }

        skeletonGraphic.initialSkinName = skinNameString;

        skeletonGraphic.Initialize(true);
    }
}