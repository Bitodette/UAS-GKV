using UnityEngine;

public class HotbarManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private SlotUI[] uiSlots;

    [Header("Inventory Data")]
    private int hotbarSize = 9;
    private int selectedIndex = 0;
    private ItemData[] inventoryItems;
    private int[] itemCounts;

    public ItemData SelectedItem
    {
        get
        {
            if (inventoryItems == null) return null;
            return inventoryItems[selectedIndex];
        }
    }

    public int SelectedIndex => selectedIndex;

    void Awake()
    {
        Debug.Log("[HotbarManager] Awake called. uiSlots == null? " + (uiSlots == null) + " | length: " + (uiSlots?.Length ?? -1));
        hotbarSize = uiSlots.Length;
        inventoryItems = new ItemData[hotbarSize];
        itemCounts = new int[hotbarSize];
    }

    void Start()
    {
        Debug.Log("[HotbarManager] Start called. selectedIndex=" + selectedIndex);
        UpdateHighlightVisual();
        RefreshUI();
    }

    void Update()
    {
        HandleScrollInput();
        HandleNumberKeys();
    }

    void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            Debug.Log("[HotbarManager] Scroll detected: " + scroll + " | selectedIndex before: " + selectedIndex);
            if (scroll > 0) selectedIndex--;
            else if (scroll < 0) selectedIndex++;

            if (selectedIndex < 0) selectedIndex = hotbarSize - 1;
            if (selectedIndex >= hotbarSize) selectedIndex = 0;

            Debug.Log("[HotbarManager] selectedIndex after: " + selectedIndex);
            UpdateHighlightVisual();
        }
    }

    void HandleNumberKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { Debug.Log("[HotbarManager] Key 1 pressed"); SetSelected(0); }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SetSelected(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SetSelected(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SetSelected(3);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) SetSelected(4);
        else if (Input.GetKeyDown(KeyCode.Alpha6)) SetSelected(5);
        else if (Input.GetKeyDown(KeyCode.Alpha7)) SetSelected(6);
        else if (Input.GetKeyDown(KeyCode.Alpha8)) SetSelected(7);
        else if (Input.GetKeyDown(KeyCode.Alpha9)) SetSelected(8);
    }

    void SetSelected(int index)
    {
        if (index < 0 || index >= hotbarSize) return;
        selectedIndex = index;
        UpdateHighlightVisual();
    }

    void UpdateHighlightVisual()
    {
        Debug.Log("[HotbarManager] UpdateHighlightVisual called. selectedIndex=" + selectedIndex + " | uiSlots length=" + (uiSlots?.Length ?? -1));
        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (uiSlots[i] != null)
            {
                uiSlots[i].SetHighlight(i == selectedIndex);
            }
            else
            {
                Debug.Log("[HotbarManager] uiSlots[" + i + "] is NULL!");
            }
        }
    }

    public void RefreshUI()
    {
        Debug.Log("[HotbarManager] RefreshUI called");
        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (uiSlots[i] != null)
            {
                uiSlots[i].UpdateSlot(inventoryItems[i], itemCounts[i]);
            }
        }
    }

    public bool AddItem(ItemData item, int count = 1)
    {
        Debug.Log("[HotbarManager] AddItem called. item=" + (item != null ? item.itemName : "NULL") + " count=" + count);
        if (item == null) return false;

        if (item.isStackable)
        {
            for (int i = 0; i < hotbarSize; i++)
            {
                if (inventoryItems[i] == item)
                {
                    itemCounts[i] += count;
                    RefreshUI();
                    return true;
                }
            }
        }

        for (int i = 0; i < hotbarSize; i++)
        {
            if (inventoryItems[i] == null)
            {
                inventoryItems[i] = item;
                itemCounts[i] = count;
                RefreshUI();
                return true;
            }
        }

        Debug.Log("Hotbar penuh!");
        return false;
    }
}
