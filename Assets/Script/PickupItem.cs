using UnityEngine;

public class PickupItem : MonoBehaviour
{
    Transform player;
    [SerializeField] float speed = 5f;
    [SerializeField] float pickupRange = 1f;
    [SerializeField] float timeToLive = 10f;

    [Header("Item Info")]
    public Sprite itemIcon;
    public string itemName = "Wood";

    private void Start()
    {
        player = GameManager.Instance.player.transform;
        Destroy(gameObject, timeToLive);
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > pickupRange) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            speed * Time.deltaTime
        );

        if (distance < 0.1f)
        {
            if (SlotbarManager.Instance != null)
                SlotbarManager.Instance.AddItem(itemIcon, itemName);
            Destroy(gameObject);
        }
    }
}
