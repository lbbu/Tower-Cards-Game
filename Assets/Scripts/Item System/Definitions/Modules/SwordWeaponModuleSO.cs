using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SwordWeapon", menuName = "Items/Modules/SwordWeapon")]
public class SwordWeaponModuleSO : ItemModuleSO
{
    
    [SerializeField] private float baseDamage;
    [SerializeField] private float hitsPerSecond;
    


    public float Damage => baseDamage;
    public float HitsPerSecond => hitsPerSecond;

    public override void ValidateDefinition()
    {
        if (float.IsNaN(baseDamage) || float.IsInfinity(baseDamage) || baseDamage < 0f)
            throw new InvalidOperationException($"Sword module '{name}' has invalid base damage.");

        if (float.IsNaN(hitsPerSecond) || float.IsInfinity(hitsPerSecond) || hitsPerSecond < 0f)
            throw new InvalidOperationException($"Sword module '{name}' has invalid hits per second.");
    }
}
