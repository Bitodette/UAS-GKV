using UnityEngine;

public class SeedController : MonoBehaviour
{
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private TilemapReadController tilemapReadController;
    private CropsManager cropsManager;
    private HotbarManager hotbarManager;
    private PlayerController playerController;

    private bool isPlanting = false;
    private float plantTimer = 0f;
    private bool plantFirstFrame = true;
    private Vector3Int pendingPlantPos;
    private string lastDirection = "depan";

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerController = GetComponent<PlayerController>();
        tilemapReadController = FindFirstObjectByType<TilemapReadController>();
        cropsManager = FindFirstObjectByType<CropsManager>();
        hotbarManager = FindFirstObjectByType<HotbarManager>();
    }

    void Update()
    {
        if (isPlanting)
        {
            if (plantFirstFrame)
            {
                plantFirstFrame = false;
                plantTimer = anim.GetCurrentAnimatorStateInfo(0).length;
            }
            else
            {
                plantTimer -= Time.deltaTime;
                if (plantTimer <= 0f)
                {
                    isPlanting = false;
                    plantFirstFrame = true;
                    if (cropsManager != null)
                    {
                        cropsManager.Seed(pendingPlantPos);
                        if (hotbarManager != null)
                            hotbarManager.ConsumeItem(hotbarManager.SelectedIndex, 1);
                    }
                    if (anim != null)
                        anim.Play("idle-" + lastDirection);
                    if (playerController != null)
                        playerController.enabled = true;
                }
            }
            return;
        }

        if (!Input.GetMouseButtonDown(0)) return;

        if (tilemapReadController == null || !tilemapReadController.IsSeedSelected()) return;

        Vector3Int gridPos = tilemapReadController.GetMouseGridPosition();
        if (!tilemapReadController.CanSeedAt(gridPos)) return;

        pendingPlantPos = gridPos;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;
        Vector2 aimDir = mousePos - transform.position;

        if (Mathf.Abs(aimDir.x) > Mathf.Abs(aimDir.y))
            lastDirection = aimDir.x > 0 ? "kanan" : "kiri";
        else if (aimDir.y > 0)
            lastDirection = "belakang";
        else
            lastDirection = "depan";

        string animName = lastDirection == "depan" ? "nyangkul-depan" : "nyangkul-kanan";
        if (lastDirection == "belakang") animName = "nyangkul-belakang";

        if (spriteRenderer != null)
            spriteRenderer.flipX = lastDirection == "kiri";

        anim.Play(animName);
        isPlanting = true;
        plantFirstFrame = true;
        if (playerController != null)
            playerController.enabled = false;
    }
}
