using UnityEngine;

public class ItemSystemTest : MonoBehaviour
{
    [SerializeField] private ItemDataSO swordItem;

    private void Start()
    {
        if (swordItem == null)
        {
            Debug.LogError("Sword Item is not assigned!");
            return;
        }

        ItemInstance sword1 = new ItemInstance(swordItem);
        ItemInstance sword2 = new ItemInstance(swordItem);

        Debug.Log("Sword 1 ID: " + sword1.InstanceID);
        Debug.Log("Sword 2 ID: " + sword2.InstanceID);

        Debug.Log("Same ItemData: " +
            (sword1.ItemData == sword2.ItemData));

        Debug.Log("Different Instance IDs: " +
            (sword1.InstanceID != sword2.InstanceID));

        if (swordItem.TryGetModule<DurabilityModuleSO>(
            out DurabilityModuleSO durabilityModule))
        {
            Debug.Log("Max Durability: " +
                durabilityModule.MaxDurability);
        }
    }
}