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
    public int upgrade_id;
    public List<int> axieClass;
}

public class AtiaBlessing : MonoBehaviour
{
    public enum Blessing
    {
        Increase_HP_Bug = 0,
        Increase_HP = 1,
        Increase_Morale,
        Increase_Speed,
        Increase_Skill,
        Backdoor,
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
            for (int y = 1; y <= (int)Blessing.Backdoor; y++)
            {
                blessingAugument.Add(new UpgradeAugument()
                    { axieClass = new List<int>() { i }, upgrade_id = y });
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

                    if (augument.upgrade_id != (int)Blessing.Backdoor)
                    {
                        FirstAugumentText.text =
                            "Increase your " + ((AxieClass)augument.axieClass[0]).ToString() + " axies " +
                            ((Blessing)augument.upgrade_id).ToString().Replace("_", " ") +
                            " stat by 1";
                    }
                    else
                    {
                        FirstAugumentText.text = "Your " + ((AxieClass)augument.axieClass[0]).ToString() +
                                                 " axies now Backdoor on start";
                    }

                    break;
                case 2:
                    if (rollSecond < 0)
                    {
                        rollSecond = 0;
                        return;
                    }

                    if (augument.upgrade_id != (int)Blessing.Backdoor)
                    {
                        SecondAugumentText.text =
                            "Increase your " + ((AxieClass)augument.axieClass[0]).ToString() + " axies " +
                            ((Blessing)augument.upgrade_id).ToString().Replace("_", " ") + " stat by 1";
                    }
                    else
                    {
                        SecondAugumentText.text = "Your " + ((AxieClass)augument.axieClass[0]).ToString() +
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

                    if (augument.upgrade_id != (int)Blessing.Backdoor)
                    {
                        ThirdAugumentText.text =
                            "Increase your " + ((AxieClass)augument.axieClass[0]).ToString() + " axies " +
                            ((Blessing)augument.upgrade_id).ToString().Replace("_", " ") + " stat by 1";
                    }
                    else
                    {
                        ThirdAugumentText.text = "Your " + ((AxieClass)augument.axieClass[0]).ToString() +
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

            if (augument1.upgrade_id != (int)Blessing.Backdoor)
            {
                FirstAugumentText.text = "Increase your " + ((AxieClass)augument1.axieClass[0]).ToString() + " axies " +
                                         ((Blessing)augument1.upgrade_id).ToString().Replace("_", " ") + " stat by 1";
            }
            else
            {
                FirstAugumentText.text = "Your " + ((AxieClass)augument1.axieClass[0]).ToString() +
                                         " axies now Backdoor on start";
            }

            if (augument2.upgrade_id != (int)Blessing.Backdoor)
            {
                SecondAugumentText.text = "Increase your " + ((AxieClass)augument2.axieClass[0]).ToString() +
                                          " axies " +
                                          ((Blessing)augument2.upgrade_id).ToString().Replace("_", " ") + " stat by 1";
            }
            else
            {
                SecondAugumentText.text = "Your " + ((AxieClass)augument2.axieClass[0]).ToString() +
                                          " axies now Backdoor on start";
            }

            if (augument3.upgrade_id != (int)Blessing.Backdoor)
            {
                ThirdAugumentText.text = "Increase your " + ((AxieClass)augument3.axieClass[0]).ToString() + " axies " +
                                         ((Blessing)augument3.upgrade_id).ToString().Replace("_", " ") + " stat by 1";
            }
            else
            {
                ThirdAugumentText.text = "Your " + ((AxieClass)augument3.axieClass[0]).ToString() +
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

    public void AugumentUpgrade(int indexAugument, string axieId, Team team)
    {
        UpgradeAugument augument = blessingAugument[indexAugument];

        AxieController axieControllers = team.GetCharactersAll()
            .Single(x => x.AxieId.ToString() == axieId);

        if (RunManagerSingleton.instance.globalUpgrades.Count <= RunManagerSingleton.instance.score)
        {
            RunManagerSingleton.instance.globalUpgrades.Add(new UpgradeValuesPerRound()
                { upgrade_values_per_round = new List<UpgradeAugument>() });
        }

        RunManagerSingleton.instance.globalUpgrades[RunManagerSingleton.instance.score].upgrade_values_per_round
            .Add(new UpgradeAugument() { upgrade_id = augument.upgrade_id, axieClass = augument.axieClass });

        switch ((Blessing)augument.upgrade_id)
        {
            case AtiaBlessing.Blessing.Increase_HP:
                axieControllers.stats.hp += 1;
                break;
            case AtiaBlessing.Blessing.Increase_Morale:
                axieControllers.stats.morale += 1;
                break;
            case AtiaBlessing.Blessing.Increase_Speed:
                axieControllers.stats.speed += 1;
                break;
            case AtiaBlessing.Blessing.Increase_Skill:
                axieControllers.stats.skill += 1;
                break;
            case AtiaBlessing.Blessing.Backdoor:
                axieControllers.ShrimpOnStart = true;
                break;
        }
    }

    public void AugumentUpgrade(int indexAugument, List<AxieClass> axieClasses, Team team)
    {
        UpgradeAugument augument = blessingAugument[indexAugument];
        if (RunManagerSingleton.instance.globalUpgrades.Count <= RunManagerSingleton.instance.score)
        {
            RunManagerSingleton.instance.globalUpgrades.Add(new UpgradeValuesPerRound()
                { upgrade_values_per_round = new List<UpgradeAugument>() });
        }

        RunManagerSingleton.instance.globalUpgrades[RunManagerSingleton.instance.score].upgrade_values_per_round
            .Add(new UpgradeAugument() { upgrade_id = augument.upgrade_id, axieClass = augument.axieClass });
        List<AxieController> axieControllers = axieClasses == null
            ? team.GetCharactersAll()
            : team.GetCharactersAll()
                .Where(x => augument.axieClass.Select(x => (AxieClass)x).Contains(x.axieIngameStats.axieClass))
                .ToList();


        foreach (var controller in axieControllers)
        {
            switch ((Blessing)augument.upgrade_id)
            {
                case AtiaBlessing.Blessing.Increase_HP:
                    controller.stats.hp += 1;
                    break;
                case AtiaBlessing.Blessing.Increase_Morale:
                    controller.stats.morale += 1;
                    break;
                case AtiaBlessing.Blessing.Increase_Speed:
                    controller.stats.speed += 1;
                    break;
                case AtiaBlessing.Blessing.Increase_Skill:
                    controller.stats.skill += 1;
                    break;
                case AtiaBlessing.Blessing.Backdoor:
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
            RunManagerSingleton.instance.globalUpgrades.Add(new UpgradeValuesPerRound()
                { upgrade_values_per_round = new List<UpgradeAugument>() });
        }

        RunManagerSingleton.instance.globalUpgrades[RunManagerSingleton.instance.score].upgrade_values_per_round
            .Add(new UpgradeAugument() { upgrade_id = augument.upgrade_id, axieClass = augument.axieClass });
        List<AxieController> axieControllers = augument.axieClass == null
            ? team.GetCharactersAll()
            : team.GetCharactersAll()
                .Where(x => augument.axieClass.Select(x => (AxieClass)x).Contains(x.axieIngameStats.axieClass))
                .ToList();

        foreach (var controller in axieControllers)
        {
            switch ((Blessing)augument.upgrade_id)
            {
                case Blessing.Increase_HP_Bug:
                    controller.stats.hp += 1;
                    break;

                case AtiaBlessing.Blessing.Increase_HP:
                    controller.stats.hp += 1;
                    break;
                case AtiaBlessing.Blessing.Increase_Morale:
                    controller.stats.morale += 1;
                    break;
                case AtiaBlessing.Blessing.Increase_Speed:
                    controller.stats.speed += 1;
                    break;
                case AtiaBlessing.Blessing.Increase_Skill:
                    controller.stats.skill += 1;
                    break;
                case AtiaBlessing.Blessing.Backdoor:
                    controller.ShrimpOnStart = true;
                    break;
            }
        }
    }
}