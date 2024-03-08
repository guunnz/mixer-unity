using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using Spine.Unity;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public Rectangle HPRectangle;
    public Rectangle ManaRectangle;
    public SpriteRenderer sr;

    private void Start()
    {
        BoneFollower boneFollower = this.gameObject.AddComponent<BoneFollower>();

        if (boneFollower.skeletonRenderer == null)
            boneFollower.skeletonRenderer = transform.parent.GetComponent<SkeletonRenderer>();

        boneFollower.boneName = "@body";
        boneFollower.followBoneRotation = false;
        boneFollower.followXYPosition = true;
        boneFollower.followZPosition = false;
        boneFollower.followSkeletonFlip = false;
        boneFollower.yOffset = 3;
        
    }

    public void SetSR(Sprite sprite)
    {
        sr.sprite = sprite;
    }

    public void SetMana(float mana)
    {
        ManaRectangle.Width = mana;
    }

    public void SetHP(float mana)
    {
        HPRectangle.Width = mana;
    }
}