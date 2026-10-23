using System;
using Newtonsoft.Json;

[Serializable]
public class DurabilityStateData : ItemModuleStateData
{
    [JsonProperty(Required = Required.Always)]
    public float currentDurability;
}
