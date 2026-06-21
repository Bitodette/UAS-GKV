using UnityEngine;

public class TreeCuttable : ToolHit
{
    [Header("Statistik Pohon")]
    public int treeHealth = 3;
    public int minHealth = 2;
    public int maxHealth = 5;
    public GameObject[] woodPrefabs;
    public int minDrop = 2;
    public int maxDrop = 5;

    [Header("Referensi")]
    public GameObject wholeTreeObject;

    private bool isDestroyed = false;

    void Start()
    {
        if (wholeTreeObject == null)
            wholeTreeObject = transform.parent.gameObject;

        treeHealth = Random.Range(minHealth, maxHealth + 1);
    }

    public override void Hit()
    {
        if (isDestroyed) return;
        TakeDamage();
    }

    void TakeDamage()
    {
        treeHealth--;

        if (treeHealth <= 0)
        {
            DestroyTree();
        }
    }

    void DestroyTree()
    {
        isDestroyed = true;

        int count = Random.Range(minDrop, maxDrop + 1);

        for (int i = 0; i < count; i++)
        {
            if (woodPrefabs == null || woodPrefabs.Length == 0) break;
            Vector3 spawnOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
            GameObject prefab = woodPrefabs[Random.Range(0, woodPrefabs.Length)];
            Instantiate(prefab, transform.position + spawnOffset, Quaternion.identity);
        }

        Destroy(wholeTreeObject);
    }
}
