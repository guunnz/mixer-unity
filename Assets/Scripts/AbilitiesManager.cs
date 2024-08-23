using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using static GetAxiesExample;

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
    public AbilityDescriptionTooltip AbilityDescriptionTooltip;
    public Image HornBodyPart;
    public Image MouthBodyPart;
    public Image BackBodyPart;
    public Image TailBodyPart;
    public Image axieClassImage;
    public GameObject PassiveGO;

    public Button ButtonHornBodyPart;
    public Button ButtonMouthBodyPart;
    public Button ButtonBackBodyPart;
    public Button ButtonTailBodyPart;
    public GameObject Tutorial;

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

    public TextMeshProUGUI EnergyText;
    public GameObject EnergyObject;

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
       
        if (PlayerPrefs.GetInt("Tutorial") == 0)
        {
            Tutorial.SetActive(true);
            PlayerPrefs.SetInt("Tutorial", 1);
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

        int passivesAdded = 0;

        foreach (var partObj in currentSelectedAxie.parts.Where(x => x.selected && x != bodyPartToSelect))
        {

            bool isPassive = skillList.axieBodyParts
            .FirstOrDefault(x => x.skillName == partObj.SkillName).isPassive;

            if (partObj.order > 1)
            {
                partObj.order -= 1;
            }

            if (isPassive)
                passivesAdded++;

            switch (partObj.BodyPart)
            {
                case BodyPart.Back:
                    BackBodyPartOrderText.text = isPassive ? "P" : partObj.order - passivesAdded + "°";
                    BackBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Mouth:
                    MouthBodyPartOrderText.text = isPassive ? "P" : partObj.order - passivesAdded + "°";
                    MouthBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Horn:
                    HornBodyPartOrderText.text = isPassive ? "P" : partObj.order - passivesAdded + "°";
                    HornBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Tail:
                    TailBodyPartOrderText.text = isPassive ? "P" : partObj.order - passivesAdded + "°";
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

        AbilityNameText.text = bodyPartToSelect.name;
        AxieBodyPart ability = skillList.axieBodyParts
            .Single(x =>
                x.bodyPart == part && bodyPartToSelect.partClass == x.bodyPartClass &&
                x.skillName == bodyPartToSelect.SkillName);

        AbilityDescriptionText.text = ability.description;
        // Set the selected part as the last orde
        bodyPartToSelect.selected = true;
        bodyPartToSelect.order = maxOrder + 1;



        switch (bodyPartToSelect.BodyPart)
        {
            case BodyPart.Back:
                BackBodyPartOrderText.text = ability.isPassive ? "P" : bodyPartToSelect.order - passivesAdded + "°";
                BackBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Mouth:
                MouthBodyPartOrderText.text = ability.isPassive ? "P" : bodyPartToSelect.order - passivesAdded + "°";
                MouthBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Horn:
                HornBodyPartOrderText.text = ability.isPassive ? "P" : bodyPartToSelect.order - passivesAdded + "°";
                HornBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Tail:
                TailBodyPartOrderText.text = ability.isPassive ? "P" : bodyPartToSelect.order - passivesAdded + "°";
                TailBodyPartOrderImage.SetActive(true);
                break;
        }

        if (ability.isPassive)
        {
            AttackAbilityText.transform.parent.gameObject.SetActive(false);
            ShieldAbilityText.transform.parent.gameObject.SetActive(false);

            EnergyObject.SetActive(false);
            PassiveGO.SetActive(true);
        }
        else
        {
            EnergyText.text = ability.energy.ToString();
            EnergyObject.SetActive(true);
            PassiveGO.SetActive(false);
            AttackAbilityText.transform.parent.gameObject.SetActive(true);
            ShieldAbilityText.transform.parent.gameObject.SetActive(true);
        }

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

        AbilityDescriptionTooltip.SetTooltips(ability.tooltipTypes);
    }

    public void ChoosePartOnlyDo()
    {
        // Deactivate all body part order images initially
        HornBodyPartOrderImage.SetActive(false);
        BackBodyPartOrderImage.SetActive(false);
        MouthBodyPartOrderImage.SetActive(false);
        TailBodyPartOrderImage.SetActive(false);

        int passivesAdded = 0;

        // Iterate over selected parts
        foreach (var partObj in currentSelectedAxie.parts.Where(x => x.selected).OrderBy(x => x.order))
        {
            // Check if the part is a passive
            bool isPassive = skillList.axieBodyParts
                .FirstOrDefault(x => x.skillName == partObj.SkillName && x.bodyPart == partObj.BodyPart).isPassive;

            // Adjust the order based on passives logic
            string displayOrder = isPassive ? "P" : (partObj.order - passivesAdded + "°");

            // Activate and update the UI elements according to the body part type
            switch (partObj.BodyPart)
            {
                case BodyPart.Back:
                    BackBodyPartOrderText.text = displayOrder == "0°" ? "1°" : displayOrder;
                    BackBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Mouth:
                    MouthBodyPartOrderText.text = displayOrder == "0°" ? "1°" : displayOrder;
                    MouthBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Horn:
                    HornBodyPartOrderText.text = displayOrder == "0°" ? "1°" : displayOrder;
                    HornBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Tail:
                    TailBodyPartOrderText.text = displayOrder == "0°" ? "1°" : displayOrder;
                    TailBodyPartOrderImage.SetActive(true);
                    break;
            }

            // Increment passives count if current part is passive
            if (isPassive) passivesAdded++;

            // Update ability details
            AbilityNameText.text = partObj.name;
            AxieBodyPart ability = skillList.axieBodyParts.Single(x =>
                x.bodyPart == partObj.BodyPart && partObj.partClass == x.bodyPartClass &&
                x.skillName == partObj.SkillName);
            AbilityDescriptionText.text = ability.description;
            ShieldAbilityText.text = ability.shield.ToString();
            AttackAbilityText.text = ability.damage.ToString();

            // Update ability state based on passive
            if (ability.isPassive)
            {
                AttackAbilityText.transform.parent.gameObject.SetActive(false);
                ShieldAbilityText.transform.parent.gameObject.SetActive(false);
                EnergyObject.SetActive(false);
                PassiveGO.SetActive(true);
            }
            else
            {
                EnergyText.text = ability.energy.ToString();
                EnergyObject.SetActive(true);
                PassiveGO.SetActive(false);
                AttackAbilityText.transform.parent.gameObject.SetActive(true);
                ShieldAbilityText.transform.parent.gameObject.SetActive(true);
            }
            AbilityDescriptionTooltip.SetTooltips(ability.tooltipTypes);
        }

        // Sort and update the button sprites for the body parts
        var AxieSelecteds = currentSelectedAxie.parts.Where(x => x.selected).OrderBy(x => x.order).ToList();
        ButtonMouthBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonBackBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonHornBodyPart.GetComponent<Image>().sprite = DeselectedSprite;
        ButtonTailBodyPart.GetComponent<Image>().sprite = DeselectedSprite;

        if (AxieSelecteds.Any(x => x.BodyPart == BodyPart.Mouth))
            ButtonMouthBodyPart.GetComponent<Image>().sprite = SelectedSprite;
        if (AxieSelecteds.Any(x => x.BodyPart == BodyPart.Back))
            ButtonBackBodyPart.GetComponent<Image>().sprite = SelectedSprite;
        if (AxieSelecteds.Any(x => x.BodyPart == BodyPart.Tail))
            ButtonTailBodyPart.GetComponent<Image>().sprite = SelectedSprite;
        if (AxieSelecteds.Any(x => x.BodyPart == BodyPart.Horn))
            ButtonHornBodyPart.GetComponent<Image>().sprite = SelectedSprite;


    }


    public void SelectAxie(string axieId, Transform parent)
    {
        foreach (var skeletonGraphic in TeamGraphics)
        {
            skeletonGraphic.transform.parent.GetComponent<Image>().sprite = DeselectedSprite;
        }

        parent.GetComponent<Image>().sprite = SelectedSprite;

        SFXManager.instance.PlaySFX(SFXType.UIButtonTap, 0.12f, true);

        GetAxiesExample.Axie axie = AccountManager.userAxies.results.Single(x => x.id == axieId);

        if (RunManagerSingleton.instance.goodTeam.GetCharactersAll().Count > 0)
        {
            AxieController axieFromTeam = RunManagerSingleton.instance.goodTeam.GetCharactersAll().Single(x => x.AxieId.ToString() == axieId);
            HealthText.text = axieFromTeam.stats.hp.ToString();
            SpeedText.text = axieFromTeam.stats.speed.ToString();
            SkillText.text = axieFromTeam.stats.skill.ToString();
            MoraleText.text = axieFromTeam.stats.morale.ToString();
        }
        else
        {
            HealthText.text = axie.stats.hp.ToString();
            SpeedText.text = axie.stats.speed.ToString();
            SkillText.text = axie.stats.skill.ToString();
            MoraleText.text = axie.stats.morale.ToString();
        }
        axieClassImage.sprite = AxieClassGraphics.Single(x => x.axieClass == axie.axieClass).axieClassSprite;

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