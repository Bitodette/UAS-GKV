using UnityEngine;

public class HarvestController : MonoBehaviour
{
    [SerializeField] private GameObject wheatPrefab;
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

        Vector3 worldPos = tilemapReadController.GridToWorldCenter(gridPos);
        worldPos.z = 0;

        if (cropsManager.Harvest(gridPos) && wheatPrefab != null)
        {
            int count = Random.Range(minDrop, maxDrop + 1);
            for (int i = 0; i < count; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
                Instantiate(wheatPrefab, worldPos + offset, Quaternion.identity);
            }
        }
    }
}
