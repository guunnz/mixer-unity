using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class FakeMonsterController : MonoBehaviour
{
    internal GetMonstersExample.Monster monster;
    public VanillaMonsterVisual visual;
    internal FakeOverlayTile standingOnTile;
    public TeamBuilderManager teamBuilderManager;
    internal Renderer renderer;
    internal bool grabbed;
    internal bool visible;
    internal Combos combos = new Combos();
    public TextMeshProUGUI dragMonstersEnabled;
    public TextMeshProUGUI dragMonstersDisabled;

    private static readonly Vector3 NormalizedRootScale = Vector3.one;
    private BoxCollider grabCollider;

    private void Awake()
    {
        EnsureVisual();
        SetVisible(false);
    }

    private void OnMouseOver()
    {
        if (grabbed || !visible || visual.CurrentState == MonsterVisualState.Hover)
            return;
        standingOnTile.beingHovered = true;
        visual.Play(MonsterVisualState.Hover, true);
    }

    private void Update()
    {
        if (grabbed)
        {
            if (this.transform.position.x >= 5 && !dragMonstersEnabled.enabled)
            {
                dragMonstersEnabled.enabled = true;
                dragMonstersDisabled.enabled = false;
            }
            else if (this.transform.position.x <= 5 && dragMonstersEnabled.enabled)
            {
                dragMonstersEnabled.enabled = false;
                dragMonstersDisabled.enabled = true;
            }
        }

    }

    public void Grab(bool grabbed)
    {
        if (this.grabbed && grabbed || monster == null)
            return;
        this.grabbed = grabbed;
        if (grabbed == false)
        {
            if (this.transform.position.x >= 5)
            {
                var monsterInList = teamBuilderManager.monsterList.FirstOrDefault(x => x.monster != null && x.monster.id == monster.id);
                if (monsterInList != null)
                {
                    monsterInList.SelectMonster();
                }
                else
                {
                    this.monster = null;
                    SetVisible(false);
                }
            }
            visual.Play(MonsterVisualState.Idle, true);
            teamBuilderManager.MonsterTeamUIObject.SetActive(true);
            teamBuilderManager.DragToRemoveObject.SetActive(false);
        }
        else
        {
            teamBuilderManager.MonsterTeamUIObject.SetActive(false);
            teamBuilderManager.DragToRemoveObject.SetActive(true);
            visual.Play(MonsterVisualState.Grabbed, true);
        }
    }

    private void OnMouseExit()
    {
        if (grabbed || !visible)
            return;
        standingOnTile.beingHovered = false;
        visual.Play(MonsterVisualState.Idle, true);
    }

    public void SetDescriptor(MonsterVisualDescriptor descriptor)
    {
        EnsureVisual();
        NormalizeRootScale();
        visual.SetDescriptor(descriptor);
        visual.Play(MonsterVisualState.Idle, true);
    }

    public void SetVisible(bool isVisible)
    {
        EnsureVisual();
        NormalizeRootScale();
        visible = isVisible;
        visual.SetVisible(isVisible);
        renderer = visual.MainRenderer;
        if (grabCollider != null)
            grabCollider.enabled = isVisible;
    }

    private void EnsureVisual()
    {
        NormalizeRootScale();
        grabCollider = MonsterScale.ApplyGrabCollider(gameObject);
        if (grabCollider != null)
            grabCollider.enabled = visible;

        if (visual == null)
            visual = VanillaMonsterVisual.Ensure(gameObject);
        renderer = visual.MainRenderer;
    }

    private void NormalizeRootScale()
    {
        transform.localScale = NormalizedRootScale;
    }
}
