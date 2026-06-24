using UnityEngine;

public class TreeFade : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private int originalSortOrder;

    [Header("Pengaturan Transparan")]
    public float transparentAlpha = 0.5f;      // 50% transparan
    public float fadeSpeed = 5f;                // kecepatan fade

    [Header("Leaf Overlay")]
    public int treeSortOrderAbove = 5;          // sorting order pas transparan

    private float targetAlpha = 1f;             // target alpha (1 = opaque)

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalSortOrder = spriteRenderer.sortingOrder;
    }

    void Update()
    {
        // smooth fade ke target alpha pake MoveTowards
        Color curColor = spriteRenderer.color;
        curColor.a = Mathf.MoveTowards(curColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
        spriteRenderer.color = curColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        targetAlpha = transparentAlpha;                    // jadi transparan
        spriteRenderer.sortingOrder = treeSortOrderAbove;  // render di depan player
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        targetAlpha = 1f;                                    // balikin opaque
        spriteRenderer.sortingOrder = originalSortOrder;     // balikin order asli
    }
}
