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
    public Transform EnemyBlessingsContainerPostMatch;
    public Transform ItemsContainerPostMatch;
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


        for (int i = EnemyBlessingsContainerPostMatch.childCount - 1; i >= 0; i--)
        {
            // Access the child at index 'i' and destroy it
            GameObject.Destroy(EnemyBlessingsContainerPostMatch.GetChild(i).gameObject);
        }

        for (int i = ItemsContainerPostMatch.childCount - 1; i >= 0; i--)
        {
            // Access the child at index 'i' and destroy it
            GameObject.Destroy(ItemsContainerPostMatch.GetChild(i).gameObject);
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

            var blessing = blessingsList.FirstOrDefault(x => x.ItemEffectName == effect);
            if (blessing == null)
            {
                blessing = BlessingManager.LandBlessing;
            }
            item.SetItem(blessing);

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
                var blessing = blessingsList.FirstOrDefault(x => x.ItemEffectName == effect);
                if (blessing == null)
                {
                    switch (effect)
                    {
                        case AtiaBlessing.BuffEffect.AxiePark:
                            blessing = BlessingManager.AxieParkBlessing;
                            break;
                        case AtiaBlessing.BuffEffect.Savannah:
                            blessing = BlessingManager.SavannahBlessing;
                            break;
                        case AtiaBlessing.BuffEffect.Forest:
                            blessing = BlessingManager.ForestBlessing;
                            break;
                        case AtiaBlessing.BuffEffect.Arctic:
                            blessing = BlessingManager.ArcticBlessing;
                            break;
                        case AtiaBlessing.BuffEffect.Mystic:
                            blessing = BlessingManager.MysticBlessing;
                            break;
                        case AtiaBlessing.BuffEffect.Genesis:
                            blessing = BlessingManager.GenesisBlessing;
                            break;
                        case AtiaBlessing.BuffEffect.LunasLanding:
                            blessing = BlessingManager.LunasLandingBlessing;
                            break;
                    }


                    ItemImageTooltip item = Instantiate(ItemPrefab, EnemyBlessingsContainer).GetComponent<ItemImageTooltip>();
                    ItemImageTooltip item2 = Instantiate(ItemPrefab, EnemyBlessingsContainerPostMatch).GetComponent<ItemImageTooltip>();
                    item.SetItem(blessing);
                    item2.SetItem(blessing);
                    itemsList.Add(item);
                    itemsList.Add(item2);
                }
                else
                {

                    ItemImageTooltip item = Instantiate(ItemPrefab, EnemyBlessingsContainer).GetComponent<ItemImageTooltip>();
                    ItemImageTooltip item2 = Instantiate(ItemPrefab, EnemyBlessingsContainerPostMatch).GetComponent<ItemImageTooltip>();
                    item.SetItem(blessingsList.Single(x => x.ItemEffectName == effect));
                    item2.SetItem(blessingsList.Single(x => x.ItemEffectName == effect));
                    itemsList.Add(item);
                    itemsList.Add(item2);
                }
            }
            else
            {
                if (buffList.Contains(buff))
                {
                    itemsList.Where(x => x.ShopItem.ItemEffectName == (AtiaBlessing.BuffEffect)buff).ToList().ForEach(x => x.IncreaseTimes());
                    return;
                }
                ItemImageTooltip item = Instantiate(ItemPrefab, ItemsContainer).GetComponent<ItemImageTooltip>();
                ItemImageTooltip item2 = Instantiate(ItemPrefab, ItemsContainerPostMatch).GetComponent<ItemImageTooltip>();
                item.SetItem(itemList.Single(x => x.ItemEffectName == effect));
                item2.SetItem(itemList.Single(x => x.ItemEffectName == effect));
                itemsList.Add(item);
                itemsList.Add(item2);
            }
            buffList.Add(buff);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message + " " + (AtiaBlessing.BuffEffect)buff);
        }

    }
}
