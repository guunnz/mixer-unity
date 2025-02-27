using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[System.Serializable]
public class UpgradeAugument
{
    public int id;
    public List<int> axie_class;
}

public class AtiaBlessing : MonoBehaviour
{
    public enum BuffEffect
    {
        Increase_HP_Bug = 0,
        Increase_HP = 1,
        Increase_Morale,
        Increase_Speed,
        Increase_Skill,
        Backdoor,
        Amethyst,
        CompositeBow,
        DeluxeFriedRice,
        DeluxeRamen,
        Diamond,
        Donut,
        Emerald,
        FruitNVegetablesSalad,
        GoldAmethystNecklace,
        GoldDiamondNecklace,
        GoldEmeraldNecklace,
        GoldRubyNecklace,
        GoldTopazNecklace,
        GoldenBook,
        GoldenSphere,
        GrilledLambChop,
        HoneyCarrotSoup,
        HoneyRoastedChicken,
        IronArmor,
        IronChainmail,
        IronHelmet,
        IronShield,
        KingsBoots,
        LargeEnergyPotion,
        LargeHastePotion,
        LargePotionOfResistance,
        LargePotionOfShield,
        MeatPie,
        MediumHastePotion,
        MediumPotionOfShield,
        MoonSphere,
        PlatinumLantern,
        PumpkinPie,
        Ramen,
        RosyHarp,
        Ruby,
        RubyBook,
        ShellOfBlinding,
        ShellOfBurn,
        ShellOfDistraction,
        ShellOfMediumAreaDamage,
        ShellOfMediumConcentratedDamage,
        ShellOfShock,
        ShellOfSilence,
        ShellOfSlow,
        ShellOfStun,
        SilverAmethystBracelet,
        SilverDiamondBracelet,
        SilverEmeraldBracelet,
        SilverRubyBracelet,
        SilverTopazBracelet,
        SteakNFries,
        SteelArrow,
        SteelHammer,
        SteelSword,
        SteelArmor,
        SteelChainmail,
        SteelBoots,
        SteelHelmet,
        SteelShield,
        ThreeWondersShoes,
        Topaz,
        //AUGUMENTS
        GainHPAqua,
        GainHPBird,
        GainHPDawn,
        GainHPBeast,
        GainHPBug,
        GainHPMech,
        GainHPPlant,
        GainHPDusk,
        GainHPReptile,

        GainSpeedAqua,
        GainSpeedBird,
        GainSpeedDawn,
        GainSpeedBeast,
        GainSpeedBug,
        GainSpeedMech,
        GainSpeedPlant,
        GainSpeedDusk,
        GainSpeedReptile,

        GainSkillAqua,
        GainSkillBird,
        GainSkillDawn,
        GainSkillBeast,
        GainSkillBug,
        GainSkillMech,
        GainSkillPlant,
        GainSkillDusk,
        GainSkillReptile,

        GainMoraleAqua,
        GainMoraleBird,
        GainMoraleDawn,
        GainMoraleBeast,
        GainMoraleBug,
        GainMoraleMech,
        GainMoralePlant,
        GainMoraleDusk,
        GainMoraleReptile,

        MoraleBuffAfterSecondsAqua,
        MoraleBuffAfterSecondsBird,
        MoraleBuffAfterSecondsDawn,
        MoraleBuffAfterSecondsBeast,
        MoraleBuffAfterSecondsBug,
        MoraleBuffAfterSecondsMech,
        MoraleBuffAfterSecondsPlant,
        MoraleBuffAfterSecondsDusk,
        MoraleBuffAfterSecondsReptile,

        SpeedBuffAfterSecondsAqua,
        SpeedBuffAfterSecondsBird,
        SpeedBuffAfterSecondsDawn,
        SpeedBuffAfterSecondsBeast,
        SpeedBuffAfterSecondsBug,
        SpeedBuffAfterSecondsMech,
        SpeedBuffAfterSecondsPlant,
        SpeedBuffAfterSecondsDusk,
        SpeedBuffAfterSecondsReptile,

        AttackBuffAfterSecondsAqua,
        AttackBuffAfterSecondsBird,
        AttackBuffAfterSecondsDawn,
        AttackBuffAfterSecondsBeast,
        AttackBuffAfterSecondsBug,
        AttackBuffAfterSecondsMech,
        AttackBuffAfterSecondsPlant,
        AttackBuffAfterSecondsDusk,
        AttackBuffAfterSecondsReptile,

        AutoAttackHealAqua,
        AutoAttackHealBird,
        AutoAttackHealDawn,
        AutoAttackHealBeast,
        AutoAttackHealBug,
        AutoAttackHealMech,
        AutoAttackHealPlant,
        AutoAttackHealDusk,
        AutoAttackHealReptile,

        ExtraAbilitySlotAqua,
        ExtraAbilitySlotBird,
        ExtraAbilitySlotDawn,
        ExtraAbilitySlotBeast,
        ExtraAbilitySlotBug,
        ExtraAbilitySlotMech,
        ExtraAbilitySlotPlant,
        ExtraAbilitySlotDusk,
        ExtraAbilitySlotReptile,

        BackdoorAqua,
        BackdoorBird,
        BackdoorDawn,
        BackdoorBeast,
        BackdoorBug,
        BackdoorMech,
        BackdoorPlant,
        BackdoorDusk,
        BackdoorReptile,

        PrismaticAqua,
        PrismaticBird,
        PrismaticDawn,
        PrismaticBeast,
        PrismaticBug,
        PrismaticMech,
        PrismaticPlant,
        PrismaticDusk,
        PrismaticReptile,
        AxiePark,
        Savannah,
        Forest,
        Arctic,
        Mystic,
        Genesis,
        LunasLanding,
        //AUGUMENTS STOP
    }

    public Team goodTeam;

    public TextMeshProUGUI FirstAugumentText;
    public TextMeshProUGUI SecondAugumentText;
    public TextMeshProUGUI ThirdAugumentText;

    public TextMeshProUGUI FirstRollNumberText;
    public TextMeshProUGUI SecondRollNumberText;
    public TextMeshProUGUI ThirdRollNumberText;

    public Image FirstAugumentImage;
    public Image SecondAugumentImage;
    public Image ThirdAugumentImage;
    public Image Cover;

    public AtiasBlessingAnimation atiaAnimation;

    public UnityEngine.UI.Button FirstAugument;
    public UnityEngine.UI.Button SecondAugument;
    public UnityEngine.UI.Button ThirdAugument;

    public GameObject RollButton2;
    public GameObject RollButton3;
    public GameObject[] rollButtons;

    public ShopItem[] blessingsList;

    public ShopItem AxieParkBlessing;
    public ShopItem SavannahBlessing;
    public ShopItem ForestBlessing;
    public ShopItem ArcticBlessing;
    public ShopItem MysticBlessing;
    public ShopItem GenesisBlessing;
    public ShopItem LunasLandingBlessing;

    public ShopItem LandBlessing
    {
        get
        {
            switch (RunManagerSingleton.instance.landType)
            {
                case LandType.axiepark:
                    return AxieParkBlessing;
                case LandType.savannah:
                    return SavannahBlessing;
                case LandType.forest:
                    return ForestBlessing;
                case LandType.arctic:
                    return ArcticBlessing;
                case LandType.mystic:
                    return MysticBlessing;
                case LandType.genesis:
                    return GenesisBlessing;
                case LandType.lunalanding:
                    return LunasLandingBlessing;
                default:
                    return null;
            }
        }
    }

    public GameObject AugumentSelect;

    private int rollSecond = 1;
    private int rollThird = 1;

    private ShopItem blessing2;
    private ShopItem blessing3;

    private List<BuffEffect> blessingsSelected = new List<BuffEffect>();
    private List<BuffEffect> blessingsRolledFor = new List<BuffEffect>();

    public void RollAugument(int augument)
    {
        ShowRandomAuguments(true, augument);
    }

