using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;

public class IngameStats
{
    public float Range = 1f;
    public string axieId;
    public float AttackSpeed;
    public float MinEnergy;
    public float MaxEnergy;
    public float CurrentEnergy;
    public float HP;
    public float currentHP;
    public float currentShield;
    public AxieClass axieClass;
}

public class AxieController : MonoBehaviour
{
    internal int AxieId;
    internal IngameStats axieIngameStats;
    internal AxieSkillEffectManager axieSkillEffectManager;
    private Team.CharacterState state;
    internal int startingCol;
    internal int startingRow;
    private AxieController[] allCharacters;
    public OverlayTile standingOnTile;
    public AxieController CurrentTarget;
    public AxieBehavior axieBehavior;
    public SkeletonAnimation SkeletonAnim;
    public StatsManager statsManagerUI;
    public Team goodTeam;
    public Team badTeam;
    public AxieSkillController axieSkillController;
    public GetAxiesExample.Stats stats;
    public int Range = 1;
    public List<SkillName> axieBodyParts = new List<SkillName>();
    internal bool imGood;

    public List<AxieController> GetAdjacent()
    {
        return MapManager.Instance.GetAdjacentTiles(this.standingOnTile)
            .Where(x => x.currentOccupier.imGood == this.imGood).Select(x => x.currentOccupier).ToList();
    }

    public Vector3 GetPartPosition(BodyPart part)
    {
        return transform.position;
    }

    public void AddStatusEffect(SkillEffect skillEffect)
    {
        axieSkillEffectManager.AddStatusEffect(skillEffect);
    }

    public void RemoveStatusEffect(SkillEffect skillEffect)
    {
        axieSkillEffectManager.RemoveAllEffects();
    }

    public List<SkillEffect> GetAllSkillEffectsNotPassives()
    {
        return axieSkillEffectManager.GetAllSkillEffectsNotPassives().ToList();
    }

    public void RemoveAllEffects()
    {
        axieSkillEffectManager.RemoveAllEffects();
    }

    private IEnumerator Start()
    {
        yield return new WaitForFixedUpdate();
        goodTeam = FindObjectsOfType<Team>().Single(x => x.isGoodTeam);
        badTeam = FindObjectsOfType<Team>().Single(x => !x.isGoodTeam);


        if (this.standingOnTile.grid2DLocation.x >= 4)
        {
            state = badTeam.GetCharacterState(axieIngameStats.axieId);
        }
        else
        {
            imGood = true;
            state = goodTeam.GetCharacterState(axieIngameStats.axieId);
        }
        
        

        if (axieIngameStats.axieClass == AxieClass.Bird)
        {
            axieIngameStats.Range = 4;
            Range = 4;
        }

        axieIngameStats.HP = AxieStatCalculator.GetHP(stats);

        axieIngameStats.currentHP = axieIngameStats.HP;
        if (imGood)
        {
            SkeletonAnim.GetComponent<Renderer>().sortingOrder =
                (int)MathHelpers.InvLerp(0, 7, (standingOnTile.grid2DLocation.y + 1));
        }
        else
        {
            SkeletonAnim.GetComponent<Renderer>().sortingOrder =
                (int)MathHelpers.InvLerp(0, 7, (standingOnTile.grid2DLocation.y));
        }

        axieBehavior.myController = this;
        SkeletonAnim.loop = true;
        axieBehavior.DoAction(AxieState.Idle);
    }


    private void OnMouseOver()
    {
        if (!goodTeam.battleStarted)
            axieBehavior.DoAction(AxieState.Hovered);
    }

    private void OnMouseExit()
    {
        if (!goodTeam.battleStarted)
            axieBehavior.DoAction(AxieState.Idle);
    }

    private void FixedUpdate()
    {
        if (goodTeam == null)
            return;
        if (CurrentTarget != null && CurrentTarget.axieIngameStats.currentHP <= 0)
        {
            CurrentTarget = null;
        }

        if (axieBehavior.axieState == AxieState.Shrimping || !goodTeam.battleStarted)
            return;

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (axieIngameStats.axieClass == AxieClass.Aquatic)
            {
                //axieBehavior.DoAction(AxieState.Shrimping);
                return;
            }
        }

        if (imGood && badTeam.GetCharacters().Count == 0 ||
            !imGood && goodTeam.GetCharacters().Count == 0)
        {
            axieBehavior.DoAction(AxieState.Victory);
            return;
        }

        if (axieIngameStats.currentHP > axieIngameStats.HP)
        {
            axieIngameStats.currentHP = axieIngameStats.HP;
        }

        if (axieIngameStats.currentHP <= 0)
        {
            axieBehavior.DoAction(AxieState.Killed);
            return;
        }
        else
        {
            statsManagerUI.SetMana(axieIngameStats.CurrentEnergy / axieSkillController.GetComboCost());
            statsManagerUI.SetHP(axieIngameStats.currentHP / axieIngameStats.HP);
        }

        if (axieBehavior.axieState == AxieState.Killed)
            this.gameObject.SetActive(false);

        if (axieBehavior.axieState == AxieState.Casting)
            return;

        if (state == null)
            return;

        if (axieBehavior.axieState == AxieState.Grabbed)
        {
            CurrentTarget = null;
            axieBehavior.DoAction(AxieState.Grabbed);
            SkeletonAnim.loop = true;
            return;
        }
        else if (axieBehavior.axieState == AxieState.Hovered && CurrentTarget == null)
        {
            axieBehavior.DoAction(AxieState.Hovered);
            return;
        }
        else if (state.isMoving == false && CurrentTarget == null)
        {
            axieBehavior.DoAction(AxieState.Idle);
            return;
        }

        axieIngameStats.CurrentEnergy += 0.002f + (stats.skill / 10000f);

        if (axieIngameStats.CurrentEnergy >= axieSkillController.GetComboCost())
        {
            axieBehavior.DoAction(AxieState.Casting);
            return;
        }

        if (CurrentTarget != null && state.isMoving == false &&
            GetManhattanDistance(this.standingOnTile, CurrentTarget.standingOnTile) <= axieIngameStats.Range)
        {
            axieBehavior.DoAction(AxieState.Attacking);

            if (CurrentTarget.standingOnTile.grid2DLocation.y != standingOnTile.grid2DLocation.y)
            {
                if (imGood)
                {
                    transform.localScale = new Vector3(-0.2f, transform.localScale.y, transform.localScale.z);
                }
                else
                {
                    transform.localScale = new Vector3(0.2f, transform.localScale.y, transform.localScale.z);
                }
            }
            else
            {
                transform.localScale =
                    new Vector3(CurrentTarget.transform.position.x > this.transform.position.x ? -0.2f : 0.2f,
                        transform.localScale.y, transform.localScale.z);
            }

            SkeletonAnim.loop = true;
        }
        else if (state.isMoving)
        {
            SkeletonAnim.AnimationName = "action/run";
            SkeletonAnim.loop = true;
            if (CurrentTarget == null)
                return;
            transform.localScale =
                new Vector3(CurrentTarget.transform.position.x > this.transform.position.x ? -0.2f : 0.2f,
                    transform.localScale.y, transform.localScale.z);
        }
        else
        {
            SkeletonAnim.AnimationName = "action/idle/normal";
            SkeletonAnim.loop = true;
        }
    }

    private int GetManhattanDistance(OverlayTile tile1, OverlayTile tile2)
    {
        return Mathf.Abs(tile1.gridLocation.x - tile2.gridLocation.x) +
               Mathf.Abs(tile1.gridLocation.z - tile2.gridLocation.z);
    }
}