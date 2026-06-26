using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;


public enum MonsterState
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

public class MonsterBehavior : MonoBehaviour
{
    public MonsterState monsterState;
    internal MonsterController myController;
    internal MonsterSkillEffectManager monsterSkillEffectManager;
    internal float AttackSpeed;
    private MonsterVisualState AttackVisualState;
    public List<SkillName> SkillList;
    private Coroutine attackCoroutine;
    internal bool shrimping;

    public void SetAttackSpeed()
    {
        AttackSpeed = MonsterStatCalculator.GetAttackSpeed(myController.stats);
    }

    private IEnumerator Start()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        SetAttackSpeed();
        if (myController.monsterIngameStats.monsterClass == MonsterClass.Bird ||
            myController.monsterIngameStats.monsterClass == MonsterClass.Dusk ||
            myController.monsterIngameStats.monsterClass == MonsterClass.Bug)
        {
            AttackVisualState = MonsterVisualState.AttackRanged;
        }
        else if (myController.monsterIngameStats.monsterClass == MonsterClass.Aquatic)
        {
            AttackVisualState = MonsterVisualState.AttackMelee;
        }
        else if (myController.monsterIngameStats.monsterClass == MonsterClass.Beast)
        {
            AttackVisualState = MonsterVisualState.AttackMelee;
        }
        else if (myController.monsterIngameStats.monsterClass == MonsterClass.Plant)
        {
            AttackVisualState = MonsterVisualState.AttackMelee;
        }
        else
        {
            AttackVisualState = MonsterVisualState.AttackMelee;
        }
    }

    private void Update()
    {
        if (this.monsterState == MonsterState.Stunned)
        {
            if (!monsterSkillEffectManager.IsStunned())
            {
                DoAction(MonsterState.Idle);
            }
        }
    }

    private void OnActionPoison()
    {
        if (monsterSkillEffectManager.IsPoisoned())
        {
            var poisonDamage = MonsterStatCalculator.GetPoisonDamage(monsterSkillEffectManager.PoisonStacks());

            foreach (var monster in monsterSkillEffectManager.poisonPlayersList)
            {
                var dmg = poisonDamage / monster.poisonTimes;

                PostBattleManager.Instance.SumDamage(monster.monsterId, dmg, !myController.imGood);
            }

            myController.monsterIngameStats.currentHP -= poisonDamage;
        }
    }

    public void DoShrimp()
    {
        StartCoroutine(GoBackdoor());
    }

    public void DoAction(MonsterState state)
    {
        if (state == MonsterState.Killed)
        {
            shrimping = false;
        }

        if (shrimping && state != MonsterState.Killed)
            return;

        if ((monsterSkillEffectManager != null && monsterSkillEffectManager.IsStunned()) && state != MonsterState.Killed && state != MonsterState.Victory)
        {
            state = MonsterState.Stunned;
        }

        if (state == monsterState)
            return;

        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        monsterState = state;
        switch (state)
        {
            case MonsterState.Attacking:
                attackCoroutine = StartCoroutine(TryAttack());
                break;
            case MonsterState.Casting:
                if (myController.CurrentTarget != null)
                {
                    OnActionPoison();
                    var characters = myController.CurrentTarget.myTeam.GetAliveCharacters();
                    if (myController.CurrentTarget.monsterSkillEffectManager.IsStenched() && characters.Count != 1 && characters.Any(x => !x.monsterSkillEffectManager.IsStenched()))
                    {
                        state = MonsterState.Idle;
                        monsterState = state;
                        myController.CurrentTarget = null;
                        return;
                    }
                }

                CastSpell();
                break;
            case MonsterState.Moving:
                PlayVisual(MonsterVisualState.Run, true);
                break;
            case MonsterState.Shrimping:
                PlayVisual(MonsterVisualState.Shrimp, false);
                DoShrimp();
                break;
            case MonsterState.Stunned:
                PlayVisual(MonsterVisualState.Hit, true);
                break;
            case MonsterState.Victory:
                PlayVisual(MonsterVisualState.Victory, true);
                break;
            case MonsterState.Grabbed:
                PlayVisual(MonsterVisualState.Grabbed, true);
                break;
            case MonsterState.Hovered:
                PlayVisual(MonsterVisualState.Hover, true);
                break;
            case MonsterState.Idle:
                PlayVisual(MonsterVisualState.Idle, true);
                break;
            case MonsterState.Killed:
                if (attackCoroutine != null)
                    StopCoroutine(attackCoroutine);
                attackCoroutine = null;
                myController.standingOnTile.occupied = false;
                myController.gameObject.SetActive(false);
                break;
        }
    }
    public IEnumerator GoBackdoorTarget()
    {
        PlayVisual(MonsterVisualState.Shrimp, false);
        shrimping = true;

        float animationHalfTime = VisualDuration(MonsterVisualState.Shrimp) / 2f;
        yield return new WaitForSeconds(animationHalfTime);
        SetVisualVisible(false);
        Vector3 positionToMove = Vector3.zero;

        if (this.myController.CurrentTarget == null || this.myController.CurrentTarget.standingOnTile == null)
        {

            SetVisualVisible(true);
            PlayVisual(MonsterVisualState.Idle, true);
            shrimping = false;
            yield break;
        }
        List<OverlayTile> possibleTiles = this.myController.CurrentTarget.standingOnTile.AdjacentTiles();

        if (possibleTiles.Count == 0)
        {
            SetVisualVisible(true);
            PlayVisual(MonsterVisualState.Idle, true);
            shrimping = false;
            yield break;
        }
        OverlayTile overlayTile = possibleTiles[0];

        if (overlayTile != null)
        {
            positionToMove = overlayTile.transform.position;
        }
        overlayTile.occupied = true;
        this.myController.standingOnTile = overlayTile;
        this.transform.position = positionToMove;

        yield return new WaitForSecondsRealtime(0.1f);
        SetVisualVisible(true);
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, animationHalfTime - 0.1f));
        PlayVisual(MonsterVisualState.Idle, true);

        if (this.myController.CurrentTarget == null)
        {
            shrimping = false;
            yield break;
        }

        if (this.myController.CurrentTarget.standingOnTile.grid2DLocation.x > this.myController.standingOnTile.grid2DLocation.x)
        {
            MonsterScale.SetFacing(transform, false);
        }
        else
        {
            MonsterScale.SetFacing(transform, true);
        }

        shrimping = false;

    }

    public IEnumerator GoBackdoor()
    {
        shrimping = true;
        PlayVisual(MonsterVisualState.Shrimp, false);

        // Pre-check for null references or invalid states
        if (myController == null || myController.Visual == null)
        {
            SetVisualVisible(true);
            PlayVisual(MonsterVisualState.Idle, true);
            MonsterScale.SetFacing(transform, !MonsterScale.IsFacingPositive(transform));
            shrimping = false;
            Debug.LogError("Vanilla visual is null.");
            yield break; // Exit the coroutine gracefully
        }

        float animationHalfTime = VisualDuration(MonsterVisualState.Shrimp) / 2f;
        if (animationHalfTime <= 0)
        {
            SetVisualVisible(true);
            PlayVisual(MonsterVisualState.Idle, true);
            shrimping = false;
            Debug.LogError("Invalid visual animation time.");
            yield break; // Exit the coroutine gracefully
        }

        yield return new WaitForSeconds(animationHalfTime);
        SetVisualVisible(false);

        if (myController.enemyTeam == null)
        {
            SetVisualVisible(true);
            PlayVisual(MonsterVisualState.Idle, true);
            shrimping = false;
            Debug.LogError("Enemy team reference is null.");
            yield break; // Exit the coroutine gracefully
        }

        Vector3 positionToMove = Vector3.zero;
        List<OverlayTile> possibleTiles = myController.enemyTeam.GetInRangeTiles(myController);
        List<OverlayTile> jumpToTiles =
            possibleTiles.Where(x => x.grid2DLocation.x == (myController.imGood ? 7 : 0)).ToList();
        OverlayTile overlayTile =
            jumpToTiles.FirstOrDefault(x => x.grid2DLocation.y == myController.standingOnTile.grid2DLocation.y);

        if (jumpToTiles.Count == 0 || overlayTile == null)
        {
            jumpToTiles = possibleTiles.Where(x => x.grid2DLocation.x == (myController.imGood ? 6 : 1)).ToList();
            overlayTile = jumpToTiles.FirstOrDefault(x => x.grid2DLocation.y == myController.standingOnTile.grid2DLocation.y);
        }

        if (overlayTile == null)
        {
            SetVisualVisible(true);
            PlayVisual(MonsterVisualState.Idle, true);
            shrimping = false;
            Debug.LogError("No valid overlay tile found for movement.");
            yield break; // Exit the coroutine gracefully
        }

        positionToMove = overlayTile.transform.position;
        overlayTile.occupied = true;
        myController.standingOnTile = overlayTile;
        this.transform.position = positionToMove;

        yield return new WaitForSecondsRealtime(0.1f);
        if (myController.Visual == null)
        {
            SetVisualVisible(true);
            PlayVisual(MonsterVisualState.Idle, true);
            shrimping = false;
            Debug.LogError("Vanilla visual component is missing.");
            yield break; // Exit the coroutine gracefully
        }

        SetVisualVisible(true);

        yield return new WaitForSecondsRealtime(animationHalfTime - 0.1f);
        PlayVisual(MonsterVisualState.Idle, true);
        MonsterScale.SetFacing(transform, !MonsterScale.IsFacingPositive(transform));
        shrimping = false;
        DoAction(MonsterState.Idle);
    }



    public IEnumerator GoBackdoorTile(OverlayTile tile)
    {
        PlayVisual(MonsterVisualState.Shrimp, false);
        float animationHalfTime = VisualDuration(MonsterVisualState.Shrimp) / 2f;
        yield return new WaitForSeconds(animationHalfTime);
        SetVisualVisible(false);
        Vector3 positionToMove = Vector3.zero;

        OverlayTile overlayTile = tile;


        positionToMove = overlayTile.transform.position;


        this.myController.standingOnTile = overlayTile;
        this.transform.position = positionToMove;

        yield return new WaitForSecondsRealtime(0.1f);
        SetVisualVisible(true);
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, animationHalfTime - 0.1f));

        MonsterScale.SetFacing(transform, !MonsterScale.IsFacingPositive(transform));
    }
    public void PerformAttack(int targetDiff, float time)
    {
        // Store the original position
        Vector3 originalPosition = transform.localPosition;

        // Calculate the move distance based on targetDiff
        float moveDistance = targetDiff < 0 ? .2f : -.2f;

        // Create a sequence for the animation
        Sequence moveSequence = DOTween.Sequence();

        var timePerAttack = time / 2.3f;
        var timeWait = time - (timePerAttack * 2);
        // First, move to the target position
        moveSequence.Append(transform.DOLocalMoveZ(originalPosition.z + moveDistance, timePerAttack));
        moveSequence.AppendInterval(timeWait);
        // Then, return to the original position
        moveSequence.Append(transform.DOLocalMoveZ(originalPosition.z, timePerAttack));
    }
    public IEnumerator TryAttack()
    {
        if (myController.CurrentTarget == null)
            yield break;

        while (monsterState == MonsterState.Attacking)
        {
            OnActionPoison();
            float attackSpeedMulti = 1;

            if (myController.monsterSkillEffectManager.IsAromad())
            {
                attackSpeedMulti = 0.75f;
            }
            if (myController.CurrentTarget != null)
            {
                var characters = myController.CurrentTarget.myTeam.GetAliveCharacters();
                if (myController.CurrentTarget.monsterSkillEffectManager.IsStenched() && characters.Count != 1 && characters.Any(x => !x.monsterSkillEffectManager.IsStenched()))
                {
                    monsterState = MonsterState.Idle;
                    myController.CurrentTarget = null;
                    yield break;
                }
            }

            if (!myController.monsterSkillController.OnAutoAttack())
            {
                float speed = VisualDuration(AttackVisualState) / (AttackSpeed * attackSpeedMulti);
                PlayVisual(AttackVisualState, false, speed);
            }

            if (myController.Range <= 1)
            {
                int targetDiff = myController.standingOnTile.grid2DLocation.y - myController.CurrentTarget.standingOnTile.grid2DLocation.y;

                if (targetDiff != 0)
                {
                    PerformAttack(targetDiff, (AttackSpeed * attackSpeedMulti));
                }
            }

            yield return new WaitForSecondsRealtime((AttackSpeed * attackSpeedMulti) / 2f);

            if (myController.Range > 1)
                AutoAttackMaNAGER.instance.SpawnProjectile(myController.transform,
                    myController.CurrentTarget.transform, myController.monsterIngameStats.monsterClass);
            else
            {
                if (myController.monsterSkillEffectManager.IsGravelanted())
                    yield break;

                AutoAttackMaNAGER.instance.SpawnAttack(myController.CurrentTarget.transform, myController.monsterIngameStats.monsterClass);
            }
            yield return new WaitForSecondsRealtime((AttackSpeed * attackSpeedMulti) / 2f);
            if (myController.CurrentTarget != null)
            {
                var characters = myController.CurrentTarget.myTeam.GetAliveCharacters();
                if (myController.CurrentTarget.monsterSkillEffectManager.IsStenched() && characters.Count != 1 && characters.Any(x => !x.monsterSkillEffectManager.IsStenched()))
                {
                    monsterState = MonsterState.Idle;
                    myController.CurrentTarget = null;
                    yield break;
                }
            }
            else
            {
                yield break;
            }

            if (monsterSkillEffectManager.IsFeared())
            {
                //Debug.Log("Missed!");
            }
            else
            {
                var target = myController.CurrentTarget;
                var attackBuff = myController.monsterSkillEffectManager.GetAttackBuff();
                var moraleBuff = myController.monsterSkillEffectManager.GetMoraleBuff();
                var attackSpeedBuff = myController.monsterSkillEffectManager.GetSpeedBuff();
                float attackDamage = (float)MonsterStatCalculator.GetRealAttack(myController.stats, attackBuff, moraleBuff, attackSpeedBuff);

                attackDamage += this.myController.monsterSkillController.passives.AutoattackIncrease;

                float damageReduction = target.monsterSkillController.passives.DamageReductionAmount + (target.monsterSkillEffectManager.GeckoStacks() * 10);

                if (target.monsterSkillEffectManager.IsAromad())
                {
                    damageReduction -= 50;
                }

                attackDamage -= (attackDamage * (damageReduction / 100f));

                if (!myController.monsterSkillEffectManager.IsJinxed() &&
                    !target.monsterSkillController.passives.ImmuneToCriticals)
                {
                    bool isLethal = target.monsterSkillEffectManager.IsLethal();
                    if (isLethal || Random.Range(0, 1f) <= MonsterStatCalculator.GetCritChance(myController.stats, moraleBuff))
                    {
                        attackDamage *= MonsterStatCalculator.GetCritDamage(myController.stats, moraleBuff);
                        target.statsManagerUI.SetCritical();
                        if (isLethal)
                        {
                            target.monsterSkillEffectManager.RemoveStatusEffect(StatusEffectEnum.Lethal);
                        }
                    }
                }

                if (myController.monsterSkillController.passives.HealOnDamageDealt > 0)
                {
                    myController.DoHeal(attackDamage + (attackDamage * (myController.monsterSkillController.passives.HealOnDamageDealt / 100f)), myController.MonsterId.ToString());
                }

                if (myController.monsterSkillController.IgnoresShieldOnAttack())
                {
                    target.monsterIngameStats.currentHP -= attackDamage;
                    target.monsterSkillController.DamageReceived(myController.monsterIngameStats.monsterClass, attackDamage,
                        myController);
                }
                else
                {
                    float shieldDamage = attackDamage - target.monsterIngameStats.currentShield;

                    if (shieldDamage < 0)
                    {
                        target.monsterIngameStats.currentShield -= attackDamage;
                    }
                    else
                    {
                        target.monsterIngameStats.currentShield = 0;
                        target.monsterIngameStats.currentHP -= shieldDamage;
                    }

                    target.monsterSkillController.DamageReceived(myController.monsterIngameStats.monsterClass, attackDamage,
               myController);
                }
            }


            // myController.monsterIngameStats.CurrentEnergy += MonsterStatCalculator.GetManaPerAttack(myController.stats);
        }
    }

    public void CastSpell()
    {
        PlayVisual(MonsterVisualState.Cast, false);
        StartCoroutine(ICastSpell());
    }

    IEnumerator ICastSpell()
    {
        if (myController.CurrentTarget == null)
        {
            myController.monsterIngameStats.CurrentEnergy -= myController.monsterIngameStats.CurrentEnergy * 0.05f;
            PlayVisual(MonsterVisualState.Idle, true);
            DoAction(MonsterState.Idle);
            yield break;
        }

        while (monsterSkillEffectManager.IsStunned())
        {
            yield return new WaitForFixedUpdate();
        }

        if (myController.Range <= 1)
        {
            int targetDiff = myController.standingOnTile.grid2DLocation.y - myController.CurrentTarget.standingOnTile.grid2DLocation.y;

            if (targetDiff != 0)
            {
                PerformAttack(targetDiff, 1.2f);
            }
        }

        yield return StartCoroutine(SkillLauncher.Instance.ThrowSkill(myController.monsterSkillController.GetMonsterSkills(),
            myController.Visual,
            myController.CurrentTarget, this.myController));

        CastSpellAftermath();
    }

    private void CastSpellAftermath()
    {
        myController.monsterIngameStats.CurrentEnergy = myController.monsterIngameStats.MinEnergy;
        PlayVisual(MonsterVisualState.Idle, true);
        DoAction(MonsterState.Idle);
    }

    private void PlayVisual(MonsterVisualState state, bool shouldLoop, float speed = 1f)
    {
        myController?.Visual?.Play(state, shouldLoop, speed);
    }

    private void SetVisualVisible(bool visible)
    {
        myController?.Visual?.SetVisible(visible);
    }

    private float VisualDuration(MonsterVisualState state)
    {
        return myController?.Visual != null ? myController.Visual.GetDuration(state) : 1f;
    }
}
