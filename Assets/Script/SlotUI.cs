using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SlotUI : MonoBehaviour, IPointerClickHandler, IDropHandler, IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler
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

    private CanvasGroup canvasGroup;
    private Transform iconTransform;
    private Transform parentAfterDrag;
    private bool countWasActive;

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

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (iconImage != null)
        {
            iconTransform = iconImage.transform;
            iconImage.raycastTarget = false;
        }
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

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (iconImage == null || iconImage.sprite == null) return;

        if (isInventorySlot)
        {
            InventoryManager inventory = FindFirstObjectByType<InventoryManager>();
            if (inventory == null || inventory.InventoryPanel == null || !inventory.InventoryPanel.activeSelf)
            {
                eventData.pointerDrag = null;
                return;
            }
        }

        parentAfterDrag = transform;

        iconTransform.SetParent(transform.root);
        iconTransform.SetAsLastSibling();

        countWasActive = countText != null && countText.gameObject.activeSelf;
        if (countText != null)
            countText.gameObject.SetActive(false);

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (iconImage == null || iconImage.sprite == null) return;
        iconTransform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (iconImage == null) return;

        iconTransform.SetParent(parentAfterDrag);
        iconTransform.localPosition = Vector3.zero;

        if (countText != null && countWasActive)
            countText.gameObject.SetActive(true);

        canvasGroup.blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        SlotUI sourceSlot = eventData.pointerDrag.GetComponent<SlotUI>();
        if (sourceSlot == null) return;

        HotbarManager hotbar = FindFirstObjectByType<HotbarManager>();
        InventoryManager inventory = FindFirstObjectByType<InventoryManager>();

        int fromIndex = sourceSlot.SlotIndex;
        int toIndex = slotIndex;

        bool sourceIsHotbar = !sourceSlot.IsInventorySlot;
        bool targetIsHotbar = !isInventorySlot;

        if (sourceIsHotbar && targetIsHotbar)
            StackOrSwapHotbar(hotbar, fromIndex, toIndex);
        else if (!sourceIsHotbar && !targetIsHotbar)
            StackOrSwapInventory(inventory, fromIndex, toIndex);
        else if (sourceIsHotbar && !targetIsHotbar)
            StackOrSwapCross(hotbar, inventory, fromIndex, toIndex, true);
        else
            StackOrSwapCross(hotbar, inventory, toIndex, fromIndex, false);
    }

    private void StackOrSwapHotbar(HotbarManager hotbar, int fromIndex, int toIndex)
    {
        ItemData sourceItem = hotbar.GetItem(fromIndex);
        int sourceCount = hotbar.GetItemCount(fromIndex);
        ItemData targetItem = hotbar.GetItem(toIndex);
        int targetCount = hotbar.GetItemCount(toIndex);

        if (CanStack(sourceItem, targetItem))
        {
            hotbar.SetItem(toIndex, targetItem, targetCount + sourceCount);
            hotbar.SetItem(fromIndex, null, 0);
            hotbar.RefreshUI();
            return;
        }

        hotbar.SwapSlot(fromIndex, toIndex);
        hotbar.RefreshUI();
    }

    private void StackOrSwapInventory(InventoryManager inventory, int fromIndex, int toIndex)
    {
        ItemData sourceItem = inventory.InventoryItems[fromIndex];
        int sourceCount = inventory.ItemCounts[fromIndex];
        ItemData targetItem = inventory.InventoryItems[toIndex];
        int targetCount = inventory.ItemCounts[toIndex];

        if (CanStack(sourceItem, targetItem))
        {
            inventory.SetItem(toIndex, targetItem, targetCount + sourceCount);
            inventory.SetItem(fromIndex, null, 0);
            inventory.RefreshUI();
            return;
        }

        inventory.SwapSlot(fromIndex, toIndex);
        inventory.RefreshUI();
    }

    private void StackOrSwapCross(HotbarManager hotbar, InventoryManager inventory,
                                   int hotbarIndex, int inventoryIndex, bool isHotbarSource)
    {
        ItemData hotbarItem = hotbar.GetItem(hotbarIndex);
        int hotbarCount = hotbar.GetItemCount(hotbarIndex);
        ItemData inventoryItem = inventory.InventoryItems[inventoryIndex];
        int inventoryCount = inventory.ItemCounts[inventoryIndex];

        if (isHotbarSource)
        {
            if (CanStack(hotbarItem, inventoryItem))
            {
                hotbar.SetItem(hotbarIndex, null, 0);
                inventory.SetItem(inventoryIndex, inventoryItem, inventoryCount + hotbarCount);
                hotbar.RefreshUI();
                inventory.RefreshUI();
                return;
            }

            hotbar.SetItem(hotbarIndex, inventoryItem, inventoryCount);
            inventory.SetItem(inventoryIndex, hotbarItem, hotbarCount);
        }
        else
        {
            if (CanStack(inventoryItem, hotbarItem))
            {
                inventory.SetItem(inventoryIndex, null, 0);
                hotbar.SetItem(hotbarIndex, hotbarItem, hotbarCount + inventoryCount);
                hotbar.RefreshUI();
                inventory.RefreshUI();
                return;
            }

            hotbar.SetItem(hotbarIndex, inventoryItem, inventoryCount);
            inventory.SetItem(inventoryIndex, hotbarItem, hotbarCount);
        }

        hotbar.RefreshUI();
        inventory.RefreshUI();
    }

    private bool CanStack(ItemData a, ItemData b)
    {
        if (a == null || b == null) return false;
        if (a != b) return false;
        return a.isStackable;
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

            if (item.itemName == "Wheat" || item.itemName == "Wheat Seed")
                iconImage.rectTransform.localScale = new Vector3(0.65f, 0.65f, 1f);
            else
                iconImage.rectTransform.localScale = Vector3.one;
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
