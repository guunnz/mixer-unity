using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[System.Serializable]
public class AxiePartGraphic
{
    public BodyPart bodyPart;
    public AxieClass axieClass;
    public Sprite bodyPartSprite;
}

public class AbilitiesManager : MonoBehaviour
{
    public AxiesManager manager;
    public List<SkeletonGraphic> TeamGraphics = new List<SkeletonGraphic>();
    public List<AxiePartGraphic> BodyPartGraphics = new List<AxiePartGraphic>();
    public SkeletonGraphic SkeletonGraphicCombo;
    public TextMeshProUGUI AbilityNameText;
    public TextMeshProUGUI AbilityDescriptionText;
    public TextMeshProUGUI AxieNameText;
    public AxieBodyPartsManager skillList;
    public Image HornBodyPart;
    public Image MouthBodyPart;
    public Image BackBodyPart;
    public Image TailBodyPart;

    public Button ButtonHornBodyPart;
    public Button ButtonMouthBodyPart;
    public Button ButtonBackBodyPart;
    public Button ButtonTailBodyPart;


    public GameObject HornBodyPartOrderImage;
    public GameObject MouthBodyPartOrderImage;
    public GameObject BackBodyPartOrderImage;
    public GameObject TailBodyPartOrderImage;

    public TextMeshProUGUI HornBodyPartOrderText;
    public TextMeshProUGUI MouthBodyPartOrderText;
    public TextMeshProUGUI BackBodyPartOrderText;
    public TextMeshProUGUI TailBodyPartOrderText;

    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI SpeedText;
    public TextMeshProUGUI MoraleText;
    public TextMeshProUGUI SkillText;
    public AxiesManager axiesManager;
    private GetAxiesExample.Axie currentSelectedAxie;

    public void LoadUI()
    {
        for (int i = 0; i < axiesManager.currentTeam.Count; i++)
        {
            string localAxieId = axiesManager.currentTeam[i].id;
            SkeletonGraphic localGraphic = TeamGraphics[i];

            localGraphic.skeletonDataAsset = axiesManager.currentTeam[i].skeletonDataAsset;
            localGraphic.startingAnimation = "action/idle/random-0" + Random.Range(1, 5).ToString();
            localGraphic.material = axiesManager.currentTeam[i].skeletonDataAssetMaterial;
            localGraphic.Initialize(true);

            var parent = localGraphic.transform.parent;
            parent.GetComponent<Button>().onClick.RemoveAllListeners();

            parent.GetComponent<Button>().onClick.AddListener(() => { SelectAxie(localAxieId); });
        }

        ButtonHornBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Horn); });
        ButtonMouthBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Mouth); });
        ButtonBackBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Back); });
        ButtonTailBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Tail); });
        SelectAxie(axiesManager.currentTeam[0].id);
    }


    public void ChoosePart(BodyPart part)
    {
        GetAxiesExample.Part bodyPartToReplace =
            currentSelectedAxie.parts.FirstOrDefault(x => x.selected && x.order == 1);

        GetAxiesExample.Part bodyPartToSelect =
            currentSelectedAxie.parts.Single(x => x.BodyPart == part);

        int amountSelected = currentSelectedAxie.parts.Count(x => x.selected);

        if (bodyPartToReplace != null && amountSelected >= currentSelectedAxie.maxBodyPartAmount)
        {
            bodyPartToReplace.order = 1;
            bodyPartToReplace.selected = false;
        }

        HornBodyPartOrderImage.SetActive(false);
        BackBodyPartOrderImage.SetActive(false);
        MouthBodyPartOrderImage.SetActive(false);
        TailBodyPartOrderImage.SetActive(false);
        foreach (var partObj in currentSelectedAxie.parts.Where(x => x.selected && x != bodyPartToSelect))
        {
            if (partObj.order > 1)
            {
                partObj.order -= 1;
            }

            switch (partObj.BodyPart)
            {
                case BodyPart.Back:
                    BackBodyPartOrderText.text = partObj.order + "°";
                    BackBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Mouth:
                    MouthBodyPartOrderText.text = partObj.order + "°";
                    MouthBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Horn:
                    HornBodyPartOrderText.text = partObj.order + "°";
                    HornBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Tail:
                    TailBodyPartOrderText.text = partObj.order + "°";
                    TailBodyPartOrderImage.SetActive(true);
                    break;
            }
        }

        // Find the maximum order among selected parts
        int maxOrder = currentSelectedAxie.parts
            .Where(x => x.selected)
            .Select(x => (int?)x.order)
            .DefaultIfEmpty(0)
            .Max() ?? 0;

        // Set the selected part as the last order
        bodyPartToSelect.selected = true;
        bodyPartToSelect.order = maxOrder + 1;

        switch (bodyPartToSelect.BodyPart)
        {
            case BodyPart.Back:
                BackBodyPartOrderText.text = bodyPartToSelect.order + "°";
                BackBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Mouth:
                MouthBodyPartOrderText.text = bodyPartToSelect.order + "°";
                MouthBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Horn:
                HornBodyPartOrderText.text = bodyPartToSelect.order + "°";
                HornBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Tail:
                TailBodyPartOrderText.text = bodyPartToSelect.order + "°";
                TailBodyPartOrderImage.SetActive(true);
                break;
        }

        AbilityNameText.text = bodyPartToSelect.name;
        AbilityDescriptionText.text = skillList.axieBodyParts
            .Single(x =>
                x.bodyPart == part && bodyPartToSelect.partClass == x.bodyPartClass &&
                x.skillName == bodyPartToSelect.SkillName).description;

        var AxieSelecteds = currentSelectedAxie.parts.Where(x => x.selected).OrderBy(x => x.order).ToList();

        axiesManager.axieControllers.Single(x => x.AxieId.ToString() == currentSelectedAxie.id).axieSkillController
            .SetAxieSkills(AxieSelecteds.Select(x => x.SkillName).ToList(),
                AxieSelecteds.Select(x => x.BodyPart).ToList());
    }

    public void ChoosePartOnlyDo()
    {
        HornBodyPartOrderImage.SetActive(false);
        BackBodyPartOrderImage.SetActive(false);
        MouthBodyPartOrderImage.SetActive(false);
        TailBodyPartOrderImage.SetActive(false);
        foreach (var partObj in currentSelectedAxie.parts.Where(x => x.selected))
        {
            switch (partObj.BodyPart)
            {
                case BodyPart.Back:
                    BackBodyPartOrderText.text = partObj.order + "°";
                    BackBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Mouth:
                    MouthBodyPartOrderText.text = partObj.order + "°";
                    MouthBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Horn:
                    HornBodyPartOrderText.text = partObj.order + "°";
                    HornBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Tail:
                    TailBodyPartOrderText.text = partObj.order + "°";
                    TailBodyPartOrderImage.SetActive(true);
                    break;
            }
        }
    }

    public void SelectAxie(string axieId)
    {
        GetAxiesExample.Axie axie = AccountManager.userAxies.results.Single(x => x.id == axieId);

        HealthText.text = axie.stats.hp.ToString();
        SpeedText.text = axie.stats.speed.ToString();
        SkillText.text = axie.stats.skill.ToString();
        MoraleText.text = axie.stats.morale.ToString();
        AxieNameText.text = axie.name;

        currentSelectedAxie = axie;
        AxieClass hornClass = axie.parts.Single(x => x.BodyPart == BodyPart.Horn).partClass;
        AxieClass backClass = axie.parts.Single(x => x.BodyPart == BodyPart.Back).partClass;
        AxieClass mouthClass = axie.parts.Single(x => x.BodyPart == BodyPart.Mouth).partClass;
        AxieClass tailClass = axie.parts.Single(x => x.BodyPart == BodyPart.Tail).partClass;

        HornBodyPart.sprite = BodyPartGraphics.Single(x => x.axieClass == hornClass && x.bodyPart == BodyPart.Horn)
            .bodyPartSprite;
        MouthBodyPart.sprite = BodyPartGraphics.Single(x => x.axieClass == mouthClass && x.bodyPart == BodyPart.Mouth)
            .bodyPartSprite;
        BackBodyPart.sprite = BodyPartGraphics.Single(x => x.axieClass == backClass && x.bodyPart == BodyPart.Back)
            .bodyPartSprite;
        TailBodyPart.sprite = BodyPartGraphics.Single(x => x.axieClass == tailClass && x.bodyPart == BodyPart.Tail)
            .bodyPartSprite;

        currentSelectedAxie = axie;
        SkeletonGraphicCombo.skeletonDataAsset = axie.skeletonDataAsset;
        SkeletonGraphicCombo.material = axie.skeletonDataAssetMaterial;
        SkeletonGraphicCombo.startingAnimation = "action/idle/normal";
        SkeletonGraphicCombo.Initialize(true);

        if (axie.parts.Any(x => x.selected))
        {
            ChoosePartOnlyDo();
        }
        else
        {
            ChoosePart(BodyPart.Horn);
            ChoosePart(BodyPart.Mouth);
        }
    }
}