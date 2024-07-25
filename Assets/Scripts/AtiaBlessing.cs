using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
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
        Topaz
    }

    public Team goodTeam;

    public TextMeshProUGUI FirstAugumentText;
    public TextMeshProUGUI SecondAugumentText;
    public TextMeshProUGUI ThirdAugumentText;

    public UnityEngine.UI.Button FirstAugument;
    public UnityEngine.UI.Button SecondAugument;
    public UnityEngine.UI.Button ThirdAugument;

    public GameObject[] rollButtons;

    public List<UpgradeAugument> blessingAugument = new List<UpgradeAugument>();
    private List<UpgradeAugument> blessingAugumentsPurchased = new List<UpgradeAugument>();

    public GameObject AugumentSelect;

    private int rollFirst = 1;
    private int rollSecond = 1;
    private int rollThird = 1;

    private void Awake()
    {
        for (int i = 0; i <= (int)AxieClass.Dusk; i++)
        {
            for (int y = 1; y <= (int)BuffEffect.Backdoor; y++)
            {
                blessingAugument.Add(new UpgradeAugument()
                    { axie_class = new List<int>() { i }, id = y });
            }
        }
    }

    public void RollAugument(int augument)
    {
        ShowRandomAuguments(true, augument);
    }

    public void ShowRandomAuguments(bool DoOnlyOne = false, int doAugument = 0)
    {
        if (DoOnlyOne)
        {
            List<UpgradeAugument> blessingAuguments = new List<UpgradeAugument>();
            blessingAuguments.AddRange(blessingAugument);
            blessingAuguments.RemoveAll(x => blessingAugumentsPurchased.Contains(x));
            UpgradeAugument augument = blessingAuguments[Random.Range(0, blessingAuguments.Count)];
            blessingAuguments.Remove(augument);
            switch (doAugument)
            {
                case 1:
                    if (rollFirst < 0)
                    {
                        rollFirst = 0;
                        return;
                    }

                    FirstAugument.onClick.RemoveAllListeners();

                    FirstAugument.onClick.AddListener(delegate
                    {
                        AugumentUpgrade(blessingAugument.IndexOf(augument), goodTeam);
                    });

                    if (augument.id != (int)BuffEffect.Backdoor)
                    {
                        FirstAugumentText.text =
                            "Increase your " + ((AxieClass)augument.axie_class[0]).ToString() + " axies " +
                            ((BuffEffect)augument.id).ToString().Replace("_", " ") +
                            " stat by 3";
                    }
                    else
                    {
                        FirstAugumentText.text = "Your " + ((AxieClass)augument.axie_class[0]).ToString() +
                                                 " axies now Backdoor on start";
                    }

                    break;
                case 2:
                    if (rollSecond < 0)
                    {
                        rollSecond = 0;
                        return;
                    }

                    if (augument.id != (int)BuffEffect.Backdoor)
                    {
                        SecondAugumentText.text =
                            "Increase your " + ((AxieClass)augument.axie_class[0]).ToString() + " axies " +
                            ((BuffEffect)augument.id).ToString().Replace("_", " ") + " stat by 3";
                    }
                    else
                    {
                        SecondAugumentText.text = "Your " + ((AxieClass)augument.axie_class[0]).ToString() +
                                                  " axies now Backdoor on start";
                    }

                    SecondAugument.onClick.RemoveAllListeners();


                    SecondAugument.onClick.AddListener(delegate
                    {
                        AugumentUpgrade(blessingAugument.IndexOf(augument), goodTeam);
                    });
                    break;
                case 3:
                    if (rollThird < 0)
                    {
                        rollThird = 0;
                        return;
                    }

                    if (augument.id != (int)BuffEffect.Backdoor)
                    {
                        ThirdAugumentText.text =
                            "Increase your " + ((AxieClass)augument.axie_class[0]).ToString() + " axies " +
                            ((BuffEffect)augument.id).ToString().Replace("_", " ") + " stat by 3";
                    }
                    else
                    {
                        ThirdAugumentText.text = "Your " + ((AxieClass)augument.axie_class[0]).ToString() +
                                                 " axies now Backdoor on start";
                    }

                    ThirdAugument.onClick.RemoveAllListeners();
                    ThirdAugument.onClick.AddListener(delegate
                    {
                        AugumentUpgrade(blessingAugument.IndexOf(augument), goodTeam);
                    });
                    break;
            }
        }
        else
        {
            foreach (var rollButton in rollButtons)
            {
                rollButton.SetActive(true);
            }

            AugumentSelect.SetActive(true);
            List<UpgradeAugument> blessingAuguments = new List<UpgradeAugument>();

            blessingAuguments.AddRange(blessingAugument);
            blessingAuguments.RemoveAll(x => blessingAugumentsPurchased.Contains(x));
            UpgradeAugument augument1 = blessingAuguments[Random.Range(0, blessingAuguments.Count)];
            blessingAuguments.Remove(augument1);
            UpgradeAugument augument2 = blessingAuguments[Random.Range(0, blessingAuguments.Count)];
            blessingAuguments.Remove(augument2);
            UpgradeAugument augument3 = blessingAuguments[Random.Range(0, blessingAuguments.Count)];
            blessingAuguments.Remove(augument3);

            augument1 = blessingAuguments[Random.Range(0, blessingAuguments.Count)];

            blessingAuguments.Remove(augument2);

            augument2 = blessingAuguments[Random.Range(0, blessingAuguments.Count)];

            blessingAuguments.Remove(augument2);

            augument3 = blessingAuguments[Random.Range(0, blessingAuguments.Count)];

            if (augument1.id != (int)BuffEffect.Backdoor)
            {
                FirstAugumentText.text = "Increase your " + ((AxieClass)augument1.axie_class[0]).ToString() + " axies " +
                                         ((BuffEffect)augument1.id).ToString().Replace("_", " ") + " stat by 3";
            }
            else
            {
                FirstAugumentText.text = "Your " + ((AxieClass)augument1.axie_class[0]).ToString() +
                                         " axies now Backdoor on start";
            }

            if (augument2.id != (int)BuffEffect.Backdoor)
            {
                SecondAugumentText.text = "Increase your " + ((AxieClass)augument2.axie_class[0]).ToString() +
                                          " axies " +
                                          ((BuffEffect)augument2.id).ToString().Replace("_", " ") + " stat by 3";
            }
            else
            {
                SecondAugumentText.text = "Your " + ((AxieClass)augument2.axie_class[0]).ToString() +
                                          " axies now Backdoor on start";
            }

            if (augument3.id != (int)BuffEffect.Backdoor)
            {
                ThirdAugumentText.text = "Increase your " + ((AxieClass)augument3.axie_class[0]).ToString() + " axies " +
                                         ((BuffEffect)augument3.id).ToString().Replace("_", " ") + " stat by 3";
            }
            else
            {
                ThirdAugumentText.text = "Your " + ((AxieClass)augument3.axie_class[0]).ToString() +
                                         " axies now Backdoor on start";
            }

            FirstAugument.onClick.RemoveAllListeners();
            SecondAugument.onClick.RemoveAllListeners();
            ThirdAugument.onClick.RemoveAllListeners();

            FirstAugument.onClick.AddListener(delegate
            {
                AugumentUpgrade(blessingAugument.IndexOf(augument1), goodTeam);
            });
            SecondAugument.onClick.AddListener(delegate
            {
                AugumentUpgrade(blessingAugument.IndexOf(augument2), goodTeam);
            });
            ThirdAugument.onClick.AddListener(delegate
            {
                AugumentUpgrade(blessingAugument.IndexOf(augument3), goodTeam);
            });
        }
    }

    public void AugumentUpgrade(int indexAugument, List<AxieClass> axieClasses, Team team)
    {
        UpgradeAugument augument = blessingAugument[indexAugument];

        List<AxieController> axieControllers = axieClasses == null
            ? team.GetCharactersAll()
            : team.GetCharactersAll()
                .Where(x => augument.axie_class.Select(x => (AxieClass)x).Contains(x.axieIngameStats.axieClass))
                .ToList();


        foreach (var controller in axieControllers)
        {
            switch ((BuffEffect)augument.id)
            {
                case AtiaBlessing.BuffEffect.Increase_HP:
                    controller.stats.hp += 3;
                    break;
                case AtiaBlessing.BuffEffect.Increase_Morale:
                    controller.stats.morale += 3;
                    break;
                case AtiaBlessing.BuffEffect.Increase_Speed:
                    controller.stats.speed += 3;
                    break;
                case AtiaBlessing.BuffEffect.Increase_Skill:
                    controller.stats.skill += 3;
                    break;
                case AtiaBlessing.BuffEffect.Backdoor:
                    controller.ShrimpOnStart = true;
                    break;
            }
        }
    }

    public void AugumentUpgrade(int indexAugument, Team team)
    {
        AugumentSelect.SetActive(false);
        UpgradeAugument augument = blessingAugument[indexAugument];
        blessingAugumentsPurchased.Add(augument);
        if (RunManagerSingleton.instance.globalUpgrades.Count <= RunManagerSingleton.instance.score)
        {
            RunManagerSingleton.instance.globalUpgrades.Add(new UpgradeValuesPerRoundList()
                { team_upgrades_values_per_round = new List<UpgradeAugument>() });
        }

        RunManagerSingleton.instance.globalUpgrades[RunManagerSingleton.instance.score].team_upgrades_values_per_round
            .Add(new UpgradeAugument() { id = augument.id, axie_class = augument.axie_class });
        List<AxieController> axieControllers = augument.axie_class == null
            ? team.GetCharactersAll()
            : team.GetCharactersAll()
                .Where(x => augument.axie_class.Select(x => (AxieClass)x).Contains(x.axieIngameStats.axieClass))
                .ToList();

        foreach (var controller in axieControllers)
        {
            switch ((BuffEffect)augument.id)
            {
                case BuffEffect.Increase_HP_Bug:
                    controller.stats.hp += 1;
                    break;

                case AtiaBlessing.BuffEffect.Increase_HP:
                    controller.stats.hp += 1;
                    break;
                case AtiaBlessing.BuffEffect.Increase_Morale:
                    controller.stats.morale += 1;
                    break;
                case AtiaBlessing.BuffEffect.Increase_Speed:
                    controller.stats.speed += 1;
                    break;
                case AtiaBlessing.BuffEffect.Increase_Skill:
                    controller.stats.skill += 1;
                    break;
                case AtiaBlessing.BuffEffect.Backdoor:
                    controller.ShrimpOnStart = true;
                    break;
            }
        }
    }
}