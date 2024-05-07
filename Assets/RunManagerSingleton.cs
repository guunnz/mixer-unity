using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AxieUpgrade
{
    public string axieId;
    public AtiaBlessing.Blessing upgrade;
}


public class RunManagerSingleton : MonoBehaviour
{
    static public RunManagerSingleton instance;

    public string userId;
    public int wins;
    public int losses;
    public int coins = 3;
    public Color lifeLostColor;
    public LandType landType;
    internal List<bool> resultsBools = new List<bool>();
    internal List<string> opponents = new List<string>();
    internal List<UpgradeAugument> globalUpgrades = new List<UpgradeAugument>();
    internal List<AxieUpgrade> axieUpgrades = new List<AxieUpgrade>();

    public TextMeshProUGUI results;
    public TextMeshProUGUI coinsText;
    public Image[] lives;
    public Team goodTeam;

    public AtiaBlessing atiaBlessing;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    public void SetResult(bool won)
    {
        resultsBools.Add(won);
        atiaBlessing.ShowRandomAuguments();
        coins += 3;
        coinsText.text = coins.ToString();
        if (won)
        {
            wins++;
            if (wins >= 12)
            {
                Debug.Log("YOU WON THE RUN");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
        }
        else
        {
            losses++;
            if (losses >= 3)
            {
                Debug.LogError("YOU LOST THE RUN");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
        }

        results.text = wins.ToString();

        for (int i = 0; i < losses; i++)
        {
            lives[i].color = lifeLostColor;
        }
    }

    public void BuyUpgrade(int upgrade)
    {
        if (coins <= 0)
            return;
        coins--;
        coinsText.text = coins.ToString();
        globalUpgrades.Add(new UpgradeAugument() { upgrade_id = upgrade });
        switch ((AtiaBlessing.Blessing)upgrade)
        {
            case AtiaBlessing.Blessing.Increase_Speed:
                foreach (var axieController in goodTeam.GetCharactersAll())
                {
                    axieController.stats.speed += 10;
                    axieController.UpdateStats();
                }

                break;
            case AtiaBlessing.Blessing.Increase_Skill:
                foreach (var axieController in goodTeam.GetCharactersAll())
                {
                    axieController.stats.skill += 10;
                    axieController.UpdateStats();
                }

                break;
            case AtiaBlessing.Blessing.Increase_HP:
                foreach (var axieController in goodTeam.GetCharactersAll())
                {
                    axieController.stats.hp += 10;
                    axieController.UpdateStats();
                }

                break;
            case AtiaBlessing.Blessing.Increase_Morale:
                foreach (var axieController in goodTeam.GetCharactersAll())
                {
                    axieController.stats.morale += 10;
                    axieController.UpdateStats();
                }

                break;
        }
    }
}