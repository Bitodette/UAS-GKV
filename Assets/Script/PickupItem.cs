using UnityEngine;

public class PickupItem : MonoBehaviour
{
    Transform player;
    [SerializeField] float speed = 5f;
    [SerializeField] float pickupRange = 1f;
    [SerializeField] float timeToLive = 30f;

    [Header("Item Info")]
    public ItemData itemData;
    public Sprite itemIcon;
    public string itemName = "Wood";

    private void Start()
    {
        Debug.Log("[PickupItem] Start. GameManager.Instance=" + (GameManager.Instance != null) + " player=" + (GameManager.Instance != null ? GameManager.Instance.player : "NULL") + " itemData=" + (itemData != null ? itemData.itemName : "NULL"));
        if (GameManager.Instance == null) { Debug.LogError("[PickupItem] GameManager.Instance is NULL!"); return; }
        if (GameManager.Instance.player == null) { Debug.LogError("[PickupItem] GameManager.Instance.player is NULL!"); return; }
        player = GameManager.Instance.player.transform;
        Destroy(gameObject, timeToLive);
    }

    private void Update()
    {
        if (player == null) { Debug.LogWarning("[PickupItem] player is null!"); return; }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > pickupRange) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            speed * Time.deltaTime
        );

        if (distance < 0.1f)
        {
            Debug.Log("[PickupItem] Picked up! itemData=" + (itemData != null ? itemData.itemName : "NULL"));
            HotbarManager hotbar = FindFirstObjectByType<HotbarManager>();
            Debug.Log("[PickupItem] HotbarManager found=" + (hotbar != null));
            if (hotbar != null && itemData != null)
                hotbar.AddItem(itemData);
            Destroy(gameObject);
        }
    }
}
