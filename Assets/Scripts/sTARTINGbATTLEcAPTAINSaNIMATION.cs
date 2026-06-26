using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class sTARTINGbATTLEcAPTAINSaNIMATION : MonoBehaviour
{
    public RectTransform myCaptain;
    public RectTransform enemyCaptain;
    public VanillaMonsterGraphic myCaptainGraphics;
    public VanillaMonsterGraphic enemyCaptainGraphics;

    private void OnEnable()
    {
        EnsureGraphics();
        myCaptain.DOAnchorPosX(-274f, .65f);
        enemyCaptain.DOAnchorPosX(274f, .65f);
        myCaptainGraphics.startingAnimation = "activity/appear";
        enemyCaptainGraphics.startingAnimation = "activity/appear";
        enemyCaptainGraphics.Initialize(true);
        myCaptainGraphics.Initialize(true);
        Invoke(nameof(EnableAnimations), .65f);
    }

    private void OnDisable()
    {
        TeamCaptainManager.Instance.DisableCaptainBehavior();
        myCaptain.DOAnchorPosX(-455f, 0f);
        enemyCaptain.DOAnchorPosX(455f, 0f);
    }

    public void EnableAnimations()
    {
        EnsureGraphics();
        myCaptainGraphics.startingAnimation = "action/idle/normal";
        enemyCaptainGraphics.startingAnimation = "action/idle/normal";
        enemyCaptainGraphics.Initialize(true);
        myCaptainGraphics.Initialize(true);
        TeamCaptainManager.Instance.EnableCaptainBehavior();
    }

    private void EnsureGraphics()
    {
        if (myCaptainGraphics == null && myCaptain != null)
            myCaptainGraphics = EnsureGraphic(myCaptain, "My Captain Graphic");

        if (enemyCaptainGraphics == null && enemyCaptain != null)
            enemyCaptainGraphics = EnsureGraphic(enemyCaptain, "Enemy Captain Graphic");
    }

    private VanillaMonsterGraphic EnsureGraphic(RectTransform parent, string name)
    {
        VanillaMonsterGraphic current = name == "My Captain Graphic" ? myCaptainGraphics : enemyCaptainGraphics;
        return VanillaMonsterGraphic.EnsureCenteredChild(parent, current, name);
    }
}
