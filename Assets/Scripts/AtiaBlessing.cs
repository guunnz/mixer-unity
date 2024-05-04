using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

[System.Serializable]
public class BlessingAugument
{
    public AtiaBlessing.Blessing blessing;
    public AxieClass axieClass;
}

public class AtiaBlessing : MonoBehaviour
{
    public enum Blessing
    {
        Increase_HP,
        Increase_Morale,
        Increase_Speed,
        Increase_Skill
    }

    public Team goodTeam;

    public TextMeshProUGUI FirstAugumentText;
    public TextMeshProUGUI SecondAugumentText;
    public TextMeshProUGUI ThirdAugumentText;

    public UnityEngine.UI.Button FirstAugument;
    public UnityEngine.UI.Button SecondAugument;
    public UnityEngine.UI.Button ThirdAugument;

    private List<BlessingAugument> blessingAugument = new List<BlessingAugument>();

    public GameObject AugumentSelect;

    private void Awake()
    {
        for (int i = 0; i <= (int)AxieClass.Dusk; i++)
        {
            for (int y = 0; y <= (int)Blessing.Increase_Skill; y++)
            {
                blessingAugument.Add(new BlessingAugument() { axieClass = (AxieClass)i, blessing = (Blessing)y });
            }
        }
    }

    public void ShowRandomAuguments()
    {
        AugumentSelect.SetActive(true);
        List<AxieController> axieControllers = goodTeam.GetCharactersAll();
        List<BlessingAugument> blessingAuguments = new List<BlessingAugument>();

        blessingAuguments.AddRange(blessingAugument);

        BlessingAugument augument1 = blessingAuguments[Random.Range(0, blessingAuguments.Count)];
        blessingAuguments.Remove(augument1);
        BlessingAugument augument2 = blessingAuguments[Random.Range(0, blessingAuguments.Count)];
        blessingAuguments.Remove(augument2);
        BlessingAugument augument3 = blessingAuguments[Random.Range(0, blessingAuguments.Count)];
        blessingAuguments.Remove(augument3);
        while (axieControllers.Count(x => x.axieIngameStats.axieClass == augument1.axieClass) == 0)
        {
            augument1 = blessingAuguments[Random.Range(0, blessingAuguments.Count)];
        }

        blessingAuguments.Remove(augument2);


        while (axieControllers.Count(x => x.axieIngameStats.axieClass == augument2.axieClass) == 0)
        {
            augument2 = blessingAuguments[Random.Range(0, blessingAuguments.Count)];
        }

        blessingAuguments.Remove(augument2);


        while (axieControllers.Count(x => x.axieIngameStats.axieClass == augument3.axieClass) == 0)
        {
            augument3 = blessingAuguments[Random.Range(0, blessingAuguments.Count)];
        }

        FirstAugumentText.text = "Increase your " + augument1.axieClass.ToString() + " axies " +
                                 augument1.blessing.ToString().Replace("_", " ") + " stat by 10";
        SecondAugumentText.text = "Increase your " + augument2.axieClass.ToString() + " axies " +
                                  augument2.blessing.ToString().Replace("_", " ") + " stat by 10";
        ThirdAugumentText.text = "Increase your " + augument3.axieClass.ToString() + " axies " +
                                 augument3.blessing.ToString().Replace("_", " ") + " stat by 10";

        FirstAugument.onClick.RemoveAllListeners();
        SecondAugument.onClick.RemoveAllListeners();
        ThirdAugument.onClick.RemoveAllListeners();

        FirstAugument.onClick.AddListener(delegate { AugumentUpgrade(blessingAugument.IndexOf(augument1)); });
        SecondAugument.onClick.AddListener(delegate { AugumentUpgrade(blessingAugument.IndexOf(augument2)); });
        ThirdAugument.onClick.AddListener(delegate { AugumentUpgrade(blessingAugument.IndexOf(augument3)); });
    }

    public void AugumentUpgrade(int indexAugument)
    {
        AugumentSelect.SetActive(false);
        BlessingAugument augument = blessingAugument[indexAugument];

        List<AxieController> axieControllers = goodTeam.GetCharactersAll()
            .Where(x => x.axieIngameStats.axieClass == augument.axieClass).ToList();

        foreach (var controller in axieControllers)
        {
            switch (augument.blessing)
            {
                case Blessing.Increase_HP:
                    controller.stats.hp += 10;
                    break;
                case Blessing.Increase_Morale:
                    controller.stats.morale += 10;
                    break;
                case Blessing.Increase_Speed:
                    controller.stats.speed += 10;
                    break;
                case Blessing.Increase_Skill:
                    controller.stats.skill += 10;
                    break;
            }
        }
    }
}