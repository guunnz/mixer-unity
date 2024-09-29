using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using Spine.Unity;
using TMPro;
using UnityEngine;
using DG.Tweening; // Import the DoTween namespace at the top
public class StatsManager : MonoBehaviour
{
    public Rectangle HPRectangle;
    public SpriteRenderer sr;
    private int lastXAxisScale;
    private Transform mainCharacter;
    public TextMeshProUGUI shield;
    public GameObject shieldObject;
    public SpriteRenderer Selected;
    BoneFollower boneFollower;
    private int ManaBars = 1;
    public Rectangle[] ManaBarsList;
    private float ManaBarSpacing = 0.05f;
    private float TotalEnergyBarSize = 1.2709f;
    public Transform AttackAnim;
    private float ManaPerBar = 0.5f;
    internal float shieldValue;

    private float CurrentMana;


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
    public void SetManaBars(int manaBars)
    {
        ManaBars = manaBars;

        ManaBarsList.ToList().ForEach(x => x.gameObject.SetActive(false));
        ManaPerBar = 1f / manaBars;
        for (int i = 0; i < manaBars; i++)
        {
            ManaBarsList[i].gameObject.SetActive(true);
            var manaBar = ManaBarsList[i].transform;

            if (i != 0)
            {
                manaBar.localScale = -new Vector3(TotalEnergyBarSize / manaBars - ManaBarSpacing, -0.5f, -1);
                manaBar.localPosition = new Vector3(0.4739688f - (ManaBarSpacing) - ((TotalEnergyBarSize / manaBars) * i), manaBar.localPosition.y, manaBar.localPosition.z);
            }
            else
            {
                manaBar.localScale = -new Vector3(TotalEnergyBarSize / manaBars, -0.5f, -1);
            }
        }
    }

    public void SetSR(Sprite sprite)
    {
        sr.sprite = sprite;
    }

    public void SetMana(float mana)
    {
        try
        {
            if (mana > 1)
            {
                mana = 1;
            }

            if (mana < CurrentMana)
            {
                CurrentMana = mana;

                for (int i = 0; i < ManaBars; i++)
                {
                    float width = mana / ManaPerBar;

                    if (width >= 1)
                    {
                        width = 1;
                    }
                    StartCoroutine(ChangeWidthCoroutine(ManaBarsList[i], width));
                    mana -= ManaPerBar;
                }
            }
            else
            {
                CurrentMana = mana;

                for (int i = 0; i < ManaBars; i++)
                {
                    float width = mana / ManaPerBar;

                    if (width >= 1)
                    {
                        width = 1;
                    }
                    ManaBarsList[i].Width = width;
                    mana -= ManaPerBar;
                }
            }
        }

        catch (Exception ex)
        {
            Debug.LogWarning(ex.Message);
        }
    }

    IEnumerator ChangeWidthCoroutine(Rectangle rectangle, float targetWidth)
    {
        float initialWidth = rectangle.Width; // Store the initial width
        float duration = 0.2f; // Total time to change the width
        float elapsed = 0; // Time elapsed since the start of the animation

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime; // Update the elapsed time
                                       // Interpolate the width based on the elapsed time
            rectangle.Width = Mathf.Lerp(initialWidth, targetWidth, elapsed / duration);
            yield return null; // Wait for the next frame
        }

        // Ensure the width is set to the target width when done, in case of any discrepancies
        rectangle.Width = targetWidth;
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
        shieldValue = shield;

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