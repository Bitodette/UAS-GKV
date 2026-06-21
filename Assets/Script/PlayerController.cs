using UnityEngine;

[RequireComponent(typeof(SfxPlayer))]
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
    private HotbarManager hotbar;
    private bool isUsingTool = false;
    private float toolTimer = 0f;
    private bool toolFirstFrame = true;
    private bool isWatering = false;
    private bool isKapak = false;

    private bool isWalkingToTarget = false;
    private Vector3 walkTarget;
    private Vector3Int walkIntermediateGrid;
    private Vector3Int walkFinalGrid;

    private Vector3Int? pendingPlowPos;
    private Vector3Int? pendingWaterPos;
    private ToolHit pendingTreeHit;

    [Header("SFX")]
    [SerializeField] private SfxPlayer sfxPlayer;

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>(); 
        mainCam = Camera.main; 
        tilemapReadController = FindFirstObjectByType<TilemapReadController>();
        cropsManager = FindFirstObjectByType<CropsManager>();
        hotbar = FindFirstObjectByType<HotbarManager>();

        if (sfxPlayer == null)
            sfxPlayer = GetComponent<SfxPlayer>();
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
                string animPrefix = GetToolAnimPrefix();
                if (delta.x > 0) { lastDirection = "kanan"; ChangeAnimationState(animPrefix + "-kanan"); }
                else if (delta.x < 0) { lastDirection = "kiri"; ChangeAnimationState(animPrefix + "-kanan"); }
                else if (delta.y > 0) { lastDirection = "belakang"; ChangeAnimationState(animPrefix == "nyiram" ? "nyiram-depan" : animPrefix == "ngapak" ? "ngapak-kanan" : animPrefix + "-belakang"); }
                else { lastDirection = "depan"; ChangeAnimationState(animPrefix + "-depan"); }
                isUsingTool = true;
                toolFirstFrame = true;
                PlayToolSfx();
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
                    isWatering = false;
                    isKapak = false;
                    if (pendingPlowPos.HasValue && cropsManager != null)
                    {
                        cropsManager.Plow(pendingPlowPos.Value);
                        pendingPlowPos = null;
                    }
                    if (pendingWaterPos.HasValue && cropsManager != null)
                    {
                        cropsManager.Water(pendingWaterPos.Value);
                        pendingWaterPos = null;
                    }
                    if (pendingTreeHit != null)
                    {
                        pendingTreeHit.Hit();
                        pendingTreeHit = null;
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

                string selectedItemName = hotbar != null && hotbar.SelectedItem != null ? hotbar.SelectedItem.itemName : "";

                bool isHoe = selectedItemName == "tools";
                bool isWaterCan = selectedItemName == "water can";
                bool isSeed = hotbar != null && hotbar.SelectedItem != null && hotbar.SelectedItem.isSeed;
                bool isKapakSelected = selectedItemName == "Kapak";

                bool canAct = false;
                if (isHoe && tilemapReadController.CanPlowAt(gridPos))
                {
                    canAct = true;
                    pendingPlowPos = gridPos;
                    pendingWaterPos = null;
                    pendingTreeHit = null;
                    isWatering = false;
                    isKapak = false;
                }
                else if (isSeed && tilemapReadController.CanSeedAt(gridPos))
                {
                    cropsManager.Seed(gridPos);
                    pendingTreeHit = null;
                    if (hotbar != null)
                        hotbar.ConsumeItem(hotbar.SelectedIndex, 1);
                    return;
                }
                else if (isWaterCan && tilemapReadController.CanWaterAt(gridPos))
                {
                    canAct = true;
                    pendingWaterPos = gridPos;
                    pendingPlowPos = null;
                    pendingTreeHit = null;
                    isWatering = true;
                    isKapak = false;
                }
                else if (isKapakSelected)
                {
                    canAct = true;
                    pendingPlowPos = null;
                    pendingWaterPos = null;
                    isWatering = false;
                    isKapak = true;

                    Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
                    mouseWorldPos.z = 0;
                    Collider2D[] hits = Physics2D.OverlapCircleAll(mouseWorldPos, 0.6f);
                    pendingTreeHit = null;
                    foreach (Collider2D col in hits)
                    {
                        ToolHit hit = col.GetComponentInParent<ToolHit>();
                        if (hit != null)
                        {
                            pendingTreeHit = hit;
                            break;
                        }
                    }
                }
                else
                {
                    pendingPlowPos = null;
                    pendingWaterPos = null;
                    pendingTreeHit = null;
                }

                if (canAct)
                {
                    Vector3Int playerGrid = tilemapReadController.GetPlayerGridPosition();
                    Vector3Int delta = gridPos - playerGrid;

                    if (gridPos == playerGrid)
                    {
                        lastDirection = "depan";
                        ChangeAnimationState(GetToolAnimPrefix() + "-depan");
                        isUsingTool = true;
                        PlayToolSfx();
                    }
                    else if (delta.x != 0 && delta.y != 0)
                    {
                        if (!isKapak)
                            HandleDiagonalClick();
                    }
                    else if (delta.y != 0)
                    {
                        if (isKapak)
                        {
                            UpdateMouseDirection();
                        }
                        else
                        {
                            walkIntermediateGrid = playerGrid;
                            walkFinalGrid = gridPos;
                            walkTarget = tilemapReadController.GridToWorldFeet(gridPos);
                            walkTarget.z = 0;
                            isWalkingToTarget = true;

                            if (delta.y > 0) { lastDirection = "belakang"; ChangeAnimationState("jalan-atas"); }
                            else { lastDirection = "depan"; ChangeAnimationState("jalan-bawah"); }
                        }
                    }
                    else
                    {
                        if (isWatering)
                        {
                            walkIntermediateGrid = playerGrid;
                            walkFinalGrid = gridPos;
                            walkTarget = tilemapReadController.GridToWorldFeet(gridPos);
                            walkTarget.z = 0;
                            isWalkingToTarget = true;
                            if (delta.x > 0) { lastDirection = "kanan"; ChangeAnimationState("jalan-kanan"); }
                            else { lastDirection = "kiri"; ChangeAnimationState("jalan-kiri"); }
                        }
                        else
                        {
                            UpdateMouseDirection();
                        }
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
        string toolPrefix = GetToolAnimPrefix();

        if (tilemapReadController != null)
        {
            Vector3Int gridPos = tilemapReadController.GetMouseGridPosition();
            Vector3Int playerGrid = tilemapReadController.GetPlayerGridPosition();
            Vector3Int delta = gridPos - playerGrid;

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                lastDirection = delta.x > 0 ? "kanan" : "kiri";
                animName = toolPrefix + "-kanan";
            }
            else if (delta.y > 0)
            {
                lastDirection = "belakang";
                animName = toolPrefix == "nyiram" ? "nyiram-depan" : toolPrefix + "-kanan";
            }
            else
            {
                lastDirection = "depan";
                animName = toolPrefix + "-depan";
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
                animName = toolPrefix + "-kanan";
            }
            else if (aimDir.y > 0)
            {
                lastDirection = "belakang";
                animName = toolPrefix == "nyiram" ? "nyiram-depan" : toolPrefix + "-kanan";
            }
            else
            {
                lastDirection = "depan";
                animName = toolPrefix + "-depan";
            }
        }

        ChangeAnimationState(animName);
        isUsingTool = true;
        PlayToolSfx();
    }

    private string GetToolAnimPrefix()
    {
        if (isWatering) return "nyiram";
        if (isKapak) return "ngapak";
        return "nyangkul";
    }

    private void PlayToolSfx()
    {
        sfxPlayer.Play(currentAnimation);
    }

    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation && !newAnimation.StartsWith("nyangkul-") && !newAnimation.StartsWith("nyiram-") && !newAnimation.StartsWith("ngapak-")) return;

        anim.Play(newAnimation);
        currentAnimation = newAnimation;

        if (spriteRenderer != null)
        {
            bool isToolAnim = currentAnimation.StartsWith("nyangkul-") || currentAnimation.StartsWith("nyiram-") || currentAnimation.StartsWith("ngapak-");
            spriteRenderer.flipX = isToolAnim && lastDirection == "kiri";
        }
    }
}
