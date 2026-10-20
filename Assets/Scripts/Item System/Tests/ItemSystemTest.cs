using System;
using UnityEngine;

public class ItemSystemTest : MonoBehaviour
{
    [SerializeField] private ItemDataSO swordItem;
    [SerializeField] private ItemRegistrySO itemRegistry;

    private void Start()
    {
        ItemInstance sword = new ItemInstance(swordItem);
        ItemSaveData saveData = sword.CaptureSaveData();

        saveData.states[0].jsonData =
            "{\"currentDurability\":9999999}";

        try
        {
            ItemInstance restored =
                ItemInstance.RestoreFromSaveData(
                    saveData,
                    itemRegistry
                );

            Debug.LogError(
                "TEST FAILED: Invalid durability was accepted!"
            );
        }
        catch (ArgumentOutOfRangeException ex)
        {
            Debug.Log(
                "TEST PASSED: Invalid durability rejected. " +
                ex.Message
            );
        }
    }

}