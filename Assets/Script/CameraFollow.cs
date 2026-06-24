using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Vector3 offset = new Vector3(0, 0, -10);   // offset kamera (-10 z biar bisa lihat)
    public float smoothSpeed = 5f;

    private Transform target;

    void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.player != null)
            target = GameManager.Instance.player.transform;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // smooth follow pake Lerp
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
