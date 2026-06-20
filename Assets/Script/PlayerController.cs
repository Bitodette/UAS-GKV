using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Pengaturan Pergerakan")]
    public float moveSpeed = 5f;

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb; 
    public Vector2 movement;
    private string currentAnimation = "";
    
    private string lastDirection = "depan"; 
    
    private Camera mainCam;
    private TilemapReadController tilemapReadController;
    private CropsManager cropsManager;
    private bool isUsingTool = false;
    private float toolTimer = 0f;
    private bool toolFirstFrame = true;

    private bool isWalkingToTarget = false;
    private Vector3 walkTarget;
    private Vector3Int walkIntermediateGrid;
    private Vector3Int walkFinalGrid;

    private Vector3Int? pendingPlowPos;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); 
        mainCam = Camera.main; 
        tilemapReadController = FindFirstObjectByType<TilemapReadController>();
        cropsManager = FindFirstObjectByType<CropsManager>();
    }

    void Update()
    {
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        // JALAN KE TARGET (sebelum tool action)
        if (isWalkingToTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, walkTarget, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, walkTarget) < 0.01f)
            {
                transform.position = walkTarget;
                isWalkingToTarget = false;

                // Setelah sampe, mulai tool animation sesuai arah
                Vector3Int delta = walkFinalGrid - walkIntermediateGrid;
                if (delta.x > 0) { lastDirection = "kanan"; ChangeAnimationState("nyangkul-kanan"); }
                else if (delta.x < 0) { lastDirection = "kiri"; ChangeAnimationState("nyangkul-kanan"); }
                else if (delta.y > 0) { lastDirection = "belakang"; ChangeAnimationState("nyangkul-belakang"); }
                else { lastDirection = "depan"; ChangeAnimationState("nyangkul-depan"); }
                isUsingTool = true;
                toolFirstFrame = true;
            }
            return;
        }

        // BLOK GERAK + INPUT selama animasi nyangkul
        if (isUsingTool)
        {
            if (toolFirstFrame)
            {
                toolFirstFrame = false;
                toolTimer = anim.GetCurrentAnimatorStateInfo(0).length;
            }
            else
            {
                toolTimer -= Time.deltaTime;
                if (toolTimer <= 0f)
                {
                    isUsingTool = false;
                    toolFirstFrame = true;
                    if (pendingPlowPos.HasValue && cropsManager != null)
                    {
                        cropsManager.Plow(pendingPlowPos.Value);
                        pendingPlowPos = null;
                    }
                    if (movement == Vector2.zero)
                        ChangeAnimationState("idle-" + lastDirection);
                }
            }
            return;
        }

        // 1. KLIK MOUSE
        if (Input.GetMouseButtonDown(0))
        {
            bool inRange = tilemapReadController == null ||
                           tilemapReadController.IsMouseOverInRangeTile();
            if (inRange && tilemapReadController != null)
            {
                Vector3Int gridPos = tilemapReadController.GetMouseGridPosition();

                // Cek apakah tile ini bisa di-plow (hanya kalo tools terpilih)
                bool canPlow = tilemapReadController.IsToolSelected() && tilemapReadController.CanPlowAt(gridPos);
                if (canPlow)
                    pendingPlowPos = gridPos;
                else
                    pendingPlowPos = null;

                if (canPlow)
                {
                    Vector3Int playerGrid = tilemapReadController.GetPlayerGridPosition();
                    Vector3Int delta = gridPos - playerGrid;

                    if (gridPos == playerGrid)
                    {
                        lastDirection = "depan";
                        ChangeAnimationState("nyangkul-depan");
                        isUsingTool = true;
                    }
                    else if (delta.x != 0 && delta.y != 0)
                    {
                        HandleDiagonalClick();
                    }
                    else if (delta.y != 0)
                    {
                        walkIntermediateGrid = playerGrid;
                        walkFinalGrid = gridPos;
                        walkTarget = tilemapReadController.GridToWorldFeet(gridPos);
                        walkTarget.z = 0;
                        isWalkingToTarget = true;

                        if (delta.y > 0) { lastDirection = "belakang"; ChangeAnimationState("jalan-atas"); }
                        else { lastDirection = "depan"; ChangeAnimationState("jalan-bawah"); }
                    }
                    else
                    {
                        UpdateMouseDirection();
                    }
                }
                if (isUsingTool || isWalkingToTarget) return;
            }
        }

        // 2. WASD
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
        // 3. DIAM
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
        if (isUsingTool || isWalkingToTarget) return;
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void HandleDiagonalClick()
    {
        Vector3Int gridPos = tilemapReadController.GetMouseGridPosition();
        Vector3Int playerGrid = tilemapReadController.GetPlayerGridPosition();
        Vector3Int delta = gridPos - playerGrid;

        Vector3Int intermediateGrid = playerGrid;
        intermediateGrid.y += delta.y > 0 ? 1 : -1;

        walkTarget = tilemapReadController.GridToWorldFeet(intermediateGrid);
        walkTarget.z = 0;
        walkIntermediateGrid = intermediateGrid;
        walkFinalGrid = gridPos;
        isWalkingToTarget = true;

        // Mulai jalan
        lastDirection = delta.y > 0 ? "belakang" : "depan";
        ChangeAnimationState(delta.y > 0 ? "jalan-atas" : "jalan-bawah");
    }

    void UpdateMouseDirection()
    {
        string animName;

        if (tilemapReadController != null)
        {
            Vector3Int gridPos = tilemapReadController.GetMouseGridPosition();
            Vector3Int playerGrid = tilemapReadController.GetPlayerGridPosition();
            Vector3Int delta = gridPos - playerGrid;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                lastDirection = delta.x > 0 ? "kanan" : "kiri";
                animName = "nyangkul-kanan";
            }
            else if (delta.y > 0)
            {
                lastDirection = "belakang";
                animName = "nyangkul-kanan";
            }
            else
            {
                lastDirection = "depan";
                animName = "nyangkul-depan";
            }
        }
        else
        {
            Vector3 mousePos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 aimDir = mousePos - transform.position;
            if (aimDir.magnitude > 3f) return;

            if (Mathf.Abs(aimDir.x) > Mathf.Abs(aimDir.y))
            {
                lastDirection = aimDir.x > 0 ? "kanan" : "kiri";
                animName = "nyangkul-kanan";
            }
            else if (aimDir.y > 0)
            {
                lastDirection = "belakang";
                animName = "nyangkul-kanan";
            }
            else
            {
                lastDirection = "depan";
                animName = "nyangkul-depan";
            }
        }

        ChangeAnimationState(animName);
        isUsingTool = true;
    }

    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation && !newAnimation.StartsWith("nyangkul-")) return;

        anim.Play(newAnimation);
        currentAnimation = newAnimation;

        if (spriteRenderer != null)
        {
            bool isNyangkul = currentAnimation.StartsWith("nyangkul-");
            spriteRenderer.flipX = isNyangkul && lastDirection == "kiri";
        }
    }
}
