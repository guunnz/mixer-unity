using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum AxieState
{
    None,
    Moving,
    Attacking,
    Stunned,
    Shrimping,
    Casting,
    Victory,
    Grabbed,
    Idle,
    Hovered,
    Killed
}

public class AxieBehavior : MonoBehaviour
{
    public AxieState axieState;
    internal AxieController myController;
    private float AttackSpeed;
    private string AttackAnimation;

    private void Start()
    {
        AttackSpeed = AxieStatCalculator.GetAttackSpeed(myController.stats);
        if (myController.spawnedAxie.axieClass == AxieClass.Bird)
        {
            AttackAnimation = "attack/ranged/cast-multi";
        }
        else if (myController.spawnedAxie.axieClass == AxieClass.Aquatic)
        {
            AttackAnimation = "attack/melee/horn-gore";
        }
        else if (myController.spawnedAxie.axieClass == AxieClass.Beast)
        {
            AttackAnimation = "attack/melee/tail-roll";
        }
        else if (myController.spawnedAxie.axieClass == AxieClass.Plant)
        {
            AttackAnimation = "attack/melee/mouth-bite";
        }
        else
        {
            AttackAnimation = "attack/melee/normal-attack";
        }
    }


    public void DoShrimp()
    {
        StartCoroutine(GoBackdoor());
    }

    public void DoAction(AxieState state)
    {
        if (state == axieState)
            return;
        axieState = state;
        switch (state)
        {
            case AxieState.Attacking:
                StartCoroutine(TryAttack());
                break;
            case AxieState.Casting:
                CastSpell();
                break;
            case AxieState.Moving:
                myController.SkeletonAnim.AnimationName = "action/run";
                myController.SkeletonAnim.loop = true;
                break;
            case AxieState.Shrimping:
                myController.SkeletonAnim.AnimationName = "attack/melee/shrimp";
                DoShrimp();
                break;
            case AxieState.Stunned:
                myController.SkeletonAnim.AnimationName = "battle/get-debuff";
                break;
            case AxieState.Victory:
                myController.SkeletonAnim.timeScale = 1;
                myController.SkeletonAnim.AnimationName = "activity/victory-pose-back-flip";
                myController.SkeletonAnim.loop = true;
                break;
            case AxieState.Grabbed:
                myController.SkeletonAnim.AnimationName = "action/idle/random-03";
                myController.SkeletonAnim.loop = true;
                break;
            case AxieState.Hovered:
                myController.SkeletonAnim.AnimationName = "action/idle/random-01";
                myController.SkeletonAnim.loop = true;
                break;
            case AxieState.Idle:
                myController.SkeletonAnim.AnimationName = "action/idle/normal";
                myController.SkeletonAnim.loop = true;
                break;
            case AxieState.Killed:
                myController.gameObject.SetActive(false);
                break;
        }
    }

    public IEnumerator GoBackdoor()
    {
        myController.SkeletonAnim.loop = false;
        yield return new WaitForSeconds(myController.SkeletonAnim.AnimationState.GetCurrent(0).AnimationEnd / 2);
        myController.SkeletonAnim.enabled = false;
        Vector3 positionToMove = Vector3.zero;

        List<OverlayTile> possibleTiles = myController.imGood
            ? myController.badTeam.GetInRangeTiles(this.myController)
            : myController.goodTeam.GetInRangeTiles(this.myController);
        List<OverlayTile> jumpToTiles =
            possibleTiles.Where(x => x.grid2DLocation.x == (myController.imGood ? 7 : 0)).ToList();

        OverlayTile overlayTile =
            jumpToTiles.FirstOrDefault(x => x.grid2DLocation.y == this.myController.standingOnTile.grid2DLocation.y);
        if (jumpToTiles.Count == 0 || overlayTile == null)
        {
            jumpToTiles = possibleTiles.Where(x => x.grid2DLocation.x == (this.myController.imGood ? 6 : 1)).ToList();
        }

        overlayTile =
            jumpToTiles.FirstOrDefault(x => x.grid2DLocation.y == this.myController.standingOnTile.grid2DLocation.y);


        if (overlayTile != null)
        {
            positionToMove = overlayTile.transform.position;
        }

        this.myController.standingOnTile = overlayTile;
        this.transform.position = positionToMove;

        yield return new WaitForSeconds(0.1f);
        myController.SkeletonAnim.GetComponent<Renderer>().enabled = true;
        myController.SkeletonAnim.enabled = true;
        yield return new WaitForSeconds(myController.SkeletonAnim.AnimationState.GetCurrent(0).AnimationEnd / 2 - 0.1f);
        myController.SkeletonAnim.loop = true;
        this.transform.localScale = new Vector3(this.transform.localScale.x * -1, this.transform.localScale.y,
            this.transform.localScale.z);
        DoAction(AxieState.Idle);
    }

    public IEnumerator GoBackdoorTile(OverlayTile tile)
    {
        yield return new WaitForSeconds(myController.SkeletonAnim.AnimationState.GetCurrent(0).AnimationEnd / 2);
        myController.SkeletonAnim.enabled = false;
        Vector3 positionToMove = Vector3.zero;

        OverlayTile overlayTile = tile;


        positionToMove = overlayTile.transform.position;


        this.myController.standingOnTile = overlayTile;
        this.transform.position = positionToMove;

        yield return new WaitForSeconds(0.1f);
        myController.SkeletonAnim.GetComponent<Renderer>().enabled = true;
        yield return new WaitForSeconds(myController.SkeletonAnim.AnimationState.GetCurrent(0).AnimationEnd / 2 - 0.1f);

        this.transform.localScale = new Vector3(this.transform.localScale.x * -1, this.transform.localScale.y,
            this.transform.localScale.z);
    }

    public IEnumerator TryAttack()
    {
        while (axieState == AxieState.Attacking)
        {
            myController.SkeletonAnim.AnimationName = AttackAnimation;
            myController.SkeletonAnim.timeScale =
                myController.SkeletonAnim.AnimationState.GetCurrent(0).AnimationEnd / AttackSpeed;

            if (myController.spawnedAxie.axieClass == AxieClass.Bird)
                AutoAttackMaNAGER.instance.SpawnProjectileBird(myController.transform,
                    myController.CurrentTarget.transform);
            yield return new WaitForSeconds(AttackSpeed);
            myController.CurrentTarget.spawnedAxie.currentHP -= AxieStatCalculator.GetAttackDamage(myController.stats);
            myController.spawnedAxie.CurrentMana += AxieStatCalculator.GetManaPerAttack(myController.stats);
            yield return null;
        }
    }

    public void CastSpell()
    {
        myController.SkeletonAnim.loop = false;
        StartCoroutine(ICastSpell());
    }

    IEnumerator ICastSpell()
    {
        float timeToWait = SkillLauncher.Instance.ThrowSkill(myController.spawnedAxie.skillName,
            myController.spawnedAxie.axieClass,
            myController.spawnedAxie.bodyPartMain,
            myController.CurrentTarget.transform, this.transform, myController.SkeletonAnim,
            myController.spawnedAxie.skillName == SkillName.Rosebud ? this.myController : myController.CurrentTarget);
        yield return new WaitForSeconds(timeToWait);
        myController.spawnedAxie.CurrentMana = myController.spawnedAxie.MinMana;
        myController.SkeletonAnim.loop = true;
        DoAction(AxieState.Idle);
    }
}