using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;

public class SpawnedAxie
{
    public float Range = 1f;
    public string axieId;
    public float AttackSpeed;
    public float MinMana;
    public float MaxMana;
    public float CurrentMana;
    public float HP;
    public float currentHP;
    public BodyPart bodyPartMain;
    public SkillName skillName;
    public AxieClass axieClass;
}


public class AxieController : MonoBehaviour
{
    public OverlayTile standingOnTile;
    public AxieController CurrentTarget;
    public AxieBehavior axieBehavior;
    public SkeletonAnimation SkeletonAnim;
    public StatsManager statsManagerUI;
    internal SpawnedAxie spawnedAxie;
    public Team goodTeam;
    public Team badTeam;
    private Team.CharacterState state;
    private AxieController[] allCharacters;
    internal bool imGood;
    public GetAxiesExample.Stats stats;

    private void Start()
    {
        goodTeam = FindObjectsOfType<Team>().Single(x => x.isGoodTeam);
        badTeam = FindObjectsOfType<Team>().Single(x => !x.isGoodTeam);
        if (this.standingOnTile.grid2DLocation.x >= 4)
        {
            state = badTeam.GetCharacterState(spawnedAxie.axieId);
        }
        else
        {
            imGood = true;
            state = goodTeam.GetCharacterState(spawnedAxie.axieId);
        }

        if (spawnedAxie.axieClass == AxieClass.Bird)
            spawnedAxie.Range = 4;

        spawnedAxie.HP = AxieStatCalculator.GetHP(stats);

        spawnedAxie.currentHP = spawnedAxie.HP;
        SetAllCharacters();
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


    public void SetAllCharacters()
    {
        allCharacters = FindObjectsOfType<AxieController>();
    }

    private void OnMouseOver()
    {
        axieBehavior.DoAction(AxieState.Hovered);
    }

    private void OnMouseExit()
    {
        axieBehavior.DoAction(AxieState.Idle);
    }

    private void Update()
    {
        if (axieBehavior.axieState == AxieState.Shrimping || !goodTeam.battleStarted)
            return;

        if (Input.GetKeyDown(KeyCode.H))
        {
            if (spawnedAxie.axieClass == AxieClass.Aquatic)
            {
                axieBehavior.DoAction(AxieState.Shrimping);
                return;
            }
        }

        if (imGood && badTeam.GetCharacters().Count == 0 ||
            !imGood && goodTeam.GetCharacters().Count == 0)
        {
            axieBehavior.DoAction(AxieState.Victory);
            return;
        }

        if (spawnedAxie.currentHP > spawnedAxie.HP)
        {
            spawnedAxie.currentHP = spawnedAxie.HP;
        }

        if (spawnedAxie.currentHP <= 0)
        {
            axieBehavior.DoAction(AxieState.Killed);
            return;
        }
        else
        {
            statsManagerUI.SetMana(spawnedAxie.CurrentMana / spawnedAxie.MaxMana);
            statsManagerUI.SetHP(spawnedAxie.currentHP / spawnedAxie.HP);
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

        spawnedAxie.CurrentMana += Time.deltaTime * 3;

        if (spawnedAxie.CurrentMana >= spawnedAxie.MaxMana)
        {
            axieBehavior.DoAction(AxieState.Casting);
            spawnedAxie.CurrentMana = 0;
            return;
        }

        if (CurrentTarget != null && state.isMoving == false &&
            GetManhattanDistancePlayer(this.transform.position, CurrentTarget.transform.position) <= spawnedAxie.Range)
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

    private int GetManhattanDistancePlayer(Vector3 tile1, Vector3 tile2)
    {
        return Mathf.RoundToInt(Mathf.Abs(tile1.x - tile2.x) +
                                Mathf.Abs(tile1.z - tile2.z));
    }
}