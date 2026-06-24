using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SaveManager
{
    private static string SaveFilePath => Application.persistentDataPath + "/savegame.json";

    private static readonly Dictionary<string, string> ItemResourceMap = new()
    {
        { "Wood", "Items/Wood" },
        { "Wheat", "Items/Wheat" },
        { "Wheat Seed", "Items/Wheat Seed" },
        { "Kapak", "Items/Kapak" },
        { "tools", "Items/Cangkul" },
        { "water can", "Items/WaterCan" }
    };

    [System.Serializable]
    public class SaveData                              // struktur data save file
    {
        public PlayerPositionData player;
        public int currentDay = 1;
        public ItemSlotData[] hotbar;
        public ItemSlotData[] inventory;
        public CropData[] crops;
        public TreeSaveData[] trees;
    }

    [System.Serializable]
    public class TreeSaveData
    {
        public float x, y;
        public int health;
        public float scale;
    }

    [System.Serializable]
    public class PlayerPositionData
    {
        public float x, y, z;
    }

    [System.Serializable]
    public class ItemSlotData
    {
        public string itemName;
        public int count;
    }

    [System.Serializable]
    public class CropData
    {
        public int x, y;
        public bool isWatered;
        public bool seeded;
        public int growthStage;
    }

    public static void Save()
    {
        var data = new SaveData();

        if (GameManager.Instance != null && GameManager.Instance.player != null)
        {
            Vector3 pos = GameManager.Instance.player.transform.position;
            data.player = new PlayerPositionData { x = pos.x, y = pos.y, z = pos.z };
        }

        var timeAgent = Object.FindObjectOfType<TimeAgent>();
        data.currentDay = timeAgent != null ? timeAgent.currentDay : 1;

        if (GameManager.Instance != null && GameManager.Instance.hotbar != null)
        {
            var hb = GameManager.Instance.hotbar;
            data.hotbar = new ItemSlotData[hb.InventoryItems.Length];
            for (int i = 0; i < hb.InventoryItems.Length; i++)
            {
                if (hb.InventoryItems[i] != null)
                    data.hotbar[i] = new ItemSlotData { itemName = hb.InventoryItems[i].itemName, count = hb.ItemCounts[i] };
                else
                    data.hotbar[i] = null;
            }
        }

        if (GameManager.Instance != null && GameManager.Instance.inventory != null)
        {
            var inv = GameManager.Instance.inventory;
            data.inventory = new ItemSlotData[inv.InventorySize];
            for (int i = 0; i < inv.InventorySize; i++)
            {
                if (inv.InventoryItems[i] != null)
                    data.inventory[i] = new ItemSlotData { itemName = inv.InventoryItems[i].itemName, count = inv.ItemCounts[i] };
                else
                    data.inventory[i] = null;
            }
        }

        var cropsManager = Object.FindObjectOfType<CropsManager>();
        if (cropsManager != null)
        {
            var cropList = new List<CropData>();
            foreach (var kvp in cropsManager.CropsData)
            {
                cropList.Add(new CropData
                {
                    x = kvp.Key.x, y = kvp.Key.y,
                    isWatered = kvp.Value.isWatered,
                    seeded = kvp.Value.seeded,
                    growthStage = kvp.Value.growthStage
                });
            }
            data.crops = cropList.ToArray();
        }

        var treeCuttables = Object.FindObjectsOfType<TreeCuttable>();
        if (treeCuttables.Length > 0)
        {
            var treeList = new List<TreeSaveData>();
            foreach (var tree in treeCuttables)
            {
                Vector3 pos = tree.transform.position;
                treeList.Add(new TreeSaveData
                {
                    x = pos.x, y = pos.y,
                    health = tree.treeHealth,
                    scale = tree.transform.lossyScale.x
                });
            }
            data.trees = treeList.ToArray();
        }

        string json = JsonUtility.ToJson(data, true);     // serialize ke JSON
        File.WriteAllText(SaveFilePath, json);             // tulis ke file
    }

    public static bool HasSaveData()
    {
        return File.Exists(SaveFilePath);
    }

    public static void Load()
    {
        if (!File.Exists(SaveFilePath))
        {
            Debug.Log("[SaveManager] No save file found.");
            return;
        }

        string json = File.ReadAllText(SaveFilePath);
        var data = JsonUtility.FromJson<SaveData>(json);
        if (data == null)
        {
            Debug.LogError("[SaveManager] Failed to deserialize save file.");
            return;
        }

        if (data.player != null && GameManager.Instance != null && GameManager.Instance.player != null)
            GameManager.Instance.player.transform.position = new Vector3(data.player.x, data.player.y, data.player.z);

        var timeAgent = Object.FindObjectOfType<TimeAgent>();
        if (timeAgent != null)
            timeAgent.currentDay = data.currentDay;

        if (data.hotbar != null && GameManager.Instance != null && GameManager.Instance.hotbar != null)
        {
            var hb = GameManager.Instance.hotbar;
            for (int i = 0; i < data.hotbar.Length && i < hb.InventoryItems.Length; i++)
            {
                if (data.hotbar[i] != null && !string.IsNullOrEmpty(data.hotbar[i].itemName))
                {
                    ItemData item = ResolveItem(data.hotbar[i].itemName);
                    hb.SetItem(i, item, data.hotbar[i].count);
                }
                else
                    hb.SetItem(i, null, 0);
            }
            hb.RefreshUI();
        }

        if (data.inventory != null && GameManager.Instance != null && GameManager.Instance.inventory != null)
        {
            var inv = GameManager.Instance.inventory;
            for (int i = 0; i < data.inventory.Length && i < inv.InventorySize; i++)
            {
                if (data.inventory[i] != null && !string.IsNullOrEmpty(data.inventory[i].itemName))
                {
                    ItemData item = ResolveItem(data.inventory[i].itemName);
                    inv.SetItem(i, item, data.inventory[i].count);
                }
                else
                    inv.SetItem(i, null, 0);
            }
            inv.RefreshUI();
        }

        if (data.crops != null)
        {
            var cropsManager = Object.FindObjectOfType<CropsManager>();
            if (cropsManager != null)
            {
                cropsManager.ClearAllCrops();
                foreach (var cropData in data.crops)
                {
                    Vector3Int pos = new Vector3Int(cropData.x, cropData.y, 0);
                    Crops crop = new Crops
                    {
                        isWatered = cropData.isWatered,
                        seeded = cropData.seeded,
                        growthStage = cropData.growthStage
                    };
                    cropsManager.RestoreCrop(pos, crop);
                }
            }
        }

        if (data.trees != null && data.trees.Length > 0)
        {
            var treeSpawner = Object.FindObjectOfType<TreeSpawner>();
            if (treeSpawner != null)
                treeSpawner.RestoreFromSave(data.trees);
        }
    }

    // cari item dari Resources/Items/ berdasarkan nama
    private static ItemData ResolveItem(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;

        ItemData item = Resources.Load<ItemData>("Items/" + itemName);
        if (item != null) return item;

        if (ItemResourceMap.TryGetValue(itemName, out string path))
        {
            item = Resources.Load<ItemData>(path);
            if (item != null) return item;
        }

        Debug.LogWarning("[SaveManager] Item '" + itemName + "' not found!");
        return null;
    }

    public static void DeleteSave()
    {
        if (File.Exists(SaveFilePath))
            File.Delete(SaveFilePath);
    }
}
