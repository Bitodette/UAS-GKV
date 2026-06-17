using UnityEngine;

public class TreeSpawner : MonoBehaviour
{
    [Header("Tree Settings")]
    public GameObject treeTemplate;
    public int treeCount = 10;
    public Vector2 spawnArea = new Vector2(30f, 30f);
    public float minDistanceBetweenTrees = 2f;

    [Header("Random Drops")]
    public GameObject[] dropPrefabs;

    void Start()
    {
        if (treeTemplate != null)
            treeTemplate.SetActive(false);

        SpawnTrees();
    }

    void SpawnTrees()
    {
        for (int i = 0; i < treeCount; i++)
        {
            Vector3? pos = GetRandomPosition();
            if (pos == null) continue;

            GameObject newTree = Instantiate(treeTemplate, pos.Value, Quaternion.identity);
            newTree.SetActive(true);

            float scale = Random.Range(0.8f, 1.3f);
            newTree.transform.localScale = new Vector3(scale, scale, 1);

            TreeCuttable[] cuttables = newTree.GetComponentsInChildren<TreeCuttable>();
            foreach (TreeCuttable c in cuttables)
            {
                if (dropPrefabs != null && dropPrefabs.Length > 0)
                    c.woodPrefabs = dropPrefabs;
                c.minDrop = 2;
                c.maxDrop = 5;
                c.minHealth = 2;
                c.maxHealth = 5;
            }
        }
    }

    Vector3? GetRandomPosition()
    {
        for (int attempt = 0; attempt < 30; attempt++)
        {
            float x = Random.Range(-spawnArea.x / 2, spawnArea.x / 2);
            float y = Random.Range(-spawnArea.y / 2, spawnArea.y / 2);
            Vector3 pos = new Vector3(x, y, 0);

            Collider2D[] hits = Physics2D.OverlapCircleAll(pos, minDistanceBetweenTrees);
            bool overlap = false;
            foreach (Collider2D hit in hits)
            {
                if (hit.GetComponentInParent<TreeCuttable>() != null)
                {
                    overlap = true;
                    break;
                }
            }

            if (!overlap)
                return pos;
        }
        return null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.15f);
        Gizmos.DrawCube(Vector3.zero, new Vector3(spawnArea.x, spawnArea.y, 1));
    }
}
