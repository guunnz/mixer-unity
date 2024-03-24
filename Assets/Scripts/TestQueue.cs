using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAction
{
    public Action Action;
    public float triggerTime;

    public SkillAction(Action action, float triggerTime)
    {
        this.Action = action;
        this.triggerTime = triggerTime;
    }
}

public class TestQueue : MonoBehaviour
{
    private List<SkillAction> skillActionsList = new List<SkillAction>();


    private void Start()
    {
        StartCoroutine(AxieSkillCoroutine());
    }

    public IEnumerator AxieSkillCoroutine()
    {
        float timer = 0;
        while (skillActionsList.Count > 0)
        {
            foreach (var skillAction in skillActionsList)
            {
                if (timer >= skillAction.triggerTime)
                {
                    skillAction.Action.Invoke();
                }
            }

            skillActionsList.RemoveAll(x => timer >= x.triggerTime);
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
    }

    public void DoAttackAnimation()
    {
        Debug.Log("Trigger Attack Animation");
    }

    public void TriggerStatusEffect()
    {
        Debug.Log("Trigger status effect");
    }

    public void DealDamage()
    {
        Debug.Log("Trigger deal damage");
    }

    public void GainShield()
    {
        Debug.Log("Trigger gain shield");
    }
}