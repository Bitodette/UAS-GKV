using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private GameObject highlightObj;

    public void UpdateSlot(ItemData item, int count)
    {
        Debug.Log("[SlotUI] UpdateSlot called. item=" + (item != null ? item.itemName : "NULL") + " count=" + count + " | iconImage=" + (iconImage != null) + " countText=" + (countText != null));
        if (item != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
            countText.text = count > 1 ? count.ToString() : "";
            countText.gameObject.SetActive(true);
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
            countText.text = "";
            countText.gameObject.SetActive(false);
        }
    }

    public void ToggleHighlight(bool isActive)
    {
        SetHighlight(isActive);
    }

    public void SetHighlight(bool isActive)
    {
        Debug.Log("[SlotUI] SetHighlight called. isActive=" + isActive + " | highlightObj=" + (highlightObj != null ? highlightObj.name : "NULL"));
        if (highlightObj != null)
        {
            highlightObj.SetActive(isActive);
            Debug.Log("[SlotUI] highlightObj.SetActive(" + isActive + ") called. activeSelf=" + highlightObj.activeSelf + " activeInHierarchy=" + highlightObj.activeInHierarchy);
        }
    }
}
