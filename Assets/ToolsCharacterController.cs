using UnityEngine;

public class ToolsCharacterController : MonoBehaviour
{
    PlayerController character;
    Rigidbody2D rb;

    [SerializeField] float offsetDistance = 1f;
    [SerializeField] float sizeOfInteractableArea = 1.2f;

    void Start()
    {
        character = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            UseTool();
        }
    }

    private void UseTool()
    {
        Vector2 position = rb.position + offsetDistance * character.movement.normalized;

        Collider2D[] colliders =
            Physics2D.OverlapCircleAll(position, sizeOfInteractableArea);

        foreach (Collider2D collider in colliders)
        {
            ToolHit hit = collider.GetComponent<ToolHit>();

            if (hit != null)
            {
                hit.Hit();
                break;
            }
        }
    }
}