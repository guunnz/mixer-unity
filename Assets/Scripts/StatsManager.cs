using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using Spine.Unity;
using TMPro;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public Rectangle HPRectangle;
    public Rectangle ManaRectangle;
    public SpriteRenderer sr;
    private int lastXAxisScale;
    private Transform mainCharacter;
    public TextMeshProUGUI shield;
    public GameObject shieldObject;
    BoneFollower boneFollower;

    private void Start()
    {
        boneFollower = this.gameObject.AddComponent<BoneFollower>();

        if (boneFollower.skeletonRenderer == null)
            boneFollower.skeletonRenderer = transform.parent.GetComponent<SkeletonRenderer>();

        boneFollower.boneName = "@body";
        boneFollower.followBoneRotation = false;
        boneFollower.followXYPosition = true;
        boneFollower.followZPosition = false;
        boneFollower.followSkeletonFlip = false;
        boneFollower.yOffset = 3;

        mainCharacter = transform.parent.parent;
        int scaleWished = mainCharacter.transform.localScale.x < 0 ? -3 : 3;

        this.transform.localScale =
            new Vector3(scaleWished, this.transform.localScale.y, this.transform.localScale.z);
        lastXAxisScale = scaleWished;
    }

    public void SetSR(Sprite sprite)
    {
        sr.sprite = sprite;
    }

    public void SetMana(float mana)
    {

        if (mana > 1)
        {
            mana = 1;
        }

        ManaRectangle.Width = mana;
    }

    public void SetHP(float mana)
    {

        if (mana > 1)
        {
            mana = 1;
        }

        HPRectangle.Width = mana;
    }

    public void SetShield(int shield)
    {

        if (shield <= 0)
        {
            shieldObject.SetActive(false);
        }
        else
        {
            shieldObject.SetActive(true);
        }

        this.shield.text = shield.ToString();
    }


    private void Update()
    {
        int scaleWished = mainCharacter.transform.localScale.x < 0 ? 3 : -3;
        if (lastXAxisScale != scaleWished)
        {
            this.transform.localScale =
                new Vector3(scaleWished, this.transform.localScale.y, this.transform.localScale.z);
            lastXAxisScale = scaleWished;
        }
    }
}