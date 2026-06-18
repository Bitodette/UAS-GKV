using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class SlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;

    private Image slotImage;
    private Sprite defaultSprite;
    private Color defaultColor;
    private Sprite selectedSprite;
    private int slotIndex;

    void Awake()
    {
        slotImage = GetComponent<Image>();
        if (slotImage != null)
        {
            defaultSprite = slotImage.sprite;
            defaultColor = slotImage.color;
        }

        Transform hl = transform.Find("HighLight");
        if (hl != null)
            hl.gameObject.SetActive(false);

        if (selectedSprite == null)
        {
            Sprite[] all = Resources.FindObjectsOfTypeAll<Sprite>();
            foreach (Sprite s in all)
            {
                if (s.name == "slotbar 1_1")
                {
                    selectedSprite = s;
                    break;
                }
            }
        }
    }

    public void SetSlotIndex(int index)
    {
        slotIndex = index;
    }

    public void SetSelectedSprite(Sprite sprite)
    {
        if (sprite != null)
            selectedSprite = sprite;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        HotbarManager hotbar = FindFirstObjectByType<HotbarManager>();
        if (hotbar != null)
            hotbar.SetSelected(slotIndex);
    }

    public void UpdateSlot(ItemData item, int count)
    {
        Debug.Log("[SlotUI] UpdateSlot called. item=" + (item != null ? item.itemName : "NULL") + " count=" + count + " | iconImage=" + (iconImage != null) + " countText=" + (countText != null));
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
