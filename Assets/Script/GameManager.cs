using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;              // singleton biar diakses script lain
    private void Awake()
    {
        Instance = this;
        if (hotbar == null) hotbar = FindFirstObjectByType<HotbarManager>();
        if (inventory == null) inventory = FindFirstObjectByType<InventoryManager>();
    }

    private void Start()
    {
        if (SaveManager.HasSaveData())
            StartCoroutine(DelayedLoad());           // load data kalo ada save
    }

    private IEnumerator DelayedLoad()
    {
        yield return null;                           // tunggu 1 frame biar semua manager siap
        SaveManager.Load();
    }

    public GameObject player;
    public HotbarManager hotbar;
    public InventoryManager inventory;

    public bool AddItem(ItemData item, int count = 1)
    {
        bool added = hotbar != null && hotbar.AddItem(item, count);      // masukin ke hotbar dulu
        if (!added)
            added = inventory != null && inventory.AddItem(item, count); // kalo penuh, masukin inventory
        return added;
    }
}
