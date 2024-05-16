using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using enemies;
using UnityEngine;
using UnityEngine.Serialization;

public class OpponentTeamManager : MonoBehaviour
{
    static public OpponentTeamManager instance;

    public List<GetAxiesEnemies.AxieEnemy> AxieEnemyList = new List<GetAxiesEnemies.AxieEnemy>();

    [FormerlySerializedAs("enemyTeam")] public Team badTeam;

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

    public IEnumerator SetupTeam(Opponent opponent, List<GetAxiesEnemies.AxieEnemy> axieEnemyList)
    {
        this.AxieEnemyList = axieEnemyList;

        while (badTeam.GetCharactersAll().Count < 5)
        {
            yield return null;
        }

        int index = 0;
        foreach (var teamUpgrades in opponent.axie_team.team_upgrades)
        {
            foreach (var upgradeAugument in teamUpgrades.upgrade_values_per_round)
            {
                atiaBlessing.AugumentUpgrade((int)upgradeAugument.upgrade_id,
                    upgradeAugument.axieClass?.Select(x => (AxieClass)x).ToList(),
                    badTeam);
            }

            index++;
            if (index >= RunManagerSingleton.instance.score)
            {
                break;
            }
        }

        index = 0;
        foreach (var axie in opponent.axie_team.axies)
        {
            foreach (var upgrade in axie.upgrades.upgrades_id)
            {
                atiaBlessing.AugumentUpgrade(upgrade, axie.axie_id, badTeam);

                index++;
                if (index >= RunManagerSingleton.instance.score)
                {
                    break;
                }
            }

            index = 0;
        }
    }
}