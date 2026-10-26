using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

        itemData.ValidateDefinition();
        ItemData = itemData;
        InstanceID = Guid.NewGuid().ToString("N");

        var stateIDs = new HashSet<string>(StringComparer.Ordinal);
        foreach (ItemModuleSO module in itemData.ItemModules)
        {
            ItemModuleState state = module.CreateState();

            if (state == null)
                continue;

            Type stateType = state.GetType();

            if (string.IsNullOrWhiteSpace(state.StateID))
                throw new InvalidOperationException($"State '{stateType.Name}' on item '{itemData.name}' has an empty StateID.");

            if (!stateIDs.Add(state.StateID))
                throw new InvalidOperationException($"Duplicate runtime StateID on item '{itemData.name}': {state.StateID}.");

            Type dataType = state.StateDataType;
            if (dataType == null || !typeof(ItemModuleStateData).IsAssignableFrom(dataType) ||
                dataType.IsAbstract || dataType.ContainsGenericParameters)
            {
                throw new InvalidOperationException($"State '{state.StateID}' on item '{itemData.name}' has an invalid StateDataType.");
            }

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
        JsonSerializer serializer = CreateStateSerializer();
        ItemSaveData saveData = new ItemSaveData
        {
            itemID = ItemData.ItemID,
            instanceID = InstanceID
        };

        foreach (var pair in moduleStates)
        {
            ItemModuleState state = pair.Value;

            ItemModuleStateData stateData = state.CaptureState();

            if (stateData == null || !state.StateDataType.IsInstanceOfType(stateData))
                throw new InvalidOperationException($"State '{state.StateID}' captured incompatible state data.");

            using var jsonWriter = new StringWriter(CultureInfo.InvariantCulture);
            serializer.Serialize(jsonWriter, stateData, state.StateDataType);

            ItemStateSaveData savedState = new ItemStateSaveData
            {
                stateID = state.StateID,
                jsonData = jsonWriter.ToString()
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
        JsonSerializer serializer = CreateStateSerializer();

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

            using var textReader = new StringReader(savedState.jsonData);
            using var jsonReader = new JsonTextReader(textReader);
            ItemModuleStateData stateData = serializer.Deserialize(
                jsonReader, runtimeState.StateDataType) as ItemModuleStateData;

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

    private static JsonSerializer CreateStateSerializer()
    {
        // Create(), unlike CreateDefault()/JsonConvert, does not inherit global defaults.
        return JsonSerializer.Create(new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            MissingMemberHandling = MissingMemberHandling.Error,
            CheckAdditionalContent = true
        });
    }


}
