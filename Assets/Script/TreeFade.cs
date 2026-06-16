using UnityEngine;

public class TreeFade : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Header("Pengaturan Transparan")]
    public float transparentAlpha = 0.5f;
    public float fadeSpeed = 5f;

    private float targetAlpha = 1f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        Color curColor = spriteRenderer.color;
        curColor.a = Mathf.MoveTowards(curColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
        spriteRenderer.color = curColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            targetAlpha = transparentAlpha;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            targetAlpha = 1f;
        }
    }
}
