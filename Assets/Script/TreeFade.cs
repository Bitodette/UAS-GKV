using UnityEngine;

public class TreeFade : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private int originalSortOrder;

    [Header("Pengaturan Transparan")]
    public float transparentAlpha = 0.5f;
    public float fadeSpeed = 5f;

    [Header("Leaf Overlay")]
    public int treeSortOrderAbove = 5;

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
        spriteRenderer.sortingOrder = treeSortOrderAbove;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        targetAlpha = 1f;
        spriteRenderer.sortingOrder = originalSortOrder;
    }
}
