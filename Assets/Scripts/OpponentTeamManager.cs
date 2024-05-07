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

        foreach (var teamUpgrades in opponent.axie_team.team_upgrades)
        {
            atiaBlessing.AugumentUpgrade((int)teamUpgrades.upgrade_id,
                teamUpgrades.axieClass?.Select(x => (AxieClass)x).ToList(),
                badTeam);
        }

        foreach (var axie in opponent.axie_team.axie)
        {
            foreach (var upgrade in axie.upgrades.upgrades_id)
            {
                atiaBlessing.AugumentUpgrade(upgrade, axie.axie_id, badTeam);
            }
        }
    }
}