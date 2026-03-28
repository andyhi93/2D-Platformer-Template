using UnityEngine;

[System.Serializable]
public class SpriteAnimData
{
    [Tooltip("動畫影格 (請拖入多張 Sprite 圖片)")]
    public Sprite[] frames;

    [Tooltip("此動畫的專屬播放速度 (FPS)")]
    public float frameRate = 12f;
}

public class SimpleSpriteAnimator : MonoBehaviour
{
    [Header("核心組件綁定")]
    [Tooltip("要替換圖片的目標 (如果不填，會自動抓取身上的 SpriteRenderer)")]
    public SpriteRenderer targetSprite;

    [Tooltip("用來讀取速度的剛體 (用來判斷是否在移動)")]
    public Rigidbody2D rb;

    [Header("地板偵測 (判斷是否在跳躍)")]
    [Tooltip("請把放在玩家腳底的 BoxCollider2D 拖進來")]
    public Collider2D groundCheckCollider;

    [Tooltip("請選擇哪些圖層代表地板")]
    public LayerMask groundLayer;

    [Header("動畫設定 (可展開獨立設定圖片與 FPS)")]
    public SpriteAnimData idleAnim; // 站立動畫包
    public SpriteAnimData runAnim;  // 跑動動畫包
    public SpriteAnimData jumpAnim; // 跳躍動畫包

    // 定義角色目前的狀態
    private enum AnimState { Idle, Run, Jump }
    private AnimState currentState = AnimState.Idle;

    // 內部計時器與影格紀錄
    private float timer;
    private int currentFrame;

    void Awake()
    {
        // 自動抓取組件防呆
        if (targetSprite == null) targetSprite = GetComponent<SpriteRenderer>();
        if (targetSprite == null) targetSprite = GetComponentInChildren<SpriteRenderer>();

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();
    }

    void Update()
    {
        if (targetSprite == null || rb == null) return;

        // --- 1. 決定目前應該處於什麼狀態 ---
        bool isGrounded = groundCheckCollider != null && groundCheckCollider.IsTouchingLayers(groundLayer);
        AnimState newState = AnimState.Idle;

        if (!isGrounded)
        {
            newState = AnimState.Jump;
        }
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            newState = AnimState.Run;
        }

        // --- 2. 如果狀態發生改變，重置動畫計時器 ---
        if (newState != currentState)
        {
            currentState = newState;
            currentFrame = 0;
            timer = 0f;
            UpdateSprite();
        }

        // --- 3. 執行動畫計時與輪播 ---
        SpriteAnimData currentAnim = GetCurrentAnimation();

        // 防呆：如果該陣列沒放圖片，就不執行
        if (currentAnim == null || currentAnim.frames == null || currentAnim.frames.Length == 0) return;

        // 累加計時器
        timer += Time.deltaTime;

        // 防呆：避免學生不小心把 FPS 設為 0 導致除以零的錯誤
        float safeFrameRate = Mathf.Max(0.1f, currentAnim.frameRate);
        float frameDuration = 1f / safeFrameRate;

        if (timer >= frameDuration)
        {
            timer -= frameDuration;
            currentFrame = (currentFrame + 1) % currentAnim.frames.Length;
            UpdateSprite();
        }
    }

    /// <summary>
    /// 根據目前狀態，回傳對應的「動畫資料包」
    /// </summary>
    private SpriteAnimData GetCurrentAnimation()
    {
        switch (currentState)
        {
            case AnimState.Jump: return jumpAnim;
            case AnimState.Run: return runAnim;
            case AnimState.Idle: return idleAnim;
            default: return idleAnim;
        }
    }

    /// <summary>
    /// 實際將圖片塞入 SpriteRenderer
    /// </summary>
    private void UpdateSprite()
    {
        SpriteAnimData currentAnim = GetCurrentAnimation();

        if (currentAnim != null && currentAnim.frames != null && currentAnim.frames.Length > 0)
        {
            if (currentFrame < currentAnim.frames.Length)
            {
                targetSprite.sprite = currentAnim.frames[currentFrame];
            }
        }
    }
}