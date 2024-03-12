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
    ApplyAdjacentTargetAndSelf,
    ApplyAdjacentTargetAndTarget,
    ApplyTeam,
    ApplyEnemyTeam,
    AllAxies,
    ApplyAllied
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

    public void SetStatus(SkillEffect effect, AxieController self, AxieController target)
    {
        List<OverlayTile> adjacentTiles;
        switch (effect.statusApplyType)
        {
            case StatusApplyType.ApplySelf:
                self.AddStatusEffect(effect);
                break;
            case StatusApplyType.ApplyTarget:
                target.AddStatusEffect(effect);
                break;
            case StatusApplyType.RemoveSelf:
                self.RemoveAllEffects();
                break;
            case StatusApplyType.RemoveTarget:
                target.RemoveAllEffects();
                break;
            case StatusApplyType.ApplyAdjacentTarget:
                adjacentTiles = MapManager.Instance.GetAdjacentTiles(target.standingOnTile);
                foreach (var OverlayTile in adjacentTiles)
                {
                    OverlayTile.currentOccupier.RemoveAllEffects();
                }
                break;
            case StatusApplyType.ApplySelfAndEnemy:
                target.AddStatusEffect(effect);
                self.AddStatusEffect(effect);
                break;
            case StatusApplyType.RemoveSelfAndTarget:
                target.RemoveAllEffects();
                self.AddStatusEffect(effect);
                break;
            case StatusApplyType.ApplyAdjacentTargetAndSelf:
                target = self;
                adjacentTiles = MapManager.Instance.GetAdjacentTiles(target.standingOnTile);
                foreach (var overlayTile in adjacentTiles)
                {
                    overlayTile.currentOccupier.AddStatusEffect(effect);
                }
                break;
            case StatusApplyType.ApplyAdjacentTargetAndTarget:
                adjacentTiles = MapManager.Instance.GetAdjacentTiles(target.standingOnTile);
                foreach (var overlayTile in adjacentTiles)
                {
                    overlayTile.currentOccupier.AddStatusEffect(effect);
                }
                break;
            case StatusApplyType.StealTargetFromSelf:
                foreach (var skillEffect in target.GetAllSkillEffectsNotPassives())
                {
                    self.AddStatusEffect(skillEffect);
                }
                target.RemoveAllEffects();
                break;
            case StatusApplyType.StealSelfFromTarget:
                foreach (var skillEffect in self.GetAllSkillEffectsNotPassives())
                {
                    target.AddStatusEffect(skillEffect);
                }
                self.RemoveAllEffects();
                break;
            case StatusApplyType.ApplyTeam:
                foreach (var teammate in self.goodTeam.GetCharacters())
                {
                    teammate.AddStatusEffect(effect);
                }
                break;
            case StatusApplyType.ApplyEnemyTeam:
                foreach (var teammate in target.goodTeam.GetCharacters())
                {
                    teammate.AddStatusEffect(effect);
                }
                break;
            case StatusApplyType.AllAxies:

                foreach (var enemy in target.goodTeam.GetCharacters())
                {
                    enemy.AddStatusEffect(effect);
                }

                foreach (var teammate in self.goodTeam.GetCharacters())
                {
                    teammate.AddStatusEffect(effect);
                }
                break;
        }
    }
}