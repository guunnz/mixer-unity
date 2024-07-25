using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine;
using Spine.Unity;
using UnityEngine;

public class VFXSkinChanger : MonoBehaviour
{
    private SkeletonAnimation data;

    private void Awake()
    {
        data = GetComponent<SkeletonAnimation>();
    }

    public void ChangeBasedOnClass(AxieClass @class)
    {
        data.initialSkinName = data.skeletonDataAsset.GetSkeletonData(false).Skins
            .FirstOrDefault(x => x.Name.Contains(@class.ToString().ToLower()))
            ?.Name;
    }
}