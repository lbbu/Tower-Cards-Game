using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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


    public ItemSaveData CaptureSaveData()
    {
        ItemSaveData saveData = new ItemSaveData
        {
            itemID = ItemData.ItemID,
            instanceID = InstanceID
        };

        foreach (var pair in moduleStates)
        {
            ItemModuleState state = pair.Value;

            ItemModuleStateData stateData = state.CaptureState();

            ItemStateSaveData savedState = new ItemStateSaveData
            {
                stateID = state.StateID,
                jsonData = JsonConvert.SerializeObject(stateData)
            };

            saveData.states.Add(savedState);
        }

        return saveData;
    }


    public static ItemInstance RestoreFromSaveData(
    ItemSaveData saveData,
    ItemRegistrySO registry)
    {
        if (saveData == null)
            throw new ArgumentNullException(nameof(saveData));

        if (registry == null)
            throw new ArgumentNullException(nameof(registry));

        if (!registry.TryGetItem(saveData.itemID, out ItemDataSO itemData))
        {
            throw new InvalidOperationException(
                $"Item not found in registry: {saveData.itemID}");
        }

        // Create instance using the saved ID
        ItemInstance item = new ItemInstance(
            itemData,
            saveData.instanceID
        );

        if (saveData.states == null)
            throw new InvalidOperationException("Saved states list is null.");

        // Build lookup using stable StateIDs
        var statesByID = new Dictionary<string, ItemModuleState>(
            StringComparer.Ordinal
        );

        foreach (ItemModuleState state in item.moduleStates.Values)
        {
            if (string.IsNullOrWhiteSpace(state.StateID))
                throw new InvalidOperationException("Empty StateID.");

            if (!statesByID.TryAdd(state.StateID, state))
            {
                throw new InvalidOperationException(
                    $"Duplicate runtime StateID: {state.StateID}");
            }
        }

        var restoredIDs = new HashSet<string>(StringComparer.Ordinal);

        foreach (ItemStateSaveData savedState in saveData.states)
        {
            if (savedState == null ||
                string.IsNullOrWhiteSpace(savedState.stateID))
            {
                throw new InvalidOperationException(
                    "Invalid saved state entry.");
            }

            if (!restoredIDs.Add(savedState.stateID))
            {
                throw new InvalidOperationException(
                    $"Duplicate saved StateID: {savedState.stateID}");
            }

            if (!statesByID.TryGetValue(
                savedState.stateID,
                out ItemModuleState runtimeState))
            {
                throw new InvalidOperationException(
                    $"Unknown StateID: {savedState.stateID}");
            }

            if (string.IsNullOrWhiteSpace(savedState.jsonData))
            {
                throw new InvalidOperationException(
                    $"Empty JSON for state: {savedState.stateID}");
            }

            ItemModuleStateData stateData =
                JsonConvert.DeserializeObject(
                    savedState.jsonData,
                    runtimeState.StateDataType
                ) as ItemModuleStateData;

            if (stateData == null)
            {
                throw new InvalidOperationException(
                    $"Invalid data for state: {savedState.stateID}");
            }

            runtimeState.RestoreState(stateData);
        }

        // Make sure no expected state was missing
        if (restoredIDs.Count != statesByID.Count)
        {
            throw new InvalidOperationException(
                "One or more runtime states are missing from saved data.");
        }

        return item;
    }


}