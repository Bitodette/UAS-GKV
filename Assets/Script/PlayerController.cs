using UnityEngine;

[RequireComponent(typeof(SfxPlayer))]
public class PlayerController : MonoBehaviour
{
    [Header("Pengaturan Pergerakan")]
    public float moveSpeed = 5f;

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb; 
    public Vector2 movement;                         // nilai input WASD (-1, 0, 1)
    private string currentAnimation = "";            // state animasi skrg, biar gak re-play
    private string lastDirection = "depan";          // arah hadap terakhir buat idle & flipX

    private Camera mainCam;
    private TilemapReadController tilemapReadController;
    private CropsManager cropsManager;
    private HotbarManager hotbar;
    private bool isUsingTool = false;                // lg animasi tool (blokir WASD)
    private float toolTimer = 0f;                    // countdown durasi animasi tool
    private bool toolFirstFrame = true;              // frame pertama tool: ambil durasi animasi
    private bool isWatering = false;                 // lg pake water can (buat pilih prefix animasi)
    private bool isKapak = false;                    // lg pake kapak

    private bool isWalkingToTarget = false;          // lg jalan otomatis ke tile target
    private Vector3 walkTarget;                      // world position tujuan jalan
    private Vector3Int walkIntermediateGrid;         // grid perantara (kalo target diagonal)
    private Vector3Int walkFinalGrid;                // grid akhir tujuan

    private Vector3Int? pendingPlowPos;              // tile yg harus di-cangkul stlh animasi selesai
    private Vector3Int? pendingWaterPos;             // tile yg harus di-siram
    private ToolHit pendingTreeHit;                  // pohon yg harus kena hit

    [Header("SFX")]
    [SerializeField] private SfxPlayer sfxPlayer;

    private float lastWalkNormalizedTime = 0f;       // buat deteksi langkah kaki dari normalizedTime animasi
    private bool wasWalking = false;

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
        if (PauseManager.IsPaused) return;

        movement.x = Input.GetAxisRaw("Horizontal"); // -1, 0, 1 — instan, tanpa smoothing
        movement.y = Input.GetAxisRaw("Vertical");

