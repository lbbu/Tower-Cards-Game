using System;

public class DurabilityState : ItemModuleState
{
    public float CurrentDurability { get; private set; }

    public DurabilityState(float maxDurability)
    {
        if (maxDurability <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxDurability));

        CurrentDurability = maxDurability;
    }

    public void TakeDamage(float amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        CurrentDurability = Math.Max(0f, CurrentDurability - amount);
    }
}