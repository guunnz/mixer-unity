using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "AxieBodyPartsManager", menuName = "Axie/BodyPartsManager")]
public class AxieBodyPartsManager : ScriptableObject
{
    public List<AxieBodyPart> axieBodyParts;

    public void AddBodyPart(AxieBodyPart bodyPart)
    {
        axieBodyParts.Add(bodyPart);
    }
}

[System.Serializable]
public class StatusEffectGraphics
{
    public Sprite sprite;
    public StatusEffectEnum statusEffectEnum;
}

