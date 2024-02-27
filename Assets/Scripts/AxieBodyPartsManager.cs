using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AxieBodyPartsManager", menuName = "Axie/BodyPartsManager")]
public class AxieBodyPartsManager : ScriptableObject
{
    public List<AxieBodyPart> axieBodyParts;

    public void AddBodyPart(AxieBodyPart bodyPart)
    {
        axieBodyParts.Add(bodyPart);
    }
}