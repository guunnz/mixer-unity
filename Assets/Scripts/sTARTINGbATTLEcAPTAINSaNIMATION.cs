using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class sTARTINGbATTLEcAPTAINSaNIMATION : MonoBehaviour
{
    public RectTransform myCaptain;
    public RectTransform enemyCaptain;
    public SkeletonGraphic myCaptainGraphics;
    public SkeletonGraphic enemyCaptainGraphics;

    private void OnEnable()
    {
        myCaptain.DOAnchorPosX(-274f, .65f);
        enemyCaptain.DOAnchorPosX(274f, .65f);
        myCaptainGraphics.startingAnimation = "activity/appear";
        enemyCaptainGraphics.startingAnimation = "activity/appear";
        myCaptainGraphics.startingLoop = false;
        enemyCaptainGraphics.startingLoop = false;
        enemyCaptainGraphics.Initialize(true);
        myCaptainGraphics.Initialize(true);
        Invoke(nameof(EnableAnimations), .65f);
    }

    private void OnDisable()
    {
        TeamCaptainManager.Instance.DisableCaptainBehavior();
    }

    public void EnableAnimations()
    {
        myCaptainGraphics.startingAnimation = "action/idle/normal";
        enemyCaptainGraphics.startingAnimation = "action/idle/normal";
        myCaptainGraphics.startingLoop = true;
        enemyCaptainGraphics.startingLoop = true;
        enemyCaptainGraphics.Initialize(true);
        myCaptainGraphics.Initialize(true);
        TeamCaptainManager.Instance.EnableCaptainBehavior();
    }
}
