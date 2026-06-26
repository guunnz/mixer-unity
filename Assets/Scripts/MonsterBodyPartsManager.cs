using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "MonsterBodyPartsManager", menuName = "Monster/BodyPartsManager")]
public class MonsterBodyPartsManager : ScriptableObject
{
    public List<MonsterBodyPart> monsterBodyParts;

    public void AddBodyPart(MonsterBodyPart bodyPart)
    {
        monsterBodyParts.Add(bodyPart);
    }
}

[System.Serializable]
public class StatusEffectGraphics
{
    public Sprite sprite;
    public StatusEffectEnum statusEffectEnum;
}

