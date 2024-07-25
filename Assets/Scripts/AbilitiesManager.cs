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

[System.Serializable]
public class AxieClassGraphic
{
    public AxieClass axieClass;
    public Sprite axieClassSprite;
}

public class AbilitiesManager : MonoBehaviour
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
    public AxiesManager axiesManager;
    private GetAxiesExample.Axie currentSelectedAxie;

    public Sprite SelectedSprite;
    public Sprite DeselectedSprite;

    public void LoadUI()
    {
        for (int i = 0; i < TeamManager.instance.currentTeam.AxieIds.Count; i++)
        {
            string localAxieId = TeamManager.instance.currentTeam.AxieIds[i].id;
            SkeletonGraphic localGraphic = TeamGraphics[i];

            localGraphic.skeletonDataAsset = TeamManager.instance.currentTeam.AxieIds[i].skeletonDataAsset;
            localGraphic.startingAnimation = "action/idle/random-0" + Random.Range(1, 5).ToString();
            localGraphic.material = TeamManager.instance.currentTeam.AxieIds[i].skeletonDataAssetMaterial;
            localGraphic.Initialize(true);

            var parent = localGraphic.transform.parent;
            parent.GetComponent<Button>().onClick.RemoveAllListeners();
            parent.GetComponent<Button>().onClick.AddListener(() => { SelectAxie(localAxieId, parent); });
        }

        ButtonHornBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Horn); });
        ButtonMouthBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Mouth); });
        ButtonBackBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Back); });
        ButtonTailBodyPart.onClick.AddListener(() => { ChoosePart(BodyPart.Tail); });
        for (int i = TeamManager.instance.currentTeam.AxieIds.Count - 1; i >= 0; i--)
        {
            SelectAxie(TeamManager.instance.currentTeam.AxieIds[i].id, TeamGraphics[i].transform.parent);
        }
    }


    public void ChoosePart(BodyPart part)
    {
        GetAxiesExample.Part bodyPartToReplace =
            currentSelectedAxie.parts.FirstOrDefault(x => x.selected && x.order == 1);

        GetAxiesExample.Part bodyPartToSelect =
            currentSelectedAxie.parts.Single(x => x.BodyPart == part);

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

        // Set the selected part as the last orde
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
        AxieBodyPart ability = skillList.axieBodyParts
            .Single(x =>
                x.bodyPart == part && bodyPartToSelect.partClass == x.bodyPartClass &&
                x.skillName == bodyPartToSelect.SkillName);
        AbilityDescriptionText.text = ability.description;

        ShieldAbilityText.text = ability.shield.ToString();
        AttackAbilityText.text = ability.damage.ToString();

        var AxieSelecteds = currentSelectedAxie.parts.Where(x => x.selected).OrderBy(x => x.order).ToList();

        axiesManager.axieControllers.Single(x => x.AxieId.ToString() == currentSelectedAxie.id).axieSkillController.SetAxieSkills(AxieSelecteds.Select(x => x.SkillName).ToList(),
                AxieSelecteds.Select(x => x.BodyPart).ToList());

        ButtonMouthBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonBackBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonHornBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonTailBodyPart.GetComponent<Image>().sprite = DeselectedSprite;

        if (AxieSelecteds.Any(x => x.BodyPart == BodyPart.Mouth))
        {
            ButtonMouthBodyPart.GetComponent<Image>().sprite = SelectedSprite;
        }

        if (AxieSelecteds.Any(x => x.BodyPart == BodyPart.Back))
        {
            ButtonBackBodyPart.GetComponent<Image>().sprite = SelectedSprite;
        }

        if (AxieSelecteds.Any(x => x.BodyPart == BodyPart.Tail))
        {
            ButtonTailBodyPart.GetComponent<Image>().sprite = SelectedSprite;
        }

        if (AxieSelecteds.Any(x => x.BodyPart == BodyPart.Horn))
        {
            ButtonHornBodyPart.GetComponent<Image>().sprite = SelectedSprite;
        }
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

            AbilityNameText.text = partObj.name;
            AxieBodyPart ability = skillList.axieBodyParts
                .Single(x =>
                    x.bodyPart == partObj.BodyPart && partObj.partClass == x.bodyPartClass &&
                    x.skillName == partObj.SkillName);
            AbilityDescriptionText.text = ability.description;

            ShieldAbilityText.text = ability.shield.ToString();
            AttackAbilityText.text = ability.damage.ToString();

            var AxieSelecteds = currentSelectedAxie.parts.Where(x => x.selected).OrderBy(x => x.order).ToList();

            axiesManager.axieControllers.Single(x => x.AxieId.ToString() == currentSelectedAxie.id).axieSkillController
                .SetAxieSkills(AxieSelecteds.Select(x => x.SkillName).ToList(),
                    AxieSelecteds.Select(x => x.BodyPart).ToList());

            ButtonMouthBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
            ButtonBackBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
            ButtonHornBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
            ButtonTailBodyPart.GetComponent<Image>().sprite = DeselectedSprite;

            if (AxieSelecteds.Any(x => x.BodyPart == BodyPart.Mouth))
            {
                ButtonMouthBodyPart.GetComponent<Image>().sprite = SelectedSprite;
            }

            if (AxieSelecteds.Any(x => x.BodyPart == BodyPart.Back))
            {
                ButtonBackBodyPart.GetComponent<Image>().sprite = SelectedSprite;
            }

            if (AxieSelecteds.Any(x => x.BodyPart == BodyPart.Tail))
            {
                ButtonTailBodyPart.GetComponent<Image>().sprite = SelectedSprite;
            }

            if (AxieSelecteds.Any(x => x.BodyPart == BodyPart.Horn))
            {
                ButtonHornBodyPart.GetComponent<Image>().sprite = SelectedSprite;
            }
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
            axie.parts = TeamManager.instance.currentTeam.AxieIds.Single(x => x.id == axieId).parts;

            if (axie.parts.Count(x => x.selected) == 0)
            {
                ChoosePart(BodyPart.Horn);
                ChoosePart(BodyPart.Mouth);
            }
            else
            {
                ChoosePartOnlyDo();
            }
        }
    }
}