using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public bool isStackable;
    // Lu bisa tambah stat lain di sini nanti (misal: damage, heal, durabilitas) tanpa merusak script UI.
}