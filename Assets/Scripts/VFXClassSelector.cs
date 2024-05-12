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

        if (axieClass == AxieClass.Dusk)
        {
            axieClass = AxieClass.Reptile;
        }
        else if (axieClass == AxieClass.Mech)
        {
            axieClass = AxieClass.Beast;
        }
        else if (axieClass == AxieClass.Dawn)
        {
            axieClass = AxieClass.Bird;
        }

        string skinNameString = axieClass.ToString().ToLower() + skinName;
        if (AquaticIsAqua)
        {
            skinNameString = skinNameString.Replace("aquatic", "aqua");
        }

        skeletonGraphic.initialSkinName = skinNameString;

        skeletonGraphic.Initialize(true);
    }
}