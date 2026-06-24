using UnityEngine;

public class PickupItem : MonoBehaviour
{
    Transform player;
    [SerializeField] float speed = 5f;
    [SerializeField] float pickupRange = 1f;
    [SerializeField] float timeToLive = 30f;          // ilang otomatis setelah 30 detik

    [Header("Item Info")]
    public ItemData itemData;
    public Sprite itemIcon;
    public string itemName = "Wood";

    private void Start()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.player == null) return;
        player = GameManager.Instance.player.transform;
        Destroy(gameObject, timeToLive);               // auto destroy kalo gak diambil
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > pickupRange) return;             // di luar jangkauan

        transform.position = Vector3.MoveTowards(       // lerp ke player
            transform.position,
            player.position,
            speed * Time.deltaTime
        );

        if (distance < 0.1f)
        {
            if (itemData != null)
                GameManager.Instance.AddItem(itemData); // masukin inventory
            Destroy(gameObject);
        }
    }
}
