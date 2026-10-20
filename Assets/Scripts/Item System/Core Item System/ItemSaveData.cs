using System;
using System.Collections.Generic;

[Serializable]
public class ItemSaveData
{
    public string itemID;
    public string instanceID;

    public List<ItemStateSaveData> states = new();
}

[Serializable]
public class ItemStateSaveData
{
    public string stateID;
    public string jsonData;
}