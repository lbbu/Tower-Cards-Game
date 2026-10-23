# Item System integration contract

- Definitions and module configuration are shared and remain unchanged during gameplay. Call `ItemDataSO.ValidateDefinition()` to validate authored content. Item construction and registry building also validate definitions.
- Each item has at most one module of each concrete type. Modules without runtime state return `null` from `CreateState()`. Stateful modules return a new, independently owned state for each call.
- Runtime state types and stable, nonblank StateIDs are unique within an item. `StateDataType` identifies a concrete, closed `ItemModuleStateData` type; capture must return a compatible non-null DTO. IDs and data types remain stable during a state's lifetime.
- `CaptureSaveData()` and `RestoreFromSaveData()` are the supported complete snapshot boundary. The unused Type-keyed `CaptureStates()` / `RestoreStates()` APIs have been removed.
- The saved field names (`itemID`, `instanceID`, `states`, `stateID`, `jsonData`, `currentDurability`) and the `durability` StateID are unchanged. Required durability data must be present and non-null. Unknown inner DTO fields, malformed data and invalid values are rejected.
- State JSON uses an isolated Newtonsoft serializer with `TypeNameHandling.None`. The future save system owns outer JSON parsing, disk persistence, validation of duplicate instance identities across a load, and the save envelope version.
- A compatibility policy is required before shipping persistent saves across content/schema changes. Adding/removing states, renaming IDs, or reducing configured limits can invalidate older saves under the current strict restoration policy. No migration system is implemented here.
- Registry caching, `OnEnable` invalidation and the `SubsystemRegistration` reset remain in place. If an editor tool explicitly changes registry contents or ItemIDs after a lookup, it must call `InvalidateLookup()` on affected registries. Live definition mutation during gameplay is unsupported.
- Numeric durability inputs must be finite. Finite gameplay setters and damage still clamp to the intended limits; restoration rejects out-of-range values.

## Verification

In Unity Test Runner, run EditMode tests in `TowerCards.Items.Tests`, or use **Tools > Tower Cards > Run Item System Tests**. The menu exports NUnit XML to `Library/ItemSystem-EditMode-results.xml`. They use predefined editor assemblies so existing runtime assembly identities and asset GUIDs are preserved. Test modules are in-memory Editor-only doubles, not gameplay modules or assets.

Tests cover DTO validation, runtime state independence, save round-trips, early definition/state validation, registry failures and recovery, registered project content, and actual Play Mode entry with all four Domain/Scene Reload combinations. Lifecycle tests restore the editor's original reload preferences in teardown. Legacy unreferenced negative-test assets under `Items Data/Base items/Test` are not registry content and remain unchanged.
