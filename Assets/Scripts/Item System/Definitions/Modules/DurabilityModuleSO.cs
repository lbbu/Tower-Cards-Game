using System;
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

    public override void ValidateDefinition()
    {
        if (float.IsNaN(maxDurability) || float.IsInfinity(maxDurability) || maxDurability <= 0f)
            throw new InvalidOperationException($"Durability module '{name}' must have finite, positive maximum durability.");
    }

    public override ItemModuleState CreateState()
    {
        return new DurabilityState(MaxDurability);
    }

}
