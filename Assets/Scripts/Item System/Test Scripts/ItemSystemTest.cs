using UnityEngine;

public class ItemSystemTest : MonoBehaviour
{
    [SerializeField] private ItemDataSO swordItem;

    private void Start()
    {
        ItemInstance sword1 = new ItemInstance(swordItem);
        ItemInstance sword2 = new ItemInstance(swordItem);

        if (sword1.TryGetState<DurabilityState>(out var state1) &&
            sword2.TryGetState<DurabilityState>(out var state2))
        {
            state1.TakeDamage(30f);

            Debug.Log("Sword 1 Durability: " + state1.CurrentDurability);
            Debug.Log("Sword 2 Durability: " + state2.CurrentDurability);
        }
    }
}