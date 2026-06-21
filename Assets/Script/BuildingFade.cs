using UnityEngine;

public class BuildingFade : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private int originalSortOrder;

    [Header("Pengaturan Transparan")]
    public float transparentAlpha = 0.5f;
    public float fadeSpeed = 5f;

    [Header("Render Order Saat Player di Belakang")]
    public int buildingSortOrderAbove = 5;

    private float targetAlpha = 1f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSortOrder = spriteRenderer.sortingOrder;
    }

    void Update()
    {
        Color curColor = spriteRenderer.color;
        curColor.a = Mathf.MoveTowards(curColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
        spriteRenderer.color = curColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        targetAlpha = transparentAlpha;
        spriteRenderer.sortingOrder = buildingSortOrderAbove;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        targetAlpha = 1f;
        spriteRenderer.sortingOrder = originalSortOrder;
    }
}
