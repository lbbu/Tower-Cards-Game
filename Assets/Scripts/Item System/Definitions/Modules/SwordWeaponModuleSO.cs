using UnityEngine;

[CreateAssetMenu(fileName = "SwordWeapon", menuName = "Items/Modules/SwordWeapon")]
public class SwordWeaponModuleSO : ItemModuleSO
{
    
    [SerializeField] private float baseDamage;
    [SerializeField] private float hitsPerSecond;
    


    public float Damage => baseDamage;
    public float HitsPerSecond => hitsPerSecond;
}
