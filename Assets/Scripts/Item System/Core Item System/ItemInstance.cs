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

    public ItemInstance(ItemDataSO itemData, string instanceID)
    : this(itemData)
    {
        if (string.IsNullOrWhiteSpace(instanceID))
            throw new ArgumentException(
                "Instance ID cannot be empty.",
                nameof(instanceID));

        InstanceID = instanceID;
    }


    public Dictionary<Type, ItemModuleStateData> CaptureStates()
    {
        var savedStates = new Dictionary<Type, ItemModuleStateData>();

        foreach (var pair in moduleStates)
        {
            savedStates.Add(
                pair.Key,
                pair.Value.CaptureState()
            );
        }

        return savedStates;
    }

    public void RestoreStates(
        Dictionary<Type, ItemModuleStateData> savedStates)
    {
        if (savedStates == null)
            throw new ArgumentNullException(nameof(savedStates));

        foreach (var pair in savedStates)
        {
            if (moduleStates.TryGetValue(
                pair.Key, out ItemModuleState state))
            {
                state.RestoreState(pair.Value);
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