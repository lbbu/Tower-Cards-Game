using UnityEngine;

public abstract class ItemModuleSO : ScriptableObject
{
    public virtual void ValidateDefinition()
    {
    }

    // Stateless modules intentionally return null. Stateful modules must return a new state.
    public virtual ItemModuleState CreateState()
    {
        return null;
    }
}
