using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class JSONSkills : MonoBehaviour
{
    public List<MonsterBodyPart> bodyParts;

    private void Start()
    {

        GUIUtility.systemCopyBuffer = GenerateJsonFromMonsterBodyParts(bodyParts);
        Debug.Log(GenerateJsonFromMonsterBodyParts(bodyParts));
    }

    public static string GenerateJsonFromMonsterBodyParts(List<MonsterBodyPart> monsterBodyParts)
    {
        var monsterBodyPartsInfo = monsterBodyParts.Select(monster => new
        {
            SkillName = monster.skillName.ToString(),
            BodyPart = monster.bodyPart.ToString(),
            BodyPartClass = monster.bodyPartClass.ToString(),
            Damage = monster.damage,
            Shield = monster.shield,
            IsPassive = monster.isPassive,
            Energy = monster.energy,
            Description = monster.description
        }).ToList();

        return JsonConvert.SerializeObject(monsterBodyPartsInfo, Formatting.Indented);
    }

}