        // 1. CEK AUTO-WALK (jalan ke target tile)
        if (isWalkingToTarget)
        {
            transform.position = Vector3.MoveTowards(transform.position, walkTarget, moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, walkTarget) < 0.01f)
            {
                transform.position = walkTarget;
                isWalkingToTarget = false;

                // stlh sampe, mainkan tool animation sesuai arah akhir
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

        // 2. CEK ANIMASI TOOL (lagi nyangkul/nyiram/ngapak)
        if (isUsingTool)
        {
            if (toolFirstFrame)
            {
                toolFirstFrame = false;
                toolTimer = anim.GetCurrentAnimatorStateInfo(0).length; // ambil durasi animasi dari Animator
            }
            else
            {
                toolTimer -= Time.deltaTime;
                if (toolTimer <= 0f) // animasi selesai → eksekusi action
                {
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

                    bool hasExtended = false;
                    if (pendingTreeHit != null)
                    {
                        float treeAnimDuration = 0f;
                        if (pendingTreeHit is TreeCuttable tree)
                            treeAnimDuration = tree.GetHitAnimDuration();
                        toolTimer += treeAnimDuration; // tambah waktu buat animasi pohon
                        hasExtended = true;
                        pendingTreeHit.Hit();
                        pendingTreeHit = null;
                    }

                    if (!hasExtended)
                    {
                        isUsingTool = false;
                        toolFirstFrame = true;
                        isWatering = false;
                        isKapak = false;
                    }

                    if (movement == Vector2.zero)
                        ChangeAnimationState("idle-" + lastDirection);
                }
            }
            return;
        }

        // 3. KLIK KIRI — TOOL / SEED
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
                    cropsManager.Seed(gridPos);                   // seed langsung, gak perlu animasi
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
                    Collider2D[] hits = Physics2D.OverlapCircleAll(mouseWorldPos, 0.6f); // deteksi pohon di area klik
                    pendingTreeHit = null;
                    foreach (Collider2D col in hits)
                    {
                        ToolHit hit = col.GetComponentInParent<ToolHit>();
                        if (hit != null)
                        {
                            if (hit is TreeCuttable tree && tree.trunkCollider != null && tree.trunkCollider != col)
                                continue; // skip kalo yg ke detect adalah daun, bukan batang
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

                    if (gridPos == playerGrid)       // tile yang sama → tool di tempat
                    {
                        lastDirection = "depan";
                        ChangeAnimationState(GetToolAnimPrefix() + "-depan");
                        isUsingTool = true;
                        PlayToolSfx();
                    }
                    else if (delta.x != 0 && delta.y != 0) // diagonal → jalan ke intermediate dulu
                    {
                        if (!isKapak)
                            HandleDiagonalClick();
                    }
                    else if (delta.y != 0)           // vertikal → auto-walk (kecuali kapak)
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
                    else                              // horizontal
                    {
                        if (isWatering)               // nyiram harus auto-walk
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
                            UpdateMouseDirection();   // cangkul/kapak horizontal langsung dari tempat berdiri
                        }
                    }
                }
                if (isUsingTool || isWalkingToTarget) return;
            }
        }

        // 4. WASD — GERAKAN BIASA
        if (movement != Vector2.zero) 
        {
            if (movement.x < 0)                      // prioritas X dulu (horizontal > vertikal)
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
        // 5. DIAM — IDLE
        else 
        {
            if (lastDirection == "kiri") ChangeAnimationState("idle-kiri");
            else if (lastDirection == "kanan") ChangeAnimationState("idle-kanan");
            else if (lastDirection == "belakang") ChangeAnimationState("idle-belakang");
            else if (lastDirection == "depan") ChangeAnimationState("idle-depan");
        }

        DetectFootstep();
    }

    void FixedUpdate()
    {
        if (isUsingTool || isWalkingToTarget || PauseManager.IsPaused) return;
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime); // gerak fisika
    }

    void HandleDiagonalClick()
    {
        Vector3Int gridPos = tilemapReadController.GetMouseGridPosition();
        Vector3Int playerGrid = tilemapReadController.GetPlayerGridPosition();
        Vector3Int delta = gridPos - playerGrid;

        Vector3Int intermediateGrid = playerGrid;
        intermediateGrid.y += delta.y > 0 ? 1 : -1;  // jalan ke tile di sumbu Y dulu

        walkTarget = tilemapReadController.GridToWorldFeet(intermediateGrid);
        walkTarget.z = 0;
        walkIntermediateGrid = intermediateGrid;
        walkFinalGrid = gridPos;
        isWalkingToTarget = true;

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

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))   // dominan horizontal
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
        else   // fallback kalo tilemap gak ada: pake posisi mouse langsung
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

    // deteksi langkah kaki dari normalizedTime animasi, mainkan sfx bergantian kiri-kanan
    private void DetectFootstep()
    {
        if (currentAnimation.StartsWith("jalan-"))
        {
            float currentNorm = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
            float norm = currentNorm - Mathf.Floor(currentNorm); // 0.0 - 1.0 (loop)

            if (!wasWalking)
            {
                wasWalking = true;
                lastWalkNormalizedTime = norm;
                return;
            }

            if (lastWalkNormalizedTime > 0.7f && norm < 0.3f)    // wrap 0.9 → 0.1 = 1 langkah
                sfxPlayer.PlaySequential(currentAnimation);
            else if (lastWalkNormalizedTime < 0.4f && norm >= 0.4f) // mid-cycle = langkah satunya
                sfxPlayer.PlaySequential(currentAnimation);

            lastWalkNormalizedTime = norm;
        }
        else
        {
            wasWalking = false;
        }
    }

    void ChangeAnimationState(string newAnimation)
    {
        if (currentAnimation == newAnimation && !newAnimation.StartsWith("nyangkul-") && !newAnimation.StartsWith("nyiram-") && !newAnimation.StartsWith("ngapak-")) return;
        // kalo state sama & bukan tool → skip. Tool harus tetap di-restart biar animasi muter dari awal

        anim.Play(newAnimation);                    // Unity: mainkan state di Animator Controller
        currentAnimation = newAnimation;

        if (spriteRenderer != null)
        {
            bool isToolAnim = currentAnimation.StartsWith("nyangkul-") || currentAnimation.StartsWith("nyiram-") || currentAnimation.StartsWith("ngapak-");
            spriteRenderer.flipX = isToolAnim && lastDirection == "kiri"; // flip sprite tool kalo arah kiri
        }
    }
}
