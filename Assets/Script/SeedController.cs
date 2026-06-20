using UnityEngine;

public class SeedController : MonoBehaviour
{
    private TilemapReadController tilemapReadController;
    private CropsManager cropsManager;
    private HotbarManager hotbarManager;

    void Start()
    {
        tilemapReadController = FindFirstObjectByType<TilemapReadController>();
        cropsManager = FindFirstObjectByType<CropsManager>();
        hotbarManager = FindFirstObjectByType<HotbarManager>();
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (tilemapReadController == null || !tilemapReadController.IsSeedSelected()) return;

        Vector3Int gridPos = tilemapReadController.GetMouseGridPosition();
        if (!tilemapReadController.CanSeedAt(gridPos)) return;

        if (cropsManager != null)
            cropsManager.Seed(gridPos);
        if (hotbarManager != null)
            hotbarManager.ConsumeItem(hotbarManager.SelectedIndex, 1);
    }
}
