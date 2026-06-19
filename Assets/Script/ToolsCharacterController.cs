using UnityEngine;
public class ToolsCharacterController : MonoBehaviour
{
    Rigidbody2D rb;

    [SerializeField] float offsetDistance = 1f;
    [SerializeField] float sizeOfInteractableArea = 1.2f;
    [SerializeField] float maxUseDistance = 1.5f;

    Camera mainCam;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            UseToolWorld();
        }
    }

    private void UseToolWorld()
    {
        Vector3 mousePosition =
            mainCam.ScreenToWorldPoint(Input.mousePosition);

        mousePosition.z = 0;

        float dist =
            Vector2.Distance(mousePosition, transform.position);

        if (dist > maxUseDistance)
            return;

        HotbarManager hotbar =
            FindFirstObjectByType<HotbarManager>();

        if (hotbar != null && hotbar.SelectedItem != null)
        {
            Debug.Log(
                "Using item: " +
                hotbar.SelectedItem.itemName
            );
        }

        Vector2 aimDirection =
            mousePosition - transform.position;

        Vector2 direction;

        if (Mathf.Abs(aimDirection.x) > Mathf.Abs(aimDirection.y))
            direction = new Vector2(Mathf.Sign(aimDirection.x), 0);
        else
            direction = new Vector2(0, Mathf.Sign(aimDirection.y));

        Vector2 position =
            rb.position + offsetDistance * direction;

        Collider2D[] colliders =
            Physics2D.OverlapCircleAll(
                position,
                sizeOfInteractableArea
            );

        foreach (Collider2D collider in colliders)
        {
            ToolHit hit =
                collider.GetComponent<ToolHit>();

            if (hit != null)
            {
                hit.Hit();
            }
        }
    }
}
