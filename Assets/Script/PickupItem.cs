using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    Transform player;
    [SerializeField] float speed = 5f;
    [SerializeField] float pickupRange = 1f;
    [SerializeField] float timeToLive = 10f;

    private void Start()
{
    player = GameObject.Find("Player").transform;

    Debug.Log("PLAYER FOUND = " + player);
}

    private void Update()
    {
        Debug.Log("player = " + player);

        if (player == null)
        {
            Debug.LogError("player NULL");
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > pickupRange)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            player.position,
            speed * Time.deltaTime
        );

        if (distance < 0.1f)
        {
            Destroy(gameObject);
        }
    }
}
