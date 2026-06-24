using UnityEngine;

// VERSI LAMA — diganti sama PlayerController yang lebih lengkap
public class PlayerDirection : MonoBehaviour
{
    [Header("Pengaturan Pergerakan")]
    public float moveSpeed = 5f;

    private Animator anim;
    private Rigidbody2D rb; 
    private Vector2 movement;
    private string currentAnimation = "";
    private Camera mainCam;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>(); 
        mainCam = Camera.main;
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        if (Input.GetMouseButtonDown(0))           // klik kiri → ubah arah hadap
        {
            UpdateFacingDirection();
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    // tentukan arah hadap dari posisi mouse relatif ke player
    void UpdateFacingDirection()
    {
        Vector3 mousePosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = mousePosition - transform.position;
        if (aimDirection.magnitude > 3f) return;

        if (Mathf.Abs(aimDirection.x) > Mathf.Abs(aimDirection.y))
        {
            if (aimDirection.x > 0) ChangeAnimationState("idle-kanan");
            else ChangeAnimationState("idle-kiri");
        }
        else
        {
            if (aimDirection.y > 0) ChangeAnimationState("idle-belakang");
            else ChangeAnimationState("idle-depan");
        }
    }

    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation) return;
        anim.Play(newAnimation);
        currentAnimation = newAnimation;
    }
}
