using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using Spine;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class FakeAxieController : MonoBehaviour
{
    internal GetAxiesExample.Axie axie;
    public SkeletonAnimation skeletonAnim;
    internal FakeOverlayTile standingOnTile;
    public TeamBuilderManager teamBuilderManager;
    internal Renderer renderer;
    internal bool grabbed;
    internal Combos combos = new Combos();
    public TextMeshProUGUI dragAxiesEnabled;
    public TextMeshProUGUI dragAxiesDisabled;

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

    private void Update()
    {
        if (grabbed)
        {
            if (this.transform.position.x >= 5 && !dragAxiesEnabled.enabled)
            {
                dragAxiesEnabled.enabled = true;
                dragAxiesDisabled.enabled = false;
            }
            else if (this.transform.position.x <= 5 && dragAxiesEnabled.enabled)
            {
                dragAxiesEnabled.enabled = false;
                dragAxiesDisabled.enabled = true;
            }
        }

    }

    public void Grab(bool grabbed)
    {
        if (this.grabbed && grabbed || axie == null)
            return;
        this.grabbed = grabbed;
        if (grabbed == false)
        {
            if (this.transform.position.x >= 5)
            {
                var axieInList = teamBuilderManager.axieList.FirstOrDefault(x => x.axie != null && x.axie.id == axie.id);
                if (axieInList != null)
                {
                    axieInList.SelectAxie();
                }
                else
                {
                    this.axie = null;
                    renderer.enabled = false;
                }
            }
            skeletonAnim.AnimationName = "action/idle/normal";
            skeletonAnim.loop = true;
            teamBuilderManager.AxieTeamUIObject.SetActive(true);
            teamBuilderManager.DragToRemoveObject.SetActive(false);
        }
        else
        {
            teamBuilderManager.AxieTeamUIObject.SetActive(false);
            teamBuilderManager.DragToRemoveObject.SetActive(true);
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