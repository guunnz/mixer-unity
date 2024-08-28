using DG.Tweening;
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

    public int FreeRerollEveryXRolls = 0;
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
    public int coins = 10;
    public Color lifeLostColor;
    public LandType landType;
    internal List<bool> resultsBools = new List<bool>();
    internal string currentOpponent = "";
    internal int netWorth = 0;
    internal int eNetWorth = 0;

    internal List<UpgradeValuesPerRoundList> globalUpgrades = new List<UpgradeValuesPerRoundList>()
    {
        new UpgradeValuesPerRoundList()
            { team_upgrades_values_per_round = new List<UpgradeAugument>() }
    };

    public EconomyPassive economyPassive = new EconomyPassive();
    public RoundsPassives roundsPassives = new RoundsPassives();
    public int score => losses + wins;

    public TextMeshProUGUI results;
    public TextMeshProUGUI coinsText;
    public Image[] lives;
    public Team goodTeam;
    public AtiaBlessing atiaBlessing;
    public RectTransform ScoreRect;
    private float skere;
    private int MaxCoinsThisRound = 10;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    public void GenesisLandBehavior()
    {
        coins += coins / 3;
    }
    public void SavannahLandBehavior()
    {
        coins += 3;
    }
    public void SetResult(bool won)
    {
        skere = 0;
        eNetWorth = 0;
        resultsBools.Add(won);
        atiaBlessing.ShowRandomAuguments();

        if (landType == LandType.genesis)
        {
            GenesisLandBehavior();
        }
        else if (landType == LandType.savannah)
        {
            SavannahLandBehavior();
        }

        coins += economyPassive.CoinsOnStart + economyPassive.ExtraCoinsPerRound;
        MaxCoinsThisRound = coins;
        economyPassive.RollsThisRound = 0;
        coinsText.text = coins.ToString();
        if (won)
        {
            if (roundsPassives.ExtraTeamHPPerRound != 0)
            {
                var axies = goodTeam.GetCharactersAll();
                axies.ForEach(axie => { axie.stats.hp += roundsPassives.ExtraTeamHPPerRound; });
            }

            if (wins >= 12)
            {
                MusicManager.Instance.PlayMusic(MusicTrack.Tululu);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
        }
        else
        {
            if (losses >= 3)
            {
                MusicManager.Instance.PlayMusic(MusicTrack.Tululu);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                return;
            }
        }
        ShopManager.instance.SetShop();
    }

    private void SetScoreGraphics()
    {
        results.text = wins.ToString();

        for (int i = 0; i < losses; i++)
        {
            lives[i].DOColor(lifeLostColor, 1f);
        }

    }

    public void SetResultUI(bool won)
    {
        if (won)
        {
            wins++;
        }
        else
        {
            losses++;
        }

        RunManagerSingleton.instance.MoveAndResize(new Vector2(-30, 150), new Vector2(2, 2), 1, 1);
    }

    public void MoveAndResize(Vector2 targetPosition, Vector2 targetSize, float duration, float waitTime)
    {
        // Store the original position and size
        Vector2 originalPosition = ScoreRect.anchoredPosition;
        Vector2 originalSize = ScoreRect.localScale;

        // Create a sequence to chain animations
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(waitTime);
        // Move and resize to target position and size
        sequence.Append(ScoreRect.DOAnchorPos(targetPosition, duration));
        sequence.Join(ScoreRect.DOScale(targetSize, duration));

        // Perform the action during the wait time
        sequence.AppendInterval(waitTime / 2); // Half the wait time before the action
        sequence.AppendCallback(() => SetScoreGraphics());
        sequence.AppendInterval(waitTime / 2); // Half the wait time after the action

        // Move and resize back to original position and size
        sequence.Append(ScoreRect.DOAnchorPos(originalPosition, duration/4));
        sequence.Join(ScoreRect.DOScale(originalSize, duration/4));

        // Start the sequence
        sequence.Play();
    }

    public void RemoveCoins(int coinsAmount)
    {
        skere += coinsAmount * 1.3245f;

        coins -= coinsAmount;

        coinsText.text = coins.ToString();
    }

    public bool BuyUpgrade(ShopItem upgrade, bool PlayBuyAudio = true)
    {
        int price = (int)Math.Floor(upgrade.price * RunManagerSingleton.instance.economyPassive.ItemCostPercentage /
                                    100f);
        netWorth += upgrade.price;
        if (coins < price)
            return false;

        if ((int)(Math.Round(skere / 1.3245f, 5) + coins) != MaxCoinsThisRound)
        {
            Debug.LogError("CHEATING");
        }
        if (PlayBuyAudio)
            SFXManager.instance.PlaySFX(SFXType.Buy);

        if (globalUpgrades.Count <= score)
        {
            globalUpgrades.Add(new UpgradeValuesPerRoundList()
            { team_upgrades_values_per_round = new List<UpgradeAugument>() });
        }

        globalUpgrades[score].team_upgrades_values_per_round
            .Add(new UpgradeAugument() { id = (int)upgrade.ItemEffectName });

        RemoveCoins((int)Math.Floor(upgrade.price * RunManagerSingleton.instance.economyPassive.ItemCostPercentage /
                                    100f));

        BuffsManager.instance.DoUpgrade(upgrade.ItemEffectName, goodTeam);

        foreach (var axieController in goodTeam.GetCharactersAll())
        {
            axieController.UpdateStats();
        }

        return true;
    }
}