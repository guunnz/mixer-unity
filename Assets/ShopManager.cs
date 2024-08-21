using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    static public ShopManager instance;
    private RunManagerSingleton rm;
    private int RollCost = 1;
    public TextMeshProUGUI RollCostText;
    public ShopItemPurchasable[] Potions;
    public ShopItemPurchasable[] Items;
    public ShopItem[] ItemList;
    private List<int> indexesRolled = new List<int>();
    public bool FreezeMode;
    public GameObject FrozenCursor;
    private int FirstActivePotion = 0;
    private int SecondActivePotion = 0;
    public Transform Pot1Pos;
    public Transform Pot2Pos;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        instance = this;
    }

    public void ToggleFreezeMode()
    {
        SFXManager.instance.PlaySFX(SFXType.Freeze, 0.1f);
        FreezeMode = !FreezeMode;
        FrozenCursor.SetActive(!FrozenCursor.activeSelf);
    }

    public void SetShop()
    {

        if (RunManagerSingleton.instance.wins == 11 || RunManagerSingleton.instance.losses == 2)
        {
            MusicManager.Instance.PlayMusic(MusicTrack.NojyHypehu);
        }
        else
        {
            MusicManager.Instance.PlayMusic(MusicTrack.Ridthabus);
        }
        if (rm == null)
        {
            rm = RunManagerSingleton.instance;
        }

        if (RunManagerSingleton.instance.landType == LandType.arctic)
        {
            foreach (var item in Items)
            {
                if (item.Frozen)
                {
                    ShopItem cloneItem = item.shopItem.CreateClone();
                    cloneItem.price /= 2;
                    item.SetItem(cloneItem);
                }
            }

            foreach (var item in Potions)
            {
                if (item.Frozen)
                {
                    ShopItem cloneItem = item.shopItem.CreateClone();
                    cloneItem.price /= 2;
                    item.SetItem(cloneItem);
                }
            }
        }
        else if (RunManagerSingleton.instance.landType == LandType.savannah)
        {
            Potions[1].gameObject.SetActive(false);
        }
        else if (RunManagerSingleton.instance.landType == LandType.axiepark && RunManagerSingleton.instance.score == 0)
        {
            var itemListCustom = ItemList.ToList();
            var itemToBuy = ItemList.Single(x => x.ItemEffectName == AtiaBlessing.BuffEffect.Ruby).CreateClone();
            itemListCustom.RemoveAll(x => x.ItemEffectName == AtiaBlessing.BuffEffect.Ruby);
            ItemList = itemListCustom.ToArray();
            itemToBuy.price = 0;
            RunManagerSingleton.instance.BuyUpgrade(itemToBuy);
        }

        RollCostText.text = RollCost.ToString();
        DoRollShop();
    }

    public void PressRollShop()
    {
        if (RunManagerSingleton.instance.coins < RollCost)
            return;

        if (indexesRolled.Count >= 30)
        {
            indexesRolled = new List<int>();
        }

        SFXManager.instance.PlaySFX(SFXType.Roll, 0.1f);

        RunManagerSingleton.instance.RemoveCoins(RollCost);

        RunManagerSingleton.instance.economyPassive.RollsThisRound++;

        DoRollShop();
    }

    private void DoRollShop()
    {
        if (rm.economyPassive.RollsThisRound -
            rm.economyPassive.RollsFreePerRound < 0)
        {
            RollCost = 0;
        }
        else if (rm.economyPassive.RollsFreePerRound != 0 && rm.economyPassive.RollsThisRound == 0 || rm.economyPassive.RollsThisRound != 0 &&
                 rm.economyPassive.FreeRerollEveryXRolls != 0 && rm.economyPassive.RollsThisRound % rm.economyPassive.FreeRerollEveryXRolls == 0)
        {
            RollCost = 0;
        }
        else
        {
            RollCost = rm.economyPassive.RollCost;
        }

        RollCostText.text = RollCost.ToString();
        int PotionRandom1 = 0;
        int PotionRandom2 = 0;

        if (!Potions[FirstActivePotion].Frozen)
        {
            PotionRandom1 = Random.Range(0, Potions.Length);
            FirstActivePotion = PotionRandom1;
        }

        if (!Potions[SecondActivePotion].Frozen)
        {
            PotionRandom2 = Random.Range(0, Potions.Length);
            while (PotionRandom2 == PotionRandom1)
            {
                PotionRandom2 = Random.Range(0, Potions.Length);
            }


            SecondActivePotion = PotionRandom2;
        }

        foreach (var shopItemPurchasable in Potions)
        {
            shopItemPurchasable.sold = false;
            shopItemPurchasable.Sold.gameObject.SetActive(false);
            shopItemPurchasable.gameObject.SetActive(false);
        }

        Potions[FirstActivePotion].transform.position = Pot1Pos.position;
        Potions[SecondActivePotion].transform.position = Pot2Pos.position;
        Potions[FirstActivePotion].gameObject.SetActive(true);
        Potions[SecondActivePotion].gameObject.SetActive(true);


        if (!Items[0].Frozen)
        {
            int ItemRandom1 = Random.Range(0, ItemList.Length);

            while (indexesRolled.Contains(ItemRandom1))
            {
                ItemRandom1 = Random.Range(0, ItemList.Length);
            }

            indexesRolled.Add(ItemRandom1);
            Items[0].SetItem(ItemList[ItemRandom1]);
        }

        if (!Items[1].Frozen)
        {
            int ItemRandom2 = Random.Range(0, ItemList.Length);

            while (indexesRolled.Contains(ItemRandom2))
            {
                ItemRandom2 = Random.Range(0, ItemList.Length);
            }

            indexesRolled.Add(ItemRandom2);

            Items[1].SetItem(ItemList[ItemRandom2]);
        }
    }
}