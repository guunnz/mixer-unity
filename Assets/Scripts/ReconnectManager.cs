using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Reconnection
{
    public enum GameState
    {

    }
    public Run run;
    public int coins;
    public AtiaBlessing.BuffEffect shopItem1Effect;
    public AtiaBlessing.BuffEffect shopItem2Effect;
    public AtiaBlessing.BuffEffect potionsEffects1;
    public AtiaBlessing.BuffEffect potionsEffects2;
}

public class ReconnectManager : MonoBehaviour
{
    public DragAndDropCharacter dragAndDropCharacter;
    public ShopManager shopManager;
    public GameObject Reconnecting;

    private void Start()
    {

    }

    public void Reconnect(Reconnection reconnection)
    {
        Reconnecting.SetActive(true);
        var runManager = RunManagerSingleton.instance;
        foreach (var axie in runManager.goodTeam.GetCharactersAll())
        {
            var axiePos = reconnection.run.axie_team.axies.FirstOrDefault(x => x.axie_id == axie.AxieId.ToString()).position_values_per_round[reconnection.run.win_loss_record.Count()];

            //Move Axie
        }

        shopManager.SetManualShopItems(reconnection.potionsEffects1, reconnection.potionsEffects2, reconnection.shopItem1Effect, reconnection.shopItem2Effect);
        runManager.coins = reconnection.coins;
    }
}