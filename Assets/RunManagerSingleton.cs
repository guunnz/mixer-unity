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
    public AtiaBlessing.BuffEffect upgrade;
}

public class EconomyPassive
{
    public int ExtraCoinsPerRound = 0;
    public int RollsFreePerRound = 0;
    public int RollsThisRound = 0;
    public int RollCost = 1;
    public int CoinsOnStart = 10;

    public int
        ItemCostPercentage =
            100; //If this 50 for example. means that every item is 50% discount. if it is 70, means every item is 30% discount

    public int FreeRerollEveryXRounds = 0;
}

public class RoundsPassives
{
    public int ExtraTeamHPPerRound = 0;
}


public class RunManagerSingleton : MonoBehaviour
{
    static public RunManagerSingleton instance;

    public string runId = "";
    public string userId;
    public int wins;
    public int losses;
    public int coins = 3;
    public Color lifeLostColor;
    public LandType landType;
    internal List<bool> resultsBools = new List<bool>();
    internal string currentOpponent = "";
    internal List<UpgradeValuesPerRoundList> globalUpgrades = new List<UpgradeValuesPerRoundList>();
    internal List<AxieUpgrade> axieUpgrades = new List<AxieUpgrade>();

    public EconomyPassive economyPassive = new EconomyPassive();
    public RoundsPassives roundsPassives = new RoundsPassives();
    public int score => losses + wins;

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
        coins += economyPassive.CoinsOnStart + economyPassive.ExtraCoinsPerRound;
        economyPassive.RollsThisRound = 0;
        coinsText.text = coins.ToString();
        if (won)
        {
            if (roundsPassives.ExtraTeamHPPerRound != 0)
            {
                var axies = goodTeam.GetCharactersAll();
                axies.ForEach(axie => { axie.stats.hp += roundsPassives.ExtraTeamHPPerRound; });
            }

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

        ShopManager.instance.SetShop();
    }

    public void RemoveCoins(int coinsAmount)
    {
        coins -= coinsAmount;

        coinsText.text = coins.ToString();
    }

    public bool BuyUpgrade(ShopItem upgrade)
    {
        if (coins < upgrade.price)
            return false;

        if (globalUpgrades.Count <= score)
        {
            globalUpgrades.Add(new UpgradeValuesPerRoundList()
                { team_upgrades_values_per_round = new List<UpgradeAugument>() });
        }

        globalUpgrades[score].team_upgrades_values_per_round
            .Add(new UpgradeAugument() { id = (int)upgrade.ItemEffectName });

        ShopItemsManager.instance.DoUpgrade(upgrade.ItemEffectName, goodTeam);
        return true;
    }
}