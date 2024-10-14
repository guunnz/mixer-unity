using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatusApplyType
{
    ApplySelf,
    ApplyTarget,
    ApplySelfAndEnemy,
    StealTargetFromSelf,
    StealSelfFromTarget,
    RemoveSelf,
    RemoveTarget,
    RemoveSelfAndTarget,
    ApplyAdjacentTarget,
    ApplyAdjacentSelfAndSelf,
    ApplyAdjacentTargetAndTarget,
    ApplyTeam,
    ApplyEnemyTeam,
    AllAxies,
    ApplyAllied,
    ApplyAdjacentSelf,
}

public class StatusManager : MonoBehaviour
{
    static public StatusManager Instance;

    public SkillEffectSO skillEffects;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
    }

    public void SetStatus(SkillEffect effect, AxieController target, bool remove, string skillerId)
    {
        if (remove)
        {
            target.RemoveAllEffects();
        }
        else
        {
            target.AddStatusEffect(effect);
        }
    }
}