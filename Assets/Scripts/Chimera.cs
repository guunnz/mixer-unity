using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class Chimera : MonoBehaviour
{
    public Team chimeraTeam;
    public MonsterController target;
    public float attackSpeed = 2;
    public float attackRange = 2;
    private float attackBuffer = 2.1333f;
    private float initialScaleX;
    public VanillaMonsterVisual visual;
    public GameObject chimeraAttackSfx;

    // Parameters for step-based movement
    public float moveStep = 1f; // Step size
    public float moveInterval = 1.333f; // Time between steps
    private bool isMoving = false;

    void Start()
    {
        if (visual == null)
            visual = VanillaMonsterVisual.Ensure(gameObject);

        visual.SetDescriptor(MonsterVisualDescriptor.Default(MonsterClass.Dusk));
        visual.Play(MonsterVisualState.Idle, true);
        initialScaleX = -transform.localScale.x; // Store the initial scale o
    }

    public void DoDamage()
    {
        Vector3 direction = (target.transform.position - transform.position).normalized;

        // Flip local scale in X based on movement direction
        if (direction.x > 0)
        {
            transform.localScale = new Vector3(!target.imGood ? -initialScaleX : initialScaleX, transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < 0)
        {
            transform.localScale = new Vector3(!target.imGood ? initialScaleX : -initialScaleX, transform.localScale.y, transform.localScale.z);
        }

        if (chimeraAttackSfx != null)
            chimeraAttackSfx.SetActive(true);

        target.monsterIngameStats.currentHP -= target.monsterIngameStats.maxHP / 5f;
        Debug.Log("Damaged target. Target HP: " + target.monsterIngameStats.currentHP);
    }

    void FixedUpdate()
    {
        if (chimeraTeam.battleEnded || chimeraTeam.enemyTeam.battleEnded)
        {
            if (chimeraTeam.GetAliveCharacters().Count != 0)
            {
                visual.Play(MonsterVisualState.Victory, true);
            }
            else
            {
                Destroy(this.gameObject);
            }

            return;
        }

        if (target == null || target.monsterBehavior.monsterState == MonsterState.Killed)
        {
            FindClosestEnemy();
        }


        if (target != null)
        {
            var transform1 = target.transform;
            var position = transform1.position;
            bool isOnAttackDistance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z),
                new Vector2(position.x, position.z)) <= attackRange;
            if (isOnAttackDistance &&
                attackBuffer <= 0)
            {
                DoDamageAnim();
                attackBuffer = 2.1333f / attackSpeed; // Reset attack buffer based on attack speed
            }
            else if (!isMoving && !isOnAttackDistance)
            {
                StartCoroutine(MoveToTarget());
            }
            else
            {
                if (attackBuffer > 0)
                {
                    attackBuffer -= Time.deltaTime; // Decrease the attack buffer over time
                }
            }
        }
    }

    public void FindClosestEnemy()
    {
        List<MonsterController> potentialTargets = chimeraTeam.enemyTeam.GetAliveCharacters();

        List<MonsterController> potentialTargetsWithoutStenched = potentialTargets
            .Where(x => !x.monsterSkillEffectManager.IsStenched())
            .ToList();

        List<MonsterController> targetsToUse = potentialTargetsWithoutStenched.Count > 0
            ? potentialTargetsWithoutStenched
            : potentialTargets;

        float closestDistance = Mathf.Infinity;
        MonsterController closestTarget = null;

        foreach (var potentialTarget in targetsToUse)
        {
            float distance = Vector3.Distance(transform.position, potentialTarget.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = potentialTarget;
            }
        }

        target = closestTarget;
    }

    public void DoDamageAnim()
    {
        if (target != null)
        {
            visual.Play(MonsterVisualState.AttackRanged, false);
            CancelInvoke(nameof(DoDamage));
            Invoke(nameof(DoDamage), visual.GetDuration(MonsterVisualState.AttackRanged) * 0.5f);
        }
    }

    public IEnumerator MoveToTarget()
    {
        if (target != null)
        {
            isMoving = true;

            // Calculate direction and distance to move
            Vector3 direction = (target.transform.position - transform.position).normalized;
            Vector3 targetPosition = transform.position + direction * moveStep;

            // Flip local scale in X based on movement direction
            if (direction.x > 0)
            {
                transform.localScale = new Vector3(!target.imGood ? -initialScaleX : initialScaleX, transform.localScale.y, transform.localScale.z); ;
            }
            else if (direction.x < 0)
            {
                transform.localScale = new Vector3(!target.imGood ? initialScaleX : -initialScaleX, transform.localScale.y, transform.localScale.z);
            }

            visual.Play(MonsterVisualState.Run, true);
            // Use DOTween to move the object
            transform.DOMove(targetPosition, moveInterval - 0.3f);
            yield return new WaitForSeconds(moveInterval);
            visual.Play(MonsterVisualState.Idle, true);
            isMoving = false;
            if (target == null)
                yield break;
            if (Vector3.Distance(transform.position, target.transform.position) > attackRange)
            {
                StartCoroutine(MoveToTarget());
            }

            Debug.Log("Moving to target. Current position: " + transform.position + " Scale: " + transform.localScale);
        }
    }
}
