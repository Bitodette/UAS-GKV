using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private SlotUI[] inventorySlots;
    [SerializeField] private Transform inventoryGrid;
    [SerializeField] private Sprite selectedSlotSprite;

    private ItemData[] inventoryItems;
    private int[] itemCounts;

    public SlotUI[] InventorySlots => inventorySlots;
    public ItemData[] InventoryItems => inventoryItems;
    public int[] ItemCounts => itemCounts;
    public int InventorySize => inventorySlots != null ? inventorySlots.Length : 0;
    public GameObject InventoryPanel => inventoryPanel;

    void Start()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        FindExistingSlots();
    }

    void FindExistingSlots()
    {
        if (inventoryGrid == null)
        {
            Debug.LogError("[InventoryManager] inventoryGrid not assigned!");
            return;
        }

        inventorySlots = inventoryGrid.GetComponentsInChildren<SlotUI>();
        inventoryItems = new ItemData[inventorySlots.Length];
        itemCounts = new int[inventorySlots.Length];

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            inventorySlots[i].SetSlotIndex(i);
            inventorySlots[i].SetIsInventorySlot(true);
            inventorySlots[i].SetSelectedSprite(selectedSlotSprite);
            inventorySlots[i].UpdateSlot(null, 0);
        }

        Debug.Log($"[InventoryManager] Found {inventorySlots.Length} existing slots under {inventoryGrid.name}");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
            ToggleInventory();
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool isActive = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isActive);

            if (isActive)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }

    public bool AddItem(ItemData item, int count = 1)
    {
        if (item == null) return false;
        if (inventoryItems == null) return false;

        if (item.isStackable)
        {
            for (int i = 0; i < inventorySlots.Length; i++)
            {
                if (inventoryItems[i] == item)
                {
                    itemCounts[i] += count;
                    RefreshUI();
                    return true;
                }
            }
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventoryItems[i] == null)
            {
                inventoryItems[i] = item;
                itemCounts[i] = count;
                RefreshUI();
                return true;
            }
        }

        Debug.Log("Inventory penuh!");
        return false;
    }

    public void SwapSlot(int fromIndex, int toIndex)
    {
        if (inventoryItems == null) return;
        if (fromIndex < 0 || fromIndex >= inventorySlots.Length || toIndex < 0 || toIndex >= inventorySlots.Length)
            return;
        if (fromIndex == toIndex) return;

        ItemData tempItem = inventoryItems[fromIndex];
        int tempCount = itemCounts[fromIndex];

        inventoryItems[fromIndex] = inventoryItems[toIndex];
        itemCounts[fromIndex] = itemCounts[toIndex];

        inventoryItems[toIndex] = tempItem;
        itemCounts[toIndex] = tempCount;
    }

    public void RemoveItem(int index)
    {
        if (inventoryItems == null) return;
        if (index < 0 || index >= inventorySlots.Length) return;
        inventoryItems[index] = null;
        itemCounts[index] = 0;
    }

    public void RefreshUI()
    {
        if (inventorySlots == null || inventoryItems == null) return;
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            if (inventorySlots[i] != null)
                inventorySlots[i].UpdateSlot(inventoryItems[i], itemCounts[i]);
        }
    }
}
