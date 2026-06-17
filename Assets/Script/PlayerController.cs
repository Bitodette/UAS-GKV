using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Pengaturan Pergerakan")]
    public float moveSpeed = 5f;

    private Animator anim;
    private Rigidbody2D rb; 
    public Vector2 movement;
    private string currentAnimation = "";
    
    // Menyimpan arah terakhir agar tahu harus idle ke arah mana saat berhenti
    private string lastDirection = "depan"; 
    
    private Camera mainCam;
    private bool isUsingTool = false;
    private float toolTimer = 0f;

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

        if (isUsingTool)
        {
            toolTimer -= Time.deltaTime;
            if (toolTimer <= 0f)
                isUsingTool = false;
        }

        // 1. CEK KLIK MOUSE
        if (Input.GetMouseButtonDown(0))
        {
            UpdateMouseDirection();
        }

        // 2. LOGIKA SAAT BERGERAK (WASD)
        if (movement != Vector2.zero) 
        {
            if (movement.x < 0) 
            {
                ChangeAnimationState("jalan-kiri");
                lastDirection = "kiri";
            }
            else if (movement.x > 0) 
            {
                ChangeAnimationState("jalan-kanan");
                lastDirection = "kanan";
            }
            else if (movement.y > 0) 
            {
                ChangeAnimationState("jalan-atas");
                lastDirection = "belakang"; 
            }
            else if (movement.y < 0) 
            {
                ChangeAnimationState("jalan-bawah");
                lastDirection = "depan"; 
            }
        }
        // 3. LOGIKA SAAT DIAM
        else 
        {
            if (lastDirection == "kiri") ChangeAnimationState("idle-kiri");
            else if (lastDirection == "kanan") ChangeAnimationState("idle-kanan");
            else if (lastDirection == "belakang") ChangeAnimationState("idle-belakang");
            else if (lastDirection == "depan") ChangeAnimationState("idle-depan");
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void UpdateMouseDirection()
    {
        Vector3 mousePosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 aimDirection = mousePosition - transform.position;

        if (Mathf.Abs(aimDirection.x) > Mathf.Abs(aimDirection.y))
        {
            if (aimDirection.x > 0)
            {
                lastDirection = "kanan";
                ChangeAnimationState("nyangkul-kanan");
                isUsingTool = true;
                toolTimer = 0.35f;
                return;
            }
            else lastDirection = "kiri";
        }
        else
        {
            if (aimDirection.y > 0) lastDirection = "belakang";
            else lastDirection = "depan";
        }
        
        if (movement == Vector2.zero)
        {
            ChangeAnimationState("idle-" + lastDirection);
        }
    }

    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation) return;

        anim.Play(newAnimation);
        currentAnimation = newAnimation;
    }
}