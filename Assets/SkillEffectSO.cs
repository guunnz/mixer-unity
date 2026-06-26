using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillEffectsManager", menuName = "Monster/SkillEffectSO")]
public class SkillEffectSO : ScriptableObject
{
    public List<StatusEffectGraphics> statusEffectGraphicsList;

    public Sprite GetStatusEffectGraphic(StatusEffectEnum statusEffect)
    {
        return statusEffectGraphicsList.Single(x => x.statusEffectEnum == statusEffect).sprite;
    }
}