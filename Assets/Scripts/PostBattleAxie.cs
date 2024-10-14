using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GetAxiesExample;

public class PostBattleAxie : MonoBehaviour
{
    public SkeletonGraphic skeletonGraphic;
    public TextMeshProUGUI text;
    public Image image;

    public List<Image> skillList;
    public void SetPostBattleAxie(SkeletonDataAsset dataAsset, Material skeletonDataAssetMaterial, Color imageColor, float fillAmount, float value, List<AxieBodyPart> bodyPartList)
    {
        skeletonGraphic.skeletonDataAsset = dataAsset;
        skeletonGraphic.material = skeletonDataAssetMaterial;
        skeletonGraphic.startingAnimation = "action/idle/normal";
        skeletonGraphic.Initialize(true);
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
                skillList[i].sprite = AbilitiesManager.instance.GetSkillSprite(bodyPartList[i]);
            }
        }
        image.color = imageColor;
        image.fillAmount = fillAmount;
        text.text = Mathf.RoundToInt(value).ToString();
    }
}
