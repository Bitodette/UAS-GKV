using UnityEngine;

public class HarvestController : MonoBehaviour
{
    [SerializeField] private ItemData wheatItem;
    [SerializeField] private int minDrop = 1;
    [SerializeField] private int maxDrop = 3;

    private TilemapReadController tilemapReadController;
    private CropsManager cropsManager;

    void Start()
    {
        tilemapReadController = FindFirstObjectByType<TilemapReadController>();
        cropsManager = FindFirstObjectByType<CropsManager>();
    }

    void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (tilemapReadController == null || cropsManager == null) return;

        Vector3Int gridPos = tilemapReadController.GetMouseGridPosition();
        if (!cropsManager.IsFullyGrown(gridPos)) return;

        if (cropsManager.Harvest(gridPos) && wheatItem != null && GameManager.Instance != null)
        {
            int count = Random.Range(minDrop, maxDrop + 1);
            GameManager.Instance.AddItem(wheatItem, count);
        }
    }
}
