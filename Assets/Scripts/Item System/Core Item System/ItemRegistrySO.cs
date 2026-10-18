using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ItemRegistry",
    menuName = "Items/Item Registry"
)]
public class ItemRegistrySO : ScriptableObject
{
    [SerializeField] private List<ItemDataSO> items = new();

    private Dictionary<string, ItemDataSO> itemLookup;

    private void OnEnable()
    {
        itemLookup = null;
    }

    private void BuildLookup()
    {
        itemLookup = new Dictionary<string, ItemDataSO>();

        foreach (ItemDataSO item in items)
        {
            if (item == null)
            {
                Debug.LogWarning("ItemRegistry contains a null item.", this);
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.ItemID))
            {
                Debug.LogError(
                    $"Item '{item.name}' has an empty ItemID.", this);
                continue;
            }

            if (!itemLookup.TryAdd(item.ItemID, item))
            {
                Debug.LogError(
                    $"Duplicate ItemID found: {item.ItemID}", this);
            }
        }
    }

    public bool TryGetItem(string itemID, out ItemDataSO item)
    {
        if (itemLookup == null)
            BuildLookup();

        if (string.IsNullOrWhiteSpace(itemID))
        {
            item = null;
            return false;
        }

        return itemLookup.TryGetValue(itemID, out item);
    }
}