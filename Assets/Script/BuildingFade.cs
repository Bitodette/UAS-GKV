using UnityEngine;

public class BuildingFade : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private int originalSortOrder;

    [Header("Pengaturan Transparan")]
    public float transparentAlpha = 0.5f;      // 50% transparan
    public float fadeSpeed = 5f;                // kecepatan fade

    [Header("Render Order Saat Player di Belakang")]
    public int buildingSortOrderAbove = 5;      // sorting order pas transparan

    private float targetAlpha = 1f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSortOrder = spriteRenderer.sortingOrder;
    }

    void Update()
    {
        // smooth fade ke target alpha
        Color curColor = spriteRenderer.color;
        curColor.a = Mathf.MoveTowards(curColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
        spriteRenderer.color = curColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        targetAlpha = transparentAlpha;
        spriteRenderer.sortingOrder = buildingSortOrderAbove;  // render di depan player
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        targetAlpha = 1f;
        spriteRenderer.sortingOrder = originalSortOrder;
    }
}
