using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GetMonstersExample;

public class PostBattleMonster : MonoBehaviour
{
    public VanillaMonsterGraphic monsterGraphic;
    public TextMeshProUGUI text;
    public Image image;

    public List<Image> skillList;
    public void SetPostBattleMonster(MonsterVisualDescriptor descriptor, Color imageColor, float fillAmount, float value, List<MonsterBodyPart> bodyPartList)
    {
        VanillaMonsterGraphic graphic = EnsureGraphic();
        graphic.SetDescriptor(descriptor);
        graphic.startingAnimation = "action/idle/normal";
        graphic.Initialize(true);
        if (skillList.Count > 0)
        {
            foreach (var item in skillList)
            {
                item.enabled = false;
            }
            for (int i = 0; i < bodyPartList.Count; i++)
            {
                skillList[i].enabled = true;
                skillList[i].GetComponent<AbilityTooltip>().skillNameTooltip = bodyPartList[i].skillName;
                VanillaMonsterIconUtility.ApplyBodyPart(skillList[i], bodyPartList[i]);
            }
        }
        image.color = imageColor;
        image.fillAmount = fillAmount;
        text.text = Mathf.RoundToInt(value).ToString();
    }

    private VanillaMonsterGraphic EnsureGraphic()
    {
        monsterGraphic = VanillaMonsterGraphic.EnsureCenteredChild(transform, monsterGraphic);
        return monsterGraphic;
    }
}
