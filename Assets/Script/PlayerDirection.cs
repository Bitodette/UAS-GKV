using UnityEngine;

public class PlayerDirection : MonoBehaviour
{
    [Header("Pengaturan Pergerakan")]
    public float moveSpeed = 5f;

    private Animator anim;
    private Rigidbody2D rb; 
    private Vector2 movement;
    private string currentAnimation = "";
    
    // Kamera utama untuk mendeteksi posisi mouse
    private Camera mainCam;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>(); 
        mainCam = Camera.main; // Mengambil referensi kamera
    }

    void Update()
    {
        // 1. INPUT PERGERAKAN (Tetap menggunakan WASD)
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // 2. LOGIKA ARAH WAJAH (Hanya saat klik kiri mouse)
        if (Input.GetMouseButtonDown(0))
        {
            UpdateFacingDirection();
        }
    }

    void FixedUpdate()
    {
        // Eksekusi pergerakan fisika (WASD)
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void UpdateFacingDirection()
    {
        // Mengubah posisi mouse di layar menjadi posisi di dalam dunia game
        Vector3 mousePosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
        
        // Mencari vektor arah dari player ke mouse
        Vector2 aimDirection = mousePosition - transform.position;
        if (aimDirection.magnitude > 3f) return;

        // Membandingkan jarak X dan Y untuk menentukan arah mana yang lebih dominan
        if (Mathf.Abs(aimDirection.x) > Mathf.Abs(aimDirection.y))
        {
            // Jika kursor lebih jauh ke kiri atau kanan
            if (aimDirection.x > 0)
            {
                ChangeAnimationState("idle-kanan");
            }
            else
            {
                ChangeAnimationState("idle-kiri");
            }
        }
        else
        {
            // Jika kursor lebih jauh ke atas atau bawah
            if (aimDirection.y > 0)
            {
                ChangeAnimationState("idle-belakang");
            }
            else
            {
                ChangeAnimationState("idle-depan");
            }
        }
    }

    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation) return;

        anim.Play(newAnimation);
        currentAnimation = newAnimation;
    }
}