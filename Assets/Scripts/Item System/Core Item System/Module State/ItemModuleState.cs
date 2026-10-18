public abstract class ItemModuleState
{
    public abstract ItemModuleStateData CaptureState();

    public abstract void RestoreState(ItemModuleStateData data);
}