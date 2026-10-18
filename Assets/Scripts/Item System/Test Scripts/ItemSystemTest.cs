using NUnit.Framework.Internal.Execution;
using UnityEngine;

public class ItemSystemTest : MonoBehaviour
{
    [SerializeField] private ItemDataSO swordItem;

    private void Start()
    {

        ItemInstance originalSword = new ItemInstance(swordItem);

        originalSword.TryGetState<DurabilityState>(out var originalState);
        originalState.TakeDamage(45f);

        // Capture
        string savedID = originalSword.InstanceID;
        var savedStates = originalSword.CaptureStates();

        // Restore
        ItemInstance restoredSword = new ItemInstance(swordItem, savedID);
        restoredSword.RestoreStates(savedStates);

        restoredSword.TryGetState<DurabilityState>(out var restoredState);

        Debug.Log("Same ID: " +
            (originalSword.InstanceID == restoredSword.InstanceID));

        Debug.Log("Restored Durability: " +
            restoredState.CurrentDurability);

    }
}