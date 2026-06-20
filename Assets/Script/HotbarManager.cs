using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class DefaultItemEntry
{
    public ItemData item;
    public int count = 1;
}

[ExecuteInEditMode]
public class HotbarManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private SlotUI[] uiSlots;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Sprite selectedSlotSprite;

    [Header("Default Items")]
    [SerializeField] private DefaultItemEntry[] defaultItems;

    [Header("Layout")]
    [SerializeField] private float slotWidth = 70f;
    [SerializeField] private float slotHeight = 70f;
    [SerializeField] private float spacing = 6f;
    [SerializeField] private float yOffset = 0f;

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
        bool needsGenerate = uiSlots == null || uiSlots.Length == 0;
        if (!needsGenerate && uiSlots[0] == null) needsGenerate = true;

        if (needsGenerate)
        {
            GenerateSlots();
            if (slotPrefab == null) return;
        }

        Debug.Log("[HotbarManager] Awake called. uiSlots length: " + (uiSlots?.Length ?? -1));

        if (!Application.isPlaying) return;

        hotbarSize = uiSlots.Length;
        inventoryItems = new ItemData[hotbarSize];
        itemCounts = new int[hotbarSize];
    }

    void GenerateSlots()
    {
        if (slotPrefab == null)
        {
            Debug.LogError("[HotbarManager] slotPrefab is not assigned! Cannot generate slots.");
            return;
        }

        uiSlots = new SlotUI[9];
        float totalWidth = 9 * slotWidth + 8 * spacing;
        float startX = -totalWidth / 2f + slotWidth / 2f;

        for (int i = 0; i < 9; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, transform);
            slotObj.name = "Slot_" + (i + 1);

            RectTransform rt = slotObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(slotWidth, slotHeight);
            rt.anchoredPosition = new Vector2(startX + i * (slotWidth + spacing), yOffset);
            rt.localScale = Vector3.one;

            SlotUI slot = slotObj.GetComponent<SlotUI>();
            slot.SetSlotIndex(i);
            slot.SetSelectedSprite(selectedSlotSprite);
            uiSlots[i] = slot;
        }
    }

    void Start()
    {
        if (!Application.isPlaying) return;
        Debug.Log("[HotbarManager] Start called. selectedIndex=" + selectedIndex);

        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (uiSlots[i] != null)
            {
                uiSlots[i].SetSlotIndex(i);
                uiSlots[i].SetSelectedSprite(selectedSlotSprite);
            }
        }

        UpdateHighlightVisual();
        RefreshUI();

        if (defaultItems != null)
        {
            foreach (DefaultItemEntry entry in defaultItems)
            {
                if (entry != null && entry.item != null)
                    AddItem(entry.item, entry.count);
            }
        }
    }

    void Update()
    {
        if (!Application.isPlaying) return;
        HandleScrollInput();
        HandleNumberKeys();
    }

#if UNITY_EDITOR
    [ContextMenu("Regenerate Slots")]
    void RegenerateInEditor()
    {
        if (Application.isPlaying) return;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith("Slot_"))
                DestroyImmediate(child.gameObject);
        }

        uiSlots = null;
        GenerateSlots();

        EditorUtility.SetDirty(this);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
    }
#endif

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

    public void SetSelected(int index)
    {
        if (index < 0 || index >= hotbarSize) return;
        selectedIndex = index;
        UpdateHighlightVisual();
    }

    public void SwapSlot(int fromIndex, int toIndex)
    {
        if (fromIndex < 0 || fromIndex >= hotbarSize || toIndex < 0 || toIndex >= hotbarSize)
            return;
        if (fromIndex == toIndex) return;

        ItemData tempItem = inventoryItems[fromIndex];
        int tempCount = itemCounts[fromIndex];

        inventoryItems[fromIndex] = inventoryItems[toIndex];
        itemCounts[fromIndex] = itemCounts[toIndex];

        inventoryItems[toIndex] = tempItem;
        itemCounts[toIndex] = tempCount;
    }

    public ItemData GetItem(int index)
    {
        if (index < 0 || index >= hotbarSize) return null;
        return inventoryItems[index];
    }

    public int GetItemCount(int index)
    {
        if (index < 0 || index >= hotbarSize) return 0;
        return itemCounts[index];
    }

    public void SetItem(int index, ItemData item, int count)
    {
        if (index < 0 || index >= hotbarSize) return;
        inventoryItems[index] = item;
        itemCounts[index] = count;
    }

    void UpdateHighlightVisual()
    {
        for (int i = 0; i < uiSlots.Length; i++)
        {
            if (uiSlots[i] != null)
            {
                // PASTIKAN memanggil SetHighlight, BUKAN ToggleHighlight
                uiSlots[i].SetHighlight(i == selectedIndex); 
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

    public bool ConsumeItem(int index, int count = 1)
    {
        if (index < 0 || index >= hotbarSize) return false;
        if (inventoryItems[index] == null) return false;

        itemCounts[index] -= count;
        if (itemCounts[index] <= 0)
        {
            inventoryItems[index] = null;
            itemCounts[index] = 0;
        }
        RefreshUI();
        return true;
    }
}
