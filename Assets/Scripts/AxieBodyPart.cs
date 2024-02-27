using UnityEngine;

[CreateAssetMenu(fileName = "AxieBodyPart", menuName = "Axie/BodyPart")]
public class AxieBodyPart : ScriptableObject
{
    public SkillName skillName;
    public BodyPart bodyPart;
    public GameObject prefab; // Prefab can be assigned in the editor if needed
}