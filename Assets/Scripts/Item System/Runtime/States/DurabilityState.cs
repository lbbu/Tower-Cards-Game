using System;

public class DurabilityState : ItemModuleState
{
    private readonly float maxDurability;

    public float CurrentDurability { get; private set; }

    public override string StateID => "durability";

    public override Type StateDataType => typeof(DurabilityStateData);

    public DurabilityState(float maxDurability)
    {
        if (maxDurability <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxDurability));

        this.maxDurability = maxDurability;
        CurrentDurability = maxDurability;
    }

    public void TakeDamage(float amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        CurrentDurability = Math.Max(
            0f,
            CurrentDurability - amount);
    }

    public void SetCurrentDurability(float value)
    {
        CurrentDurability = Math.Clamp(
            value,
            0f,
            maxDurability);
    }


    public override ItemModuleStateData CaptureState()
    {
        return new DurabilityStateData
        {
            currentDurability = CurrentDurability
        };
    }

    public override void RestoreState(ItemModuleStateData data)
    {
        if (data is not DurabilityStateData durabilityData)
        {
            throw new ArgumentException(
                "Invalid state data for DurabilityState.",
                nameof(data));
        }

        float value = durabilityData.currentDurability;

        if (float.IsNaN(value) ||
            float.IsInfinity(value) ||
            value < 0f ||
            value > maxDurability)
        {
            throw new ArgumentOutOfRangeException(
                nameof(data),
                $"Invalid durability value: {value}"
            );
        }

        CurrentDurability = value;
    }


}