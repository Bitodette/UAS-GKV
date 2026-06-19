using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private void Awake()
    {
        Instance = this;
        if (hotbar == null) hotbar = FindFirstObjectByType<HotbarManager>();
        if (inventory == null) inventory = FindFirstObjectByType<InventoryManager>();
    }

    public GameObject player;
    public HotbarManager hotbar;
    public InventoryManager inventory;

    public bool AddItem(ItemData item, int count = 1)
    {
        bool added = hotbar != null && hotbar.AddItem(item, count);
        if (!added)
            added = inventory != null && inventory.AddItem(item, count);
        return added;
    }
}
