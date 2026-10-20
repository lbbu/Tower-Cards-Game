using System;
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
        InvalidateLookup();
    }

    public void InvalidateLookup()
    {
        itemLookup = null;
    }

    private void BuildLookup()
    {
        var newLookup = new Dictionary<string, ItemDataSO>(
            StringComparer.Ordinal
        );

        if (items == null)
            throw new InvalidOperationException($"ItemRegistry '{name}' has a null item list.");

        foreach (ItemDataSO item in items)
        {
            if (item == null)
            {
                throw new InvalidOperationException(
                    $"ItemRegistry '{name}' contains a null item."
                );
            }

            item.ValidateDefinition();

            if (!newLookup.TryAdd(item.ItemID, item))
            {
                throw new InvalidOperationException(
                    $"Duplicate ItemID found: {item.ItemID}"
                );
            }
        }

        itemLookup = newLookup;

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
