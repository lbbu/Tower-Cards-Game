using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Items/Item Base/ItemData")]
public class ItemData : ScriptableObject
{

    [Header("Identity")]
    [SerializeField] private string itemID;
    [SerializeField] private string itemName;

    [TextArea(3, 5)]
    [SerializeField] private string itemDescription;

    [Header("Visuals")]
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private GameObject worldPrefab;

    [Header("General Properties")]
    [Min(0)]
    [SerializeField] private float basePrice;

    [Min(0f)]
    [SerializeField] private float itemWeight;

    [Min(1)]
    [SerializeField] private int maxStackSize = 1;

    public string ItemID => itemID;
    public string ItemName => itemName;
    public string ItemDescription => itemDescription;
    public Sprite ItemIcon => itemIcon;
    public GameObject WorldPrefab => worldPrefab;
    public float BasePrice => basePrice;
    public float ItemWeight => itemWeight;
    public int MaxStackSize => maxStackSize;




}
