using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro; // Include the namespace for TextMeshPro

public class ItemRelicManager : MonoBehaviour
{
    public GameObject itemPrefab;
    public Transform itemsParent; // Parent object for all items
    public TextMeshProUGUI plusItemText; // TextMeshProUGUI component for showing toggle text
    public GameObject plusItemObject; // GameObject that contains the plusItemText
    public float minX;
    public float maxX;
    public float spacing;
    public Vector2 minBounds;
    public Vector2 maxBounds;
    public float startY; // Starting Y position for the first row
    public float startZ; // Starting Z position

    private List<GameObject> items = new List<GameObject>();
    private bool isShowingMore = false;
    private const int MaxVisibleItems = 5;
    private float currentX;
    private float currentY;
    private float currentZ;
    public static ItemRelicManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentX = minX;
        currentY = startY;
        currentZ = startZ;
        plusItemObject.SetActive(false); // Initialize plus item visibility
        UpdatePlusItemText(); // Initialize plus item text
    }

    public void InstantiateItem(Vector3 startPosition, ShopItem shopItem)
    {
        GameObject newItem = Instantiate(itemPrefab, startPosition, itemPrefab.transform.rotation, itemsParent);
        if (shopItem.isPotion())
        {
            newItem.transform.localScale = new Vector3(.008f, .008f, 1);
        }
        newItem.GetComponent<ItemLikeRelic>().SetShopItem(shopItem);
        newItem.SetActive(isShowingMore || items.Count < MaxVisibleItems);
        items.Add(newItem);

        UpdateItemPositions(newItem.activeSelf);
    }

    private void UpdateItemPositions(bool animate = false)
    {
        int numItemsInRow = Mathf.FloorToInt((maxX - minX) / spacing) + 1;  // Calculate how many items can fit in a single row
        for (int i = 0; i < items.Count; i++)
        {
            items[i].SetActive(i < MaxVisibleItems || isShowingMore);  // Control visibility based on whether "Show More" is active

            // Calculate position based on item index
            float row = i / numItemsInRow;
            float col = i % numItemsInRow;

            float newX = minX + col * spacing;
            float newY = startY - row * (spacing / 1.5f);  // Adjust Y for each new row
            float newZ = startZ - row * (spacing / 1.5f);  // Adjust Z for each new row

            Vector3 targetPosition = new Vector3(newX, newY, newZ);
            if (animate && items[i].activeSelf)
                items[i].transform.DOLocalMove(targetPosition, 0.5f);  // Animate movement if needed
            else
                items[i].transform.localPosition = targetPosition;  // Update position without animation
        }

        UpdatePlusItemPosition();  // Update the position of the plus item
    }


    private void UpdatePlusItemPosition()
    {
        // Calculate how many items can fit within one row based on spacing and the width from minX to maxX
        int numItemsInRow = Mathf.FloorToInt((maxX - minX) / spacing) + 1;
        // Determine the number of visible items, which can be the maximum visible items or total items if showing more
        int totalVisibleItems = isShowingMore ? items.Count : Mathf.Min(items.Count, MaxVisibleItems);

        if (totalVisibleItems < items.Count || isShowingMore)
        {
            plusItemObject.SetActive(true);

            // Plus item index would be right after the last visible item
            int plusItemIndex = totalVisibleItems; // Place the plus item after the last visible item

            float row = plusItemIndex / numItemsInRow;
            float col = plusItemIndex % numItemsInRow;

            float newX = minX + col * spacing;
            float newY = startY - row * (spacing / 1.5f); // Adjust Y for each new row
            float newZ = startZ - row * (spacing / 1.5f); // Adjust Z similarly

            // Check if the new X position exceeds maxX and should wrap to the next row
            if (newX > maxX)
            {
                newX = minX; // Start at minX on the next row
                newY -= spacing / 1.5f;
                newZ -= spacing / 1.5f;
            }

            // Animate movement of the plusItemObject to the new calculated position
            plusItemObject.transform.DOLocalMove(new Vector3(newX, newY, newZ), 0.5f).SetEase(Ease.InOutQuad);
        }
        else
        {
            plusItemObject.SetActive(false);
        }
        UpdatePlusItemText();
    }






    private void UpdatePlusItemText()
    {
        plusItemText.text = isShowingMore ? "[-]" : "[+]"; // Update text based on the current display state
    }

    public void ToggleShowMoreLess()
    {
        isShowingMore = !isShowingMore;
        UpdateItemPositions(); // This will update positions without animation
    }
}
