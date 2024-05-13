using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Spine;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;

public class FakeAxieController : MonoBehaviour
{
    internal GetAxiesExample.Axie axie;
    public SkeletonAnimation skeletonAnim;
    internal FakeOverlayTile standingOnTile;
    internal Renderer renderer;
    internal bool grabbed;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
    }

    private void OnMouseOver()
    {
        if (grabbed || renderer.enabled == false || skeletonAnim.AnimationName == "action/idle/random-01")
            return;
        standingOnTile.beingHovered = true;
        skeletonAnim.AnimationName = "action/idle/random-01";
        skeletonAnim.loop = true;
    }

    public void Grab(bool grabbed)
    {
        if (this.grabbed && grabbed)
            return;
        this.grabbed = grabbed;
        if (grabbed == false)
        {
            skeletonAnim.AnimationName = "action/idle/normal";
            skeletonAnim.loop = true;
        }
        else
        {
            skeletonAnim.AnimationName = "action/idle/random-03";
            skeletonAnim.loop = true;
        }
    }

    private void OnMouseExit()
    {
        if (grabbed || renderer.enabled == false)
            return;
        standingOnTile.beingHovered = false;
        skeletonAnim.AnimationName = "action/idle/normal";
        skeletonAnim.loop = true;
    }
}