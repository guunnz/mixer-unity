using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum Upgrades
{
    HP,
    Skill,
    Speed,
    Morale
}

public class RunManagerSingleton : MonoBehaviour
{
    static public RunManagerSingleton instance;

    public int wins;
    public int losses;
    public int coins = 3;
    public Color lifeLostColor;

    public TextMeshProUGUI results;
    public TextMeshProUGUI coinsText;
    public Image[] lives;
    public Team goodTeam;

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
        coins += 3;
        coinsText.text = coins.ToString();
        if (won)
        {
            wins++;
            losses++;
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
        switch ((Upgrades)upgrade)
        {
            case Upgrades.Speed:
                foreach (var axieController in goodTeam.GetCharactersAll())
                {
                    axieController.stats.speed += 10;
                    axieController.UpdateStats();
                }

                break;
            case Upgrades.Skill:
                foreach (var axieController in goodTeam.GetCharactersAll())
                {
                    axieController.stats.skill += 10;
                    axieController.UpdateStats();
                }

                break;
            case Upgrades.HP:
                foreach (var axieController in goodTeam.GetCharactersAll())
                {
                    axieController.stats.hp += 10;
                    axieController.UpdateStats();
                }

                break;
            case Upgrades.Morale:
                foreach (var axieController in goodTeam.GetCharactersAll())
                {
                    axieController.stats.morale += 10;
                    axieController.UpdateStats();
                }

                break;
        }
    }
}