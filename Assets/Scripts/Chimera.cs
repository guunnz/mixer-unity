using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Spine;
using Spine.Unity;
using Spine.Unity.Prototyping;

public class Chimera : MonoBehaviour
{
    public Team chimeraTeam;
    public AxieController target;
    public float attackSpeed = 2;
    public float attackRange = 2;
    private float attackBuffer = 2.1333f;
    private float initialScaleX;
    public SkeletonAnimation skeletonAnimation;
    public SkeletonAnimation chimeraAttackSfx;

    // Parameters for step-based movement
    public float moveStep = 1f; // Step size
    public float moveInterval = 1.333f; // Time between steps
    private bool isMoving = false;
    public SpineEventUnityHandler handler;

    void Start()
    {
        initialScaleX = transform.localScale.x; // Store the initial scale o
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

        chimeraAttackSfx.Initialize(true);
        target.axieIngameStats.currentHP -= target.axieIngameStats.maxHP / 5f;
        Debug.Log("Damaged target. Target HP: " + target.axieIngameStats.currentHP);
    }

    void FixedUpdate()
    {
        if (chimeraTeam.battleEnded ||
            chimeraTeam.enemyTeam.battleEnded && skeletonAnimation.AnimationName != "action/random-01")
        {
            if (chimeraTeam.GetAliveCharacters().Count != 0)
            {
                skeletonAnimation.AnimationName = "action/random-01";
                skeletonAnimation.loop = true;
                skeletonAnimation.Initialize(true);
            }
            else
            {
                Destroy(this.gameObject);
            }

            return;
        }

        if (target == null || target.axieBehavior.axieState == AxieState.Killed)
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
        List<AxieController> potentialTargets = chimeraTeam.enemyTeam.GetAliveCharacters();

        List<AxieController> potentialTargetsWithoutStenched = potentialTargets
            .Where(x => !x.axieSkillEffectManager.IsStenched())
            .ToList();

        List<AxieController> targetsToUse = potentialTargetsWithoutStenched.Count > 0
            ? potentialTargetsWithoutStenched
            : potentialTargets;

        float closestDistance = Mathf.Infinity;
        AxieController closestTarget = null;

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
            skeletonAnimation.AnimationName = "attack/ranged/cast-high";
            skeletonAnimation.Initialize(true);
            handler.SetEvents();
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

            skeletonAnimation.AnimationName = "action/move-forward";
            skeletonAnimation.Initialize(true);
            // Use DOTween to move the object
            transform.DOMove(targetPosition, moveInterval - 0.3f);
            yield return new WaitForSeconds(moveInterval);
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