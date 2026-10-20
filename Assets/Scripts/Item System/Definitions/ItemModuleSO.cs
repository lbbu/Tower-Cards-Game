using UnityEngine;

public abstract class ItemModuleSO : ScriptableObject
{
    public virtual ItemModuleState CreateState()
    {
        return null;
    }
}