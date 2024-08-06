using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


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
    internal AxieSkillEffectManager axieSkillEffectManager;
    private float AttackSpeed;
    private string AttackAnimation;
    public List<SkillName> SkillList;
    private Coroutine attackCoroutine;

    private IEnumerator Start()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        AttackSpeed = AxieStatCalculator.GetAttackSpeed(myController.stats);
        if (myController.axieIngameStats.axieClass == AxieClass.Bird ||
            myController.axieIngameStats.axieClass == AxieClass.Dusk ||
            myController.axieIngameStats.axieClass == AxieClass.Bug)
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

    private void Update()
    {
        if (this.axieState == AxieState.Stunned)
        {
            if (!axieSkillEffectManager.IsStunned())
            {
                DoAction(AxieState.Idle);
            }
        }
    }

    private void OnAction()
    {
        if (axieSkillEffectManager.IsPoisoned())
        {
            myController.axieIngameStats.currentHP -=
                AxieStatCalculator.GetPoisonDamage(axieSkillEffectManager.PoisonStacks());
        }
    }

    public void DoShrimp()
    {
        StartCoroutine(GoBackdoor());
    }

    public void DoAction(AxieState state)
    {
        if (state == axieState && state != AxieState.Idle)
            return;

        if (axieSkillEffectManager.IsStunned() && state != AxieState.Killed && state != AxieState.Victory)
        {
            state = AxieState.Stunned;
        }

        OnAction();
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        axieState = state;
        switch (state)
        {
            case AxieState.Attacking:
                attackCoroutine = StartCoroutine(TryAttack());
                break;
            case AxieState.Casting:
                if (myController.CurrentTarget != null)
                {
                    var characters = myController.CurrentTarget.myTeam.GetCharacters();
                    if (myController.CurrentTarget.axieSkillEffectManager.IsStenched() && characters.Count != 1 && characters.Any(x => !x.axieSkillEffectManager.IsStenched()))
                    {
                        state = AxieState.Idle;
                        axieState = state;
                        myController.CurrentTarget = null;
                        return;
                    }
                }

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

        List<OverlayTile> possibleTiles = myController.enemyTeam.GetInRangeTiles(this.myController);
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
            if (myController.CurrentTarget != null)
            {
                var characters = myController.CurrentTarget.myTeam.GetCharacters();
                if (myController.CurrentTarget.axieSkillEffectManager.IsStenched() && characters.Count != 1 && characters.Any(x => !x.axieSkillEffectManager.IsStenched()))
                {
                    axieState = AxieState.Idle;
                    myController.CurrentTarget = null;
                    yield break;
                }
            }

            if (!myController.axieSkillController.OnAutoAttack())
            {
                myController.SkeletonAnim.AnimationName = AttackAnimation;
                myController.SkeletonAnim.timeScale =
                    myController.SkeletonAnim.AnimationState.GetCurrent(0).AnimationEnd / AttackSpeed;
            }

  

            yield return new WaitForSecondsRealtime(AttackSpeed/2f);

            if (myController.Range > 1)
                AutoAttackMaNAGER.instance.SpawnProjectile(myController.transform,
                    myController.CurrentTarget.transform, myController.axieIngameStats.axieClass);
            else
            {
                AutoAttackMaNAGER.instance.SpawnAttack(myController.CurrentTarget.transform, myController.axieIngameStats.axieClass);
            }
            yield return new WaitForSecondsRealtime(AttackSpeed / 2f);
            if (myController.CurrentTarget != null)
            {
                var characters = myController.CurrentTarget.myTeam.GetCharacters();
                if (myController.CurrentTarget.axieSkillEffectManager.IsStenched() && characters.Count != 1 && characters.Any(x => !x.axieSkillEffectManager.IsStenched()))
                {
                    axieState = AxieState.Idle;
                    myController.CurrentTarget = null;
                    yield break;
                }
            }
            else
            {
                yield break;
            }

            if (axieSkillEffectManager.IsFeared())
            {
                //Debug.Log("Missed!");
            }
            else
            {
                var target = myController.CurrentTarget;
                float attackDamage = AxieStatCalculator.GetAttackDamage(myController.stats);

                attackDamage += this.myController.axieSkillController.passives.AutoattackIncrease;


                float damageReduction = target.axieSkillController.passives.DamageReductionAmount;

                if (target.axieSkillEffectManager.IsAromad())
                {
                    damageReduction -= 50;
                }

                attackDamage -= (attackDamage * (damageReduction / 100f));

                if (!myController.axieSkillEffectManager.IsJinxed() &&
                    !target.axieSkillController.passives.ImmuneToCriticals)
                {
                    bool isLethal = target.axieSkillEffectManager.IsLethal();
                    if (isLethal || Random.Range(0, 1f) <= AxieStatCalculator.GetCritChance(myController.stats))
                    {
                        attackDamage *= AxieStatCalculator.GetCritDamage(myController.stats);

                        if (isLethal)
                        {
                            target.axieSkillEffectManager.RemoveStatusEffect(StatusEffectEnum.Lethal);
                        }
                    }
                }

                myController.axieIngameStats.currentHP +=
                    attackDamage * myController.axieSkillController.passives.HealOnDamageDealt;

                if (myController.axieSkillController.IgnoresShieldOnAttack())
                {
                    target.axieIngameStats.currentHP -= attackDamage;
                    target.axieSkillController.DamageReceived(myController.axieIngameStats.axieClass, attackDamage,
                        myController);
                }
                else
                {
                    float shieldDamage = attackDamage - target.axieIngameStats.currentShield;

                    if (shieldDamage < 0)
                    {
                        target.axieIngameStats.currentShield -= attackDamage;
                    }
                    else
                    {
                        target.axieIngameStats.currentShield = 0;
                        target.axieIngameStats.currentHP -= shieldDamage;
                    }

                    target.axieSkillController.DamageReceived(myController.axieIngameStats.axieClass, shieldDamage,
                        myController);
                }
            }


            // myController.axieIngameStats.CurrentEnergy += AxieStatCalculator.GetManaPerAttack(myController.stats);
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

        while (axieSkillEffectManager.IsStunned())
        {
            yield return new WaitForFixedUpdate();
        }

        yield return StartCoroutine(SkillLauncher.Instance.ThrowSkill(myController.axieSkillController.GetAxieSkills(),
            myController.SkeletonAnim,
            myController.CurrentTarget, this.myController));

        CastSpellAftermath();
    }

    private void CastSpellAftermath()
    {
        myController.axieIngameStats.CurrentEnergy = myController.axieIngameStats.MinEnergy;
        myController.SkeletonAnim.loop = true;
        DoAction(AxieState.Idle);
    }
}