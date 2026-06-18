using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SlotUI : MonoBehaviour, IPointerClickHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Sprite selectedSprite;

    [SerializeField] private GameObject hoverOverlay;

    private Image slotImage;
    private Sprite defaultSprite;
    private Color defaultColor;
    private int slotIndex;
    private bool isInventorySlot;

    void Awake()
    {
        slotImage = GetComponent<Image>();
        if (slotImage != null)
        {
            defaultSprite = slotImage.sprite;
            defaultColor = slotImage.color;
        }

        if (hoverOverlay != null)
            hoverOverlay.SetActive(false);
    }

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }

    public void SetIsInventorySlot(bool value)
    {
        isInventorySlot = value;
    }

    public bool IsInventorySlot => isInventorySlot;
    public int SlotIndex => slotIndex;

    public void SetSelectedSprite(Sprite sprite)
    {
        if (sprite != null)
            selectedSprite = sprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isInventorySlot)
        {
            HotbarManager hotbar = FindFirstObjectByType<HotbarManager>();
            if (hotbar != null && slotIndex < 9)
                hotbar.SetSelected(slotIndex);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverOverlay != null)
            hoverOverlay.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverOverlay != null)
            hoverOverlay.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggedItem == null) return;

        SlotUI sourceSlot = draggedItem.parentAfterDrag.GetComponent<SlotUI>();
        if (sourceSlot == null) return;

        HotbarManager hotbar = FindFirstObjectByType<HotbarManager>();
        InventoryManager inventory = FindFirstObjectByType<InventoryManager>();

        int fromIndex = sourceSlot.SlotIndex;
        int toIndex = slotIndex;

        bool sourceIsHotbar = !sourceSlot.IsInventorySlot;
        bool targetIsHotbar = !isInventorySlot;

        if (sourceIsHotbar && targetIsHotbar)
        {
            hotbar.SwapSlot(fromIndex, toIndex);
            hotbar.RefreshUI();
        }
        else if (!sourceIsHotbar && !targetIsHotbar)
        {
            inventory.SwapSlot(fromIndex, toIndex);
            inventory.RefreshUI();
        }
        else if (sourceIsHotbar && !targetIsHotbar)
        {
            ItemData sourceItem = hotbar.GetItem(fromIndex);
            int sourceCount = hotbar.GetItemCount(fromIndex);
            ItemData targetItem = inventory.InventoryItems[toIndex];
            int targetCount = inventory.ItemCounts[toIndex];

            hotbar.SetItem(fromIndex, targetItem, targetCount);
            inventory.InventoryItems[toIndex] = sourceItem;
            inventory.ItemCounts[toIndex] = sourceCount;

            hotbar.RefreshUI();
            inventory.RefreshUI();
        }
        else if (!sourceIsHotbar && targetIsHotbar)
        {
            ItemData sourceItem = inventory.InventoryItems[fromIndex];
            int sourceCount = inventory.ItemCounts[fromIndex];
            ItemData targetItem = hotbar.GetItem(toIndex);
            int targetCount = hotbar.GetItemCount(toIndex);

            inventory.InventoryItems[fromIndex] = targetItem;
            inventory.ItemCounts[fromIndex] = targetCount;
            hotbar.SetItem(toIndex, sourceItem, sourceCount);

            hotbar.RefreshUI();
            inventory.RefreshUI();
        }

        draggedItem.parentAfterDrag = transform;
        draggedItem.transform.SetParent(transform);
        draggedItem.transform.localPosition = Vector3.zero;
    }

    public void UpdateSlot(ItemData item, int count)
    {
        if (item != null)
        {
            iconImage.sprite = item.icon;
            iconImage.preserveAspect = true;
            iconImage.gameObject.SetActive(true);
            iconImage.enabled = true;
            countText.text = count > 1 ? count.ToString() : "";
            countText.gameObject.SetActive(true);
        }
        else
        {
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);
            iconImage.enabled = false;
            countText.text = "";
            countText.gameObject.SetActive(false);
        }
    }

    public void SetHighlight(bool isActive)
    {
        if (slotImage == null) return;

        if (isActive && selectedSprite != null)
        {
            slotImage.sprite = selectedSprite;
            slotImage.color = Color.white;
        }
        else
        {
            slotImage.sprite = defaultSprite;
            slotImage.color = defaultColor;
        }
    }
}
