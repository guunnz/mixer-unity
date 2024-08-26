using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static GetAxiesExample;

public class SelectedComboData
{
    public bool passive;
    public string name;
    public string description;
    public string damage;
    public string shield;
    public string energy;
    public TooltipType[] tooltips;
}

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
    private SelectedComboData data = new SelectedComboData();
    public AbilityDescriptionTooltip AbilityDescriptionTooltip;
    public Button ButtonHornBodyPart;
    public Button ButtonMouthBodyPart;
    public Button ButtonBackBodyPart;
    public Button ButtonTailBodyPart;

    public GameObject HornBodyPartOrderImage;
    public GameObject MouthBodyPartOrderImage;
    public GameObject BackBodyPartOrderImage;
    public GameObject TailBodyPartOrderImage;
    public GameObject PassiveGO;
    public TextMeshProUGUI HornBodyPartOrderText;
    public TextMeshProUGUI MouthBodyPartOrderText;
    public TextMeshProUGUI BackBodyPartOrderText;
    public TextMeshProUGUI TailBodyPartOrderText;
    public TextMeshProUGUI EnergyText;
    public GameObject EnergyObject;
    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI SpeedText;
    public TextMeshProUGUI MoraleText;
    public TextMeshProUGUI SkillText;
    public FakeAxiesManager axiesManager;
    private GetAxiesExample.Axie currentSelectedAxie;
    public Sprite SelectedSprite;
    public Sprite DeselectedSprite;

    public void SetUI()
    {
        PassiveGO.SetActive(data.passive);
        AttackAbilityText.transform.parent.gameObject.SetActive(!data.passive);
        ShieldAbilityText.transform.parent.gameObject.SetActive(!data.passive);
        EnergyObject.SetActive(!data.passive);
        EnergyText.text = data.energy;
        AbilityNameText.text = data.name;
        AbilityDescriptionText.text = data.description;
        ShieldAbilityText.text = data.shield;
        AttackAbilityText.text = data.damage;


        AbilityDescriptionTooltip.SetTooltips(data.tooltips);
    }

    public void SetUI(SelectedComboData data)
    {
        PassiveGO.SetActive(data.passive);
        AttackAbilityText.transform.parent.gameObject.SetActive(!data.passive);
        ShieldAbilityText.transform.parent.gameObject.SetActive(!data.passive);
        EnergyObject.SetActive(!data.passive);
        EnergyText.text = data.energy;
        AbilityNameText.text = data.name;
        AbilityDescriptionText.text = data.description;
        ShieldAbilityText.text = data.shield;
        AttackAbilityText.text = data.damage;

        AbilityDescriptionTooltip.SetTooltips(data.tooltips);
    }

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

    public void BodyPartHover(BodyPart part)
    {
        GetAxiesExample.Part bodyPartToSelect =
            currentSelectedAxie.parts.Single(x => x.BodyPart == part);
        AxieBodyPart ability = skillList.axieBodyParts
    .Single(x =>
        x.bodyPart == part && bodyPartToSelect.partClass == x.bodyPartClass &&
        x.skillName == bodyPartToSelect.SkillName);

        SelectedComboData data = new SelectedComboData();
        data.name = bodyPartToSelect.name;
        data.description = ability.description;
        data.energy = ability.energy.ToString();
        data.damage = ability.damage.ToString();
        data.shield = ability.shield.ToString();
        data.passive = ability.isPassive;
        data.tooltips = ability.tooltipTypes;
        SetUI(data);
    }

    public void BodyPartStopHover()
    {
        SetUI();
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
                    ButtonBackBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    BackBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Mouth:
                    MouthBodyPartOrderText.text = isPassive ? "P" : partObj.order - passivesAdded + "°";
                    ButtonMouthBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    MouthBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Horn:
                    HornBodyPartOrderText.text = isPassive ? "P" : partObj.order - passivesAdded + "°";
                    ButtonHornBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    HornBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Tail:
                    TailBodyPartOrderText.text = isPassive ? "P" : partObj.order - passivesAdded + "°";
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

        AbilityNameText.text = bodyPartToSelect.name;
        AxieBodyPart ability = skillList.axieBodyParts
            .Single(x =>
                x.bodyPart == part && bodyPartToSelect.partClass == x.bodyPartClass &&
                x.skillName == bodyPartToSelect.SkillName);
        AbilityDescriptionText.text = ability.description;

        switch (bodyPartToSelect.BodyPart)
        {
            case BodyPart.Back:
                BackBodyPartOrderText.text = ability.isPassive ? "P" : bodyPartToSelect.order - passivesAdded + "°";
                ButtonBackBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                BackBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Mouth:
                MouthBodyPartOrderText.text = ability.isPassive ? "P" : bodyPartToSelect.order - passivesAdded + "°";
                ButtonMouthBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                MouthBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Horn:
                HornBodyPartOrderText.text = ability.isPassive ? "P" : bodyPartToSelect.order - passivesAdded + "°";
                ButtonHornBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                HornBodyPartOrderImage.SetActive(true);
                break;
            case BodyPart.Tail:
                TailBodyPartOrderText.text = ability.isPassive ? "P" : bodyPartToSelect.order - passivesAdded + "°";
                ButtonTailBodyPart.GetComponent<Image>().sprite = SelectedSprite;
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

        AbilityDescriptionTooltip.SetTooltips(ability.tooltipTypes);
        ShieldAbilityText.text = ability.shield.ToString();
        AttackAbilityText.text = ability.damage.ToString();

        data.description = ability.description;
        data.energy = ability.energy.ToString();
        data.damage = ability.damage.ToString();
        data.shield = ability.shield.ToString();
        data.passive = ability.isPassive;
        data.tooltips = ability.tooltipTypes;
        data.name = bodyPartToSelect.name;
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
        int passivesAdded = 0;

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
                    ButtonBackBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    BackBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Mouth:
                    MouthBodyPartOrderText.text = displayOrder == "0°" ? "1°" : displayOrder;
                    ButtonMouthBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    MouthBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Horn:
                    HornBodyPartOrderText.text = displayOrder == "0°" ? "1°" : displayOrder;
                    ButtonHornBodyPart.GetComponent<Image>().sprite = SelectedSprite;
                    HornBodyPartOrderImage.SetActive(true);
                    break;
                case BodyPart.Tail:
                    TailBodyPartOrderText.text = displayOrder == "0°" ? "1°" : displayOrder;
                    ButtonTailBodyPart.GetComponent<Image>().sprite = SelectedSprite;
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
            data.description = ability.description;
            data.energy = ability.energy.ToString();
            data.damage = ability.damage.ToString();
            data.shield = ability.shield.ToString();
            data.passive = ability.isPassive;
            data.tooltips = ability.tooltipTypes;
            data.name = partObj.name;
            AbilityDescriptionTooltip.SetTooltips(ability.tooltipTypes);
        }
    }

    public void SelectAxie(string axieId, Transform parent)
    {
        foreach (var skeletonGraphic in TeamGraphics)
        {
            skeletonGraphic.transform.parent.GetComponent<Image>().sprite = DeselectedSprite;
        }
        SFXManager.instance.PlaySFX(SFXType.UIButtonTap, 0.12f, true);
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