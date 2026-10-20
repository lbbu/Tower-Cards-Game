using System;

public abstract class ItemModuleState
{
    public abstract string StateID { get; }

    public abstract Type StateDataType { get; }

    public abstract ItemModuleStateData CaptureState();

    public abstract void RestoreState(ItemModuleStateData data);
}