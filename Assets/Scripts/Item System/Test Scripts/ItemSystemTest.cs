using UnityEngine;
using Newtonsoft.Json;

public class ItemSystemTest : MonoBehaviour
{
    [SerializeField] private ItemDataSO swordItem;
    [SerializeField] private ItemRegistrySO itemRegistry;

    private void Start()
    {
        ItemInstance originalSword = new ItemInstance(swordItem);

        originalSword.TryGetState<DurabilityState>(
            out DurabilityState originalDurability);

        originalDurability.TakeDamage(45f);

        // Capture and serialize
        ItemSaveData saveData = originalSword.CaptureSaveData();

        string json = JsonConvert.SerializeObject(saveData);

        // Deserialize
        ItemSaveData loadedData =
            JsonConvert.DeserializeObject<ItemSaveData>(json);

        // Restore complete ItemInstance
        ItemInstance restoredSword =
            ItemInstance.RestoreFromSaveData(
                loadedData,
                itemRegistry
            );

        restoredSword.TryGetState<DurabilityState>(
            out DurabilityState restoredDurability);

        Debug.Log("Same Instance ID: " +
            (originalSword.InstanceID == restoredSword.InstanceID));

        Debug.Log("Same Item Data: " +
            (originalSword.ItemData == restoredSword.ItemData));

        Debug.Log("Original Durability: " +
            originalDurability.CurrentDurability);

        Debug.Log("Restored Durability: " +
            restoredDurability.CurrentDurability);
    }
}