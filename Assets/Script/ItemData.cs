using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public bool isStackable;           // bisa ditumpuk dalam 1 slot (contoh: kayu, gandum)
    public bool isSeed;                // true kalo ini benih (bisa ditanam)
}
