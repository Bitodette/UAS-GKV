using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SlotbarManager : MonoBehaviour
{
    public static SlotbarManager Instance;

    [Header("Slotbar Settings")]
    public int slotCount = 9;
    public float spacing = 4f;
    public float padding = 6f;

    private List<Image> slotIcons = new List<Image>();
    private List<Text> slotTexts = new List<Text>();
    private List<string> slotItemNames = new List<string>();
    private List<int> slotCounts = new List<int>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        RectTransform slotbarRect = GetComponent<RectTransform>();

        float barWidth = slotbarRect.rect.width;
        float barHeight = slotbarRect.rect.height;

        float availableWidth = barWidth - padding * 2;
        float slotSize = (availableWidth - (slotCount - 1) * spacing) / slotCount;
        float slotHeight = Mathf.Min(slotSize, barHeight - padding * 2);

        float totalSlotsWidth = slotCount * slotSize + (slotCount - 1) * spacing;
        float startX = -totalSlotsWidth / 2f + slotSize / 2f;

        for (int i = 0; i < slotCount; i++)
        {
            GameObject slotObj = new GameObject("Slot_" + i);
            slotObj.transform.SetParent(transform, false);

            RectTransform slotRt = slotObj.AddComponent<RectTransform>();
            slotRt.sizeDelta = new Vector2(slotSize, slotHeight);
            slotRt.anchoredPosition = new Vector2(startX + i * (slotSize + spacing), 0);
            slotRt.localScale = Vector3.one;

            Image slotBg = slotObj.AddComponent<Image>();
            slotBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            GameObject iconObj = new GameObject("Icon");
            iconObj.transform.SetParent(slotObj.transform, false);

            RectTransform iconRt = iconObj.AddComponent<RectTransform>();
            float iconSize = slotSize * 0.75f;
            iconRt.sizeDelta = new Vector2(iconSize, iconSize);
            iconRt.anchoredPosition = Vector2.zero;
            iconRt.localScale = Vector3.one;

            Image icon = iconObj.AddComponent<Image>();
            icon.raycastTarget = false;
            icon.preserveAspect = true;
            icon.enabled = false;

            GameObject countObj = new GameObject("Count");
            countObj.transform.SetParent(slotObj.transform, false);

            RectTransform countRt = countObj.AddComponent<RectTransform>();
            countRt.anchorMin = Vector2.zero;
            countRt.anchorMax = Vector2.one;
            countRt.offsetMin = new Vector2(2, 2);
            countRt.offsetMax = new Vector2(-2, -2);
            countRt.localScale = Vector3.one;

            Text countText = countObj.AddComponent<Text>();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            countText.font = font;
            countText.fontSize = Mathf.RoundToInt(slotSize * 0.3f);
            countText.fontStyle = FontStyle.Bold;
            countText.alignment = TextAnchor.LowerRight;
            countText.color = Color.white;
            countText.text = "";
            countObj.SetActive(false);

            slotIcons.Add(icon);
            slotTexts.Add(countText);
            slotItemNames.Add("");
            slotCounts.Add(0);
        }
    }

    public bool AddItem(Sprite itemSprite, string itemName)
    {
        int count = slotItemNames.Count;
        if (count == 0) return false;

        for (int i = 0; i < count; i++)
        {
            if (slotItemNames[i] == itemName)
            {
                slotCounts[i]++;
                UpdateSlotVisual(i);
                return true;
            }
        }

        for (int i = 0; i < count; i++)
        {
            if (slotItemNames[i] == "")
            {
                slotItemNames[i] = itemName;
                slotCounts[i] = 1;
                slotIcons[i].sprite = itemSprite;
                slotIcons[i].enabled = true;
                UpdateSlotVisual(i);
                return true;
            }
        }

        return false;
    }

    void UpdateSlotVisual(int index)
    {
        slotTexts[index].text = slotCounts[index].ToString();
        slotTexts[index].gameObject.SetActive(true);
    }
}