    public void ShowRandomAuguments(bool DoOnlyOne = false, int doAugument = 0)
    {
        TooltipManagerSingleton.instance.DisableTooltip();
        if (!DoOnlyOne)
        {
            blessingsRolledFor.Clear();
            Cover.gameObject.SetActive(true);
            atiaAnimation.DoAnim();
            if (RunManagerSingleton.instance.landType != LandType.mystic)
            {
                rollSecond = 2;
                rollThird = 2;
            }
            else
            {
                rollSecond = 4;
                rollThird = 4;
            }
            RollButton2.SetActive(true);
            RollButton3.SetActive(true);
        }

        List<ShopItem> blessings = new List<ShopItem>();
        if (DoOnlyOne)
        {
            switch (doAugument)
            {
                case 2:
                    if (rollSecond < 0)
                    {
                        rollSecond = 0;
                        return;
                    }

                    SecondAugument.onClick.RemoveAllListeners();


                    blessings = blessingsList.ToList();
                    blessings.RemoveAll(x => blessingsSelected.Contains(x.ItemEffectName));

                    blessings.Remove(blessing2);
                    blessings.Remove(blessing3);

                    blessing2 = blessings[Random.Range(0, blessings.Count)];

                    rollSecond--;
                    if (rollSecond == 0)
                    {
                        RollButton2.SetActive(false);
                    }
                    SecondAugument.onClick.AddListener(delegate
                    {
                        AugumentUpgrade((int)blessing2.ItemEffectName, goodTeam);
                    });
                    break;
                case 3:
                    if (rollThird < 0)
                    {
                        rollThird = 0;
                        return;
                    }

                    ThirdAugument.onClick.RemoveAllListeners();

                    blessings = blessingsList.ToList();
                    blessings.RemoveAll(x => blessingsSelected.Contains(x.ItemEffectName));

                    blessings.Remove(blessing2);
                    blessings.Remove(blessing3);

                    blessing3 = blessings[Random.Range(0, blessings.Count)];
                    rollThird--;
                    if (rollThird == 0)
                    {
                        RollButton3.SetActive(false);
                    }
                    ThirdAugument.onClick.AddListener(delegate
                    {
                        AugumentUpgrade((int)blessing3.ItemEffectName, goodTeam);
                    });
                    break;
            }
        }
        else
        {
            if (RunManagerSingleton.instance.landType == LandType.mystic)
            {
                rollSecond = 4;
                rollThird = 4;
            }
            else
            {
                rollSecond = 2;
                rollThird = 2;
            }
            FirstAugumentText.text = LandBlessing.description;
            FirstAugumentImage.sprite = LandBlessing.ShopItemImage;
            if (LandBlessing.ShowValue != ShowValue.none)
            {
                switch (LandBlessing.ShowValue)
                {
                    case ShowValue.coinSpentReroll:

                        break;
                    case ShowValue.unspentBlessingRerollDividedByTwo:
                        FirstAugumentText.text = FirstAugumentText.text.Replace("[SHOW]", Environment.NewLine + "(Coins = " + ((RunManagerSingleton.instance.economyPassive.atiasNotRerolled / 2) + 5).ToString()) + ")";
                        break;
                    case ShowValue.coinsEconomyGenesis:
                        FirstAugumentText.text = FirstAugumentText.text.Replace("[SHOW]", Environment.NewLine + "(Coins = " + (5 + RunManagerSingleton.instance.economyPassive.genesisEconomyGained) + ")");
                        break;
                    case ShowValue.smoothPotions:
                        FirstAugumentText.text = FirstAugumentText.text.Replace("[SHOW]", Environment.NewLine + "(Coins = " + (5 + RunManagerSingleton.instance.economyPassive.smoothPotionsPurchased).ToString()) + ")";
                        break;
                    case ShowValue.rerollsThisGame:
                        FirstAugumentText.text = FirstAugumentText.text.Replace("[SHOW]", Environment.NewLine + "(Coins = " + (5 + ShopManager.instance.reRolls).ToString()) + ")";
                        break;
                }
            }

            blessings = blessingsList.ToList();
            blessings.RemoveAll(x => blessingsSelected.Contains(x.ItemEffectName));
            blessing2 = blessings[Random.Range(0, blessings.Count)];
            blessings.Remove(blessing2);
            blessing3 = blessings[Random.Range(0, blessings.Count)];
            blessings.Remove(blessing3);

            FirstAugument.onClick.RemoveAllListeners();
            SecondAugument.onClick.RemoveAllListeners();
            ThirdAugument.onClick.RemoveAllListeners();

            FirstAugument.onClick.AddListener(delegate
            {
                AugumentUpgrade((int)LandBlessing.ItemEffectName, goodTeam);
            });

            SecondAugument.onClick.AddListener(delegate
            {
                AugumentUpgrade((int)blessing2.ItemEffectName, goodTeam);
            });
            ThirdAugument.onClick.AddListener(delegate
            {
                AugumentUpgrade((int)blessing3.ItemEffectName, goodTeam);
            });
        }




        SecondAugumentText.text = blessing2.description;
        SecondAugumentImage.sprite = blessing2.ShopItemImage;
        SecondRollNumberText.text = rollSecond.ToString();

        ThirdAugumentText.text = blessing3.description;
        ThirdAugumentImage.sprite = blessing3.ShopItemImage;
        ThirdRollNumberText.text = rollThird.ToString();
    }

    public void AugumentUpgrade(int indexAugument, List<AxieClass> axieClasses, Team team)
    {
        try
        {
            RunManagerSingleton.instance.eNetWorth += ShopManager.instance.ItemList.FirstOrDefault(x => x.ItemEffectName == (BuffEffect)indexAugument)?.price ?? 0;
            BuffsManager.instance.DoUpgrade((BuffEffect)indexAugument, team);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }

    }

    public void AugumentUpgrade(int indexAugument, Team team)
    {
        AugumentSelect.SetActive(false);
        Cover.gameObject.SetActive(false);
        InBattleGraphicsManager.Instance.AddUpgradeMe(indexAugument);

        RunManagerSingleton.instance.economyPassive.atiasNotRerolled += (rollSecond + rollThird);

        if (((BuffEffect)indexAugument).ToString().ToLower().Contains("prismatic"))
        {
            blessingsSelected.Add((BuffEffect)indexAugument);
        }

        if (RunManagerSingleton.instance.globalUpgrades.Count <= RunManagerSingleton.instance.score)
        {
            RunManagerSingleton.instance.globalUpgrades.Add(new UpgradeValuesPerRoundList()
            { team_upgrades_values_per_round = new List<UpgradeAugument>() });
        }
        Dictionary<string, string> itemDict = new Dictionary<string, string>();

        itemDict["atia_blessing_id"] = ((BuffEffect)indexAugument).ToString();

        MavisTracking.Instance.TrackAction("atia-blessing", itemDict);
        RunManagerSingleton.instance.globalUpgrades[RunManagerSingleton.instance.score].team_upgrades_values_per_round
            .Add(new UpgradeAugument() { id = indexAugument });

        BuffsManager.instance.DoUpgrade((BuffEffect)indexAugument, team);


    }
}