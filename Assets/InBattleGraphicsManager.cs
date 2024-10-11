using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InBattleGraphicsManager : MonoBehaviour
{

    static public InBattleGraphicsManager Instance;

    public Transform MyBlessingsContainer;
    public Transform EnemyBlessingsContainer;
    public Transform ItemsContainer;
    public GameObject ItemPrefab;
    public GameObject BagGraphics;
    public ShopItem[] itemList;
    private ShopItem[] blessingsList;
    public AtiaBlessing BlessingManager;
    private List<int> buffList = new List<int>();
    private List<ItemImageTooltip> itemsList = new List<ItemImageTooltip>();

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        blessingsList = BlessingManager.blessingsList;
    }

    public void ToggleItems()
    {
        ItemsContainer.gameObject.SetActive(!ItemsContainer.gameObject.activeSelf);
    }

    public void CleanBlessingsOpponent()
    {
        for (int i = ItemsContainer.childCount - 1; i >= 0; i--)
        {
            // Access the child at index 'i' and destroy it
            GameObject.Destroy(ItemsContainer.GetChild(i).gameObject);
        }

        for (int i = EnemyBlessingsContainer.childCount - 1; i >= 0; i--)
        {
            // Access the child at index 'i' and destroy it
            GameObject.Destroy(EnemyBlessingsContainer.GetChild(i).gameObject);
        }
        itemsList.Clear();
        buffList.Clear();
        BagGraphics.SetActive(false);
    }

    public void AddUpgradeMe(int buff)
    {
        AtiaBlessing.BuffEffect effect = (AtiaBlessing.BuffEffect)buff;

        if (buff > (int)AtiaBlessing.BuffEffect.Topaz)
        {
            ItemImageTooltip item = Instantiate(ItemPrefab, MyBlessingsContainer).GetComponent<ItemImageTooltip>();

            item.SetItem(blessingsList.Single(x => x.ItemEffectName == effect));

        }
    }

    public void AddUpgrade(int buff)
    {
        try
        {
            BagGraphics.SetActive(true);
            AtiaBlessing.BuffEffect effect = (AtiaBlessing.BuffEffect)buff;
            

            if (buff > (int)AtiaBlessing.BuffEffect.Topaz)
            {
                ItemImageTooltip item = Instantiate(ItemPrefab, EnemyBlessingsContainer).GetComponent<ItemImageTooltip>();
                item.SetItem(blessingsList.Single(x => x.ItemEffectName == effect));
                itemsList.Add(item);
            }
            else
            {
                if (buffList.Contains(buff))
                {
                    itemsList.FirstOrDefault(x => x.ShopItem.ItemEffectName == (AtiaBlessing.BuffEffect)buff).IncreaseTimes();
                    return;
                }
                ItemImageTooltip item = Instantiate(ItemPrefab, ItemsContainer).GetComponent<ItemImageTooltip>();
                item.SetItem(itemList.Single(x => x.ItemEffectName == effect));
                itemsList.Add(item);
            }
            buffList.Add(buff);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message + " " + (AtiaBlessing.BuffEffect)buff);
        }

    }
}
