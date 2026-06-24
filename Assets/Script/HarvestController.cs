using UnityEngine;

public class HarvestController : MonoBehaviour
{
    [SerializeField] private ItemData wheatItem;       // item gandum yg dihasilkan
    [SerializeField] private int minDrop = 1;
    [SerializeField] private int maxDrop = 3;

    private TilemapReadController tilemapReadController;
    private CropsManager cropsManager;

    void Start()
    {
        tilemapReadController = FindFirstObjectByType<TilemapReadController>();
        cropsManager = FindFirstObjectByType<CropsManager>();
        if (wheatItem == null)
            wheatItem = Resources.Load<ItemData>("Items/Wheat");
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (tilemapReadController == null || cropsManager == null) return;

        Vector3Int gridPos = tilemapReadController.GetMouseGridPosition();
        if (!cropsManager.IsFullyGrown(gridPos)) return;     // cuma bisa panen yg udah matang

        if (cropsManager.Harvest(gridPos) && wheatItem != null && GameManager.Instance != null)
        {
            int count = Random.Range(minDrop, maxDrop + 1);  // random jumlah panen
            GameManager.Instance.AddItem(wheatItem, count);
        }
    }
}
