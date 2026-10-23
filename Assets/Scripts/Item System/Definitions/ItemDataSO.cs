using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ItemData",
    menuName = "Items/Item Base/ItemData"
)]
public class ItemDataSO : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string itemID;
    [SerializeField] private string itemName;

    [TextArea(3, 5)]
    [SerializeField] private string itemDescription;

    [Header("Visuals")]
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private GameObject worldPrefab;

    [Header("General Properties")]
    [Min(0)]
    [SerializeField] private float basePrice;

    [Min(0f)]
    [SerializeField] private float itemWeight;

    [Min(1)]
    [SerializeField] private int maxStackSize = 1;

    [Header("Modules")]
    [SerializeField] private List<ItemModuleSO> itemModules = new();

    public IReadOnlyList<ItemModuleSO> ItemModules => itemModules;

    public string ItemID => itemID;
    public string ItemName => itemName;
    public string ItemDescription => itemDescription;
    public Sprite ItemIcon => itemIcon;
    public GameObject WorldPrefab => worldPrefab;
    public float BasePrice => basePrice;
    public float ItemWeight => itemWeight;
    public int MaxStackSize => maxStackSize;

    public void ValidateDefinition()
    {
        if (string.IsNullOrWhiteSpace(itemID))
            throw new InvalidOperationException($"Item '{name}' has an empty ItemID.");

        if (float.IsNaN(basePrice) || float.IsInfinity(basePrice) || basePrice < 0f)
            throw new InvalidOperationException($"Item '{name}' has an invalid base price.");

        if (float.IsNaN(itemWeight) || float.IsInfinity(itemWeight) || itemWeight < 0f)
            throw new InvalidOperationException($"Item '{name}' has an invalid weight.");

        if (maxStackSize < 1)
            throw new InvalidOperationException($"Item '{name}' must have a positive MaxStackSize.");

        if (itemModules == null)
            throw new InvalidOperationException($"Item '{name}' has a null module list.");

        var moduleTypes = new HashSet<Type>();
        for (int index = 0; index < itemModules.Count; index++)
        {
            ItemModuleSO module = itemModules[index];
            if (module == null)
                throw new InvalidOperationException($"Item '{name}' has a missing module at index {index}.");

            if (!moduleTypes.Add(module.GetType()))
                throw new InvalidOperationException($"Item '{name}' has duplicate module type: {module.GetType().Name}.");

            module.ValidateDefinition();
        }
    }

    public bool TryGetModule<TModule>(out TModule module)
        where TModule : ItemModuleSO
    {
        foreach (ItemModuleSO itemModule in itemModules)
        {
            if (itemModule is TModule foundModule)
            {
                module = foundModule;
                return true;
            }
        }

        module = null;
        return false;
    }
}
