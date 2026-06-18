using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IInitializePotentialDragHandler
{
    [HideInInspector] public Transform parentAfterDrag;
    private Image image;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        image = GetComponent<Image>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (image.sprite == null) return;

        InventoryManager inventory = FindFirstObjectByType<InventoryManager>();
        if (inventory == null || inventory.InventoryPanel == null || !inventory.InventoryPanel.activeSelf)
        {
            eventData.pointerDrag = null;
            return;
        }

        parentAfterDrag = transform.parent;

        transform.SetParent(transform.root);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (image.sprite == null) return;
        transform.position = Input.mousePosition; // Ikon mengikuti mouse
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (image.sprite == null) return;

        transform.SetParent(parentAfterDrag);
        transform.localPosition = Vector3.zero;
        
        canvasGroup.blocksRaycasts = true;
    }

    public void OnDrop(PointerEventData eventData)
    {
        SlotUI slot = GetComponentInParent<SlotUI>();
        if (slot != null)
            slot.OnDrop(eventData);
    }
}