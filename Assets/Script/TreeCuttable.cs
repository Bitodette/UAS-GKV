using System.Collections;
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
    public Transform logSpawnPoint;

    [Header("Hit Animation")]
    public Sprite[] hitAnimationSprites;
    public float hitAnimSpeed = 0.08f;

    public bool healthSetBySave = false;
    private bool isDestroyed = false;
    private bool isHitAnimating = false;
    private Coroutine hitAnimCoroutine;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        if (wholeTreeObject == null)
            wholeTreeObject = transform.parent.gameObject;

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (!healthSetBySave)
            treeHealth = Random.Range(minHealth, maxHealth + 1);
    }

    public float GetHitAnimDuration()
    {
        if (hitAnimationSprites == null || hitAnimationSprites.Length < 2) return 0f;
        return (hitAnimationSprites.Length - 1) * hitAnimSpeed;
    }

    public override void Hit()
    {
        if (isDestroyed || isHitAnimating) return;
        isHitAnimating = true;
        PlayHitAnimation();
        TakeDamage();
    }

    void PlayHitAnimation()
    {
        if (hitAnimationSprites == null || hitAnimationSprites.Length < 2 || spriteRenderer == null) return;

        if (hitAnimCoroutine != null)
            StopCoroutine(hitAnimCoroutine);

        hitAnimCoroutine = StartCoroutine(HitAnimRoutine());
    }

    IEnumerator HitAnimRoutine()
    {
        spriteRenderer.sprite = hitAnimationSprites[0];

        yield return new WaitForSeconds(hitAnimSpeed / 2f);

        for (int i = 1; i < hitAnimationSprites.Length; i++)
        {
            spriteRenderer.sprite = hitAnimationSprites[i];
            yield return new WaitForSeconds(hitAnimSpeed);
        }

        spriteRenderer.sprite = hitAnimationSprites[0];

        hitAnimCoroutine = null;
        isHitAnimating = false;
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

        Vector3 spawnPos = logSpawnPoint != null ? logSpawnPoint.position : transform.position;

        for (int i = 0; i < count; i++)
        {
            if (woodPrefabs == null || woodPrefabs.Length == 0) break;
            Vector3 spawnOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
            GameObject prefab = woodPrefabs[Random.Range(0, woodPrefabs.Length)];
            Instantiate(prefab, spawnPos + spawnOffset, Quaternion.identity);
        }

        Destroy(wholeTreeObject);
    }
}
