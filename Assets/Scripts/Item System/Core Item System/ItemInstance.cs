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

        foreach (ItemModuleSO module in itemData.ItemModules)
        {
            if (module == null)
                continue;

            ItemModuleState state = module.CreateState();

            if (state == null)
                continue;

            Type stateType = state.GetType();

            if (!moduleStates.TryAdd(stateType, state))
            {
                throw new InvalidOperationException(
                    $"Duplicate runtime state type: {stateType.Name}"
                );
            }
        }
    }

    public bool TryGetState<TState>(out TState state)
        where TState : ItemModuleState
    {
        if (moduleStates.TryGetValue(
            typeof(TState), out ItemModuleState foundState))
        {
            state = (TState)foundState;
            return true;
        }

        state = null;
        return false;
    }
}