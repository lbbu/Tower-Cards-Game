using UnityEngine;

[CreateAssetMenu(
    fileName = "DurabilityModule",
    menuName = "Items/Modules/Durability"
)]
public class DurabilityModuleSO : ItemModuleSO
{

    [Min(1f)]
    [SerializeField] private float maxDurability = 100f;

    public float MaxDurability => maxDurability;

    public override ItemModuleState CreateState()
    {
        return new DurabilityState(MaxDurability);
    }

}