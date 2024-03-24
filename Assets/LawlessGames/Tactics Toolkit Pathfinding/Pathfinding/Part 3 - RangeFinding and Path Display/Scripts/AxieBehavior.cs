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
    public List<SkillName> SkillList;

    private void Start()
    {
        AttackSpeed = AxieStatCalculator.GetAttackSpeed(myController.stats);
        if (myController.axieIngameStats.axieClass == AxieClass.Bird)
        {
            AttackAnimation = "attack/ranged/cast-multi";
        }
        else if (myController.axieIngameStats.axieClass == AxieClass.Aquatic)
        {
            AttackAnimation = "attack/melee/horn-gore";
        }
        else if (myController.axieIngameStats.axieClass == AxieClass.Beast)
        {
            AttackAnimation = "attack/melee/tail-roll";
        }
        else if (myController.axieIngameStats.axieClass == AxieClass.Plant)
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

        yield return new WaitForSecondsRealtime(0.1f);
        myController.SkeletonAnim.GetComponent<Renderer>().enabled = true;
        myController.SkeletonAnim.enabled = true;
        yield return new WaitForSecondsRealtime(
            myController.SkeletonAnim.AnimationState.GetCurrent(0).AnimationEnd / 2 - 0.1f);
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

        yield return new WaitForSecondsRealtime(0.1f);
        myController.SkeletonAnim.GetComponent<Renderer>().enabled = true;
        yield return new WaitForSecondsRealtime(
            myController.SkeletonAnim.AnimationState.GetCurrent(0).AnimationEnd / 2 - 0.1f);

        this.transform.localScale = new Vector3(this.transform.localScale.x * -1, this.transform.localScale.y,
            this.transform.localScale.z);
    }

    public IEnumerator TryAttack()
    {
        if (myController.CurrentTarget == null)
            yield break;
        while (axieState == AxieState.Attacking)
        {
            myController.SkeletonAnim.AnimationName = AttackAnimation;
            myController.SkeletonAnim.timeScale =
                myController.SkeletonAnim.AnimationState.GetCurrent(0).AnimationEnd / AttackSpeed;

            if (myController.axieIngameStats.axieClass == AxieClass.Bird)
                AutoAttackMaNAGER.instance.SpawnProjectileBird(myController.transform,
                    myController.CurrentTarget.transform);
            yield return new WaitForSecondsRealtime(AttackSpeed);
            if (myController.CurrentTarget == null)
                yield break;
            myController.CurrentTarget.axieIngameStats.currentHP -= AxieStatCalculator.GetAttackDamage(myController.stats);
            myController.axieIngameStats.CurrentEnergy += AxieStatCalculator.GetManaPerAttack(myController.stats);
        }
    }

    public void CastSpell()
    {
        myController.SkeletonAnim.loop = false;
        StartCoroutine(ICastSpell());
    }

    IEnumerator ICastSpell()
    {
        if (myController.CurrentTarget == null)
        {
            myController.axieIngameStats.CurrentEnergy -= myController.axieIngameStats.CurrentEnergy * 0.05f;
            myController.SkeletonAnim.loop = true;
            DoAction(AxieState.Idle);
            yield break;
        }

        yield return StartCoroutine(SkillLauncher.Instance.ThrowSkill(myController.axieSkillController.GetAxieSkills(),
            myController.SkeletonAnim,
            this.myController, myController.CurrentTarget));

        CastSpellAftermath();
    }

    private void CastSpellAftermath()
    {
        myController.axieIngameStats.CurrentEnergy = myController.axieIngameStats.MinEnergy;
        myController.SkeletonAnim.loop = true;
        DoAction(AxieState.Idle);
    }
}