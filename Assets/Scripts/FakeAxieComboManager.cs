using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FakeAxieComboManager : MonoBehaviour
{
    public List<SkeletonGraphic> TeamGraphics = new List<SkeletonGraphic>();
    public List<AxiePartGraphic> BodyPartGraphics = new List<AxiePartGraphic>();
    public List<AxieClassGraphic> AxieClassGraphics = new List<AxieClassGraphic>();
    public SkeletonGraphic SkeletonGraphicCombo;
    public TextMeshProUGUI AbilityNameText;
    public TextMeshProUGUI AbilityDescriptionText;
    public TextMeshProUGUI AxieNameText;
    public TextMeshProUGUI ShieldAbilityText;
    public TextMeshProUGUI AttackAbilityText;
    public AxieBodyPartsManager skillList;
    public Image HornBodyPart;
    public Image MouthBodyPart;
    public Image BackBodyPart;
    public Image TailBodyPart;
    public Image axieClassImage;

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
    public FakeAxiesManager axiesManager;
    private GetAxiesExample.Axie currentSelectedAxie;
    public Sprite SelectedSprite;
    public Sprite DeselectedSprite;

    public void LoadUI()
    {
        for (int i = 0; i < axiesManager.instantiatedAxies.Count; i++)
        {
            string localAxieId = axiesManager.instantiatedAxies[i].axie.id;
            SkeletonGraphic localGraphic = TeamGraphics[i];

            localGraphic.skeletonDataAsset = axiesManager.instantiatedAxies[i].axie.skeletonDataAsset;
            localGraphic.startingAnimation = "action/idle/random-0" + Random.Range(1, 5).ToString();
            localGraphic.material = axiesManager.instantiatedAxies[i].axie.skeletonDataAssetMaterial;
            localGraphic.Initialize(true);

            var parent = localGraphic.transform.parent;
            parent.GetComponent<Button>().onClick.RemoveAllListeners();

            parent.GetComponent<Button>().onClick.AddListener(() => { SelectAxie(localAxieId, parent); });
        }

        ButtonHornBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Horn); });
        ButtonMouthBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Mouth); });
        ButtonBackBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Back); });
        ButtonTailBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Tail); });
        for (int i = axiesManager.instantiatedAxies.Count - 1; i >= 0; i--)
        {
            SelectAxie(axiesManager.instantiatedAxies[i].axie.id, TeamGraphics[i].transform.parent);
        }
    }

    public void ChoosePart(BodyPart part)
    {
        GetAxiesExample.Part bodyPartToReplace =
            currentSelectedAxie.parts.FirstOrDefault(x => x.selected && x.order == 1);

        GetAxiesExample.Part bodyPartToSelect =
            currentSelectedAxie.parts.Single(x => x.BodyPart == part);

        // if (currentSelectedAxie.parts.Where(x => x.selected).Contains(bodyPartToSelect))
        // {
        //     bodyPartToReplace = null;
        // }

        if (bodyPartToSelect.order == currentSelectedAxie.maxBodyPartAmount)
        {
            return;
        }

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
        ButtonMouthBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonBackBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonHornBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonTailBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
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
                    ButtonBackBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    BackBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Mouth:
                    MouthBodyPartOrderText.text = partObj.order + "°";
                    ButtonMouthBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    MouthBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Horn:
                    HornBodyPartOrderText.text = partObj.order + "°";
                    ButtonHornBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    HornBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Tail:
                    TailBodyPartOrderText.text = partObj.order + "°";
                    ButtonTailBodyPart.GetComponent<Image>().sprite = SelectedSprite;
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
                ButtonBackBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                BackBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Mouth:
                MouthBodyPartOrderText.text = bodyPartToSelect.order + "°";
                ButtonMouthBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                MouthBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Horn:
                HornBodyPartOrderText.text = bodyPartToSelect.order + "°";
                ButtonHornBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                HornBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Tail:
                TailBodyPartOrderText.text = bodyPartToSelect.order + "°";
                ButtonTailBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                TailBodyPartOrderImage.SetActive(true);
                break;
        }

        AbilityNameText.text = bodyPartToSelect.name;
        AxieBodyPart ability = skillList.axieBodyParts
            .Single(x =>
                x.bodyPart == part && bodyPartToSelect.partClass == x.bodyPartClass &&
                x.skillName == bodyPartToSelect.SkillName);
        AbilityDescriptionText.text = ability.description +
                                      (ability.isPassive ? " (PASSIVES/BATTLECRIES DO NOT WORK FOR NOW)" : "");

        ShieldAbilityText.text = ability.shield.ToString();
        AttackAbilityText.text = ability.damage.ToString();
    }

    public void ChoosePartOnlyDo()
    {
        HornBodyPartOrderImage.SetActive(false);
        BackBodyPartOrderImage.SetActive(false);
        MouthBodyPartOrderImage.SetActive(false);
        TailBodyPartOrderImage.SetActive(false);

        ButtonMouthBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonBackBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonHornBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonTailBodyPart.GetComponent<Image>().sprite = DeselectedSprite;

        foreach (var partObj in currentSelectedAxie.parts.Where(x => x.selected))
        {
            switch (partObj.BodyPart)
            {
                case BodyPart.Back:
                    BackBodyPartOrderText.text = partObj.order + "°";
                    ButtonBackBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    BackBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Mouth:
                    MouthBodyPartOrderText.text = partObj.order + "°";
                    ButtonMouthBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    MouthBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Horn:
                    HornBodyPartOrderText.text = partObj.order + "°";
                    ButtonHornBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    HornBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Tail:
                    TailBodyPartOrderText.text = partObj.order + "°";
                    ButtonTailBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    TailBodyPartOrderImage.SetActive(true);
                    break;
            }

            AbilityNameText.text = partObj.name;
            AbilityDescriptionText.text = skillList.axieBodyParts
                .Single(x =>
                    x.bodyPart == partObj.BodyPart && partObj.partClass == x.bodyPartClass &&
                    x.skillName == partObj.SkillName).description;
        }
    }

    public void SelectAxie(string axieId, Transform parent)
    {
        foreach (var skeletonGraphic in TeamGraphics)
        {
            skeletonGraphic.transform.parent.GetComponent<Image>().sprite = DeselectedSprite;
        }

        parent.GetComponent<Image>().sprite = SelectedSprite;
        GetAxiesExample.Axie axie = AccountManager.userAxies.results.Single(x => x.id == axieId);

        axieClassImage.sprite = AxieClassGraphics.Single(x => x.axieClass == axie.axieClass).axieClassSprite;
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