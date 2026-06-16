using UnityEngine;

public class TreeCuttable : MonoBehaviour
{
    [Header("Statistik Pohon")]
    public int treeHealth = 3;
    public GameObject woodPrefab;
    public int dropCount = 3;

    [Header("Referensi")]
    public GameObject wholeTreeObject;

    private bool isDestroyed = false;

    void Start()
    {
        if (wholeTreeObject == null)
            wholeTreeObject = transform.parent.gameObject;
    }


    void OnMouseDown()
    {
        if (isDestroyed) return;

        GameObject player = GameManager.Instance.player;
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance > 2f) return;
        }

        TakeDamage();
    }

    void TakeDamage()
    {
        treeHealth--;

        wholeTreeObject.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
        Invoke("ResetScale", 0.08f);

        if (treeHealth <= 0)
        {
            DestroyTree();
        }
    }

    void ResetScale()
    {
        if (wholeTreeObject != null)
        {
            wholeTreeObject.transform.localScale = Vector3.one;
        }
    }

    void DestroyTree()
    {
        isDestroyed = true;

        for (int i = 0; i < dropCount; i++)
        {
            Vector3 spawnOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
            Instantiate(woodPrefab, transform.position + spawnOffset, Quaternion.identity);
        }

        Destroy(wholeTreeObject);
    }
}
