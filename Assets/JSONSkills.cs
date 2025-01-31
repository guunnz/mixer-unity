using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JSONSkills : MonoBehaviour
{
    public List<AxieBodyPart> bodyParts;

    private void Start()
    {

        GUIUtility.systemCopyBuffer = GenerateJsonFromAxieBodyParts(bodyParts);
        Debug.Log(GenerateJsonFromAxieBodyParts(bodyParts));
    }

    public static string GenerateJsonFromAxieBodyParts(List<AxieBodyPart> axieBodyParts)
    {
        var axieBodyPartsInfo = axieBodyParts.Select(axie => new
        {
            SkillName = axie.skillName.ToString(),
            BodyPart = axie.bodyPart.ToString(),
            BodyPartClass = axie.bodyPartClass.ToString(),
            Damage = axie.damage,
            Shield = axie.shield,
            IsPassive = axie.isPassive,
            Energy = axie.energy,
            Description = axie.description
        }).ToList();

        return JsonConvert.SerializeObject(axieBodyPartsInfo, Formatting.Indented);
    }

}
