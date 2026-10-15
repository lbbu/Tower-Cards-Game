using System;
using System.Collections.Generic;

public class ItemInstance
{
    private readonly Dictionary<Type, ItemModuleState> moduleStates = new();

    public string InstanceID { get; private set; }
    public ItemDataSO ItemData { get; private set; }

    public ItemInstance(ItemDataSO itemData)
    {
        if (itemData == null)
            throw new ArgumentNullException(nameof(itemData));

        ItemData = itemData;
        InstanceID = Guid.NewGuid().ToString("N");

        if (itemData.TryGetModule<DurabilityModuleSO>(
            out DurabilityModuleSO durabilityModule))
        {
            moduleStates.Add(
                typeof(DurabilityState),
                new DurabilityState(durabilityModule.MaxDurability)
            );
        }
    }

    public bool TryGetState<TState>(out TState state)
        where TState : ItemModuleState
    {
        if (moduleStates.TryGetValue(typeof(TState), out ItemModuleState foundState))
        {
            state = (TState)foundState;
            return true;
        }

        state = null;
        return false;
    }
}