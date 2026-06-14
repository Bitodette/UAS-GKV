using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Pengaturan Pergerakan")]
    public float moveSpeed = 5f;

    private Animator anim;
    private Rigidbody2D rb; 
    public Vector2 movement;
    private string currentAnimation = "";

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>(); 
    }

    void Update()
    {
        // Menangkap input keyboard (WASD / Panah)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // Logika animasi dengan prioritas Horizontal (Kiri/Kanan)
        if (movement.x != 0 || movement.y != 0)
        {
            if (Input.GetKey(KeyCode.A))
            {
                ChangeAnimationState("idle-kiri");
            }
            else if (Input.GetKey(KeyCode.D))
            {
                ChangeAnimationState("idle-kanan");
            }
            else if (Input.GetKey(KeyCode.W))
            {
                ChangeAnimationState("idle-belakang");
            }
            else if (Input.GetKey(KeyCode.S))
            {
                ChangeAnimationState("idle-depan");
            }
        }
    }

    void FixedUpdate()
    {
        // Pergerakan fisika tetap berjalan ke semua arah (termasuk serong)
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation) return;

        anim.Play(newAnimation);
        currentAnimation = newAnimation;
    }
}