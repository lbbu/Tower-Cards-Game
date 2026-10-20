using UnityEngine;

public static class ItemRegistryRuntimeInitializer
{
    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void Initialize()
    {
        ItemRegistrySO[] registries =
            Resources.FindObjectsOfTypeAll<ItemRegistrySO>();

        foreach (ItemRegistrySO registry in registries)
        {
            if (registry != null)
            {
                registry.InvalidateLookup();
            }
        }
    }
}