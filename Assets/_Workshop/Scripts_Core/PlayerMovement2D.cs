using UnityEngine;
using UnityEngine.Events;

// ==========================================
// 👇 定義三個資料群組，讓 Inspector 變乾淨
// ==========================================

[System.Serializable]
public class BasicSettings
{
    [Tooltip("角色最高移動速度")]
    public float maxMoveSpeed = 8f;
    [Tooltip("基礎跳躍力")]
    public float jumpForce = 6f;
}

[System.Serializable]
public class AdvancedFeelSettings
{
    [Header("加速與移動手感")]
    [Tooltip("達到最高速所需的總時間 (秒)")]
    public float timeToMaxSpeed = 0.5f;
    [Tooltip("加速曲線。橫軸(X)為時間比例(0~1)，縱軸(Y)為目標速度比例(0~1)。")]
    public AnimationCurve accelerationCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [Tooltip("起步與加速時的物理推力係數")]
    public float accelerationForce = 20f;

    [Header("減速與摩擦力設定")]
    [Tooltip("是否啟用腳本內建的地面煞車功能？(關閉時可用於零摩擦力冰面)")]
    public bool applyGroundFriction = true;
    [Tooltip("放開按鍵且在地面時的煞車阻力係數")]
    public float decelerationForce = 20f;

    // ==========================================
    // [Week 2 施工區預留位置]
    // ==========================================
    // [Header("Week 2 施工區：進階跳躍")]
    // [Tooltip("【郊狼時間】離開邊緣後，還能起跳的寬容時間 (秒)")]
    // public float coyoteTime = 0.15f;
    // [Tooltip("【短按小跳】放開跳躍鍵時，向上的速度保留比例 (0~1)")]
    // public float jumpCutMultiplier = 0.5f;
    // [Tooltip("【預先輸入】落地前幾秒內按下跳躍，落地瞬間會自動起跳 (秒)")]
    // public float jumpBufferTime = 0.1f;
}

[System.Serializable]
public class SpecialCardSettings
{
    [Header("蓄力青蛙跳設定 (Jump King 風格)")]
    public float maxChargeTime = 1f;
    public float maxChargeMultiplier = 2f;
    public bool lockMoveWhileCharging = true;

    [Header("後座力模式設定")]
    public float recoilForce = 15f;

    [Header("彈珠台模式設定")]
    public float bounceBackForce = 20f;
}

// ==========================================
// 👇 主要控制器腳本
// ==========================================

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("⚙️ 參數設定區 (點擊箭頭展開)")]
    public BasicSettings basic = new BasicSettings();
    public AdvancedFeelSettings advanced = new AdvancedFeelSettings();
    public SpecialCardSettings special = new SpecialCardSettings();

    [Header("🧩 核心組件 (拖曳綁定)")]
    public Collider2D groundCheckCollider;
    public LayerMask groundLayer;

    [Header("📡 狀態廣播 (UnityEvents)")]
    public UnityEvent OnJumpSuccess;
    public UnityEvent OnRecoilTriggered;
    public UnityEvent OnGravityFlipped;

    // 內部記憶體 (隱藏不顯示，企劃勿動)
    private float originalGravityScale;
    private bool isGravityFlipped = false;
    private Rigidbody2D rb;
    private Vector2 currentInput;
    private Vector2 lastMousePosition;

    private float inputTimer = 0f;
    private float currentChargeTimer = 0f;
    private bool isChargingJump = false;

    // private float jumpBufferCounter = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        originalGravityScale = rb.gravityScale;
    }

    void Update()
    {
        void Update()
        {
            // [W2 施工區 - 郊狼時間計時器]

            // [W2 施工區 - Jump Buffer 落地判定與觸發]
            // if (jumpBufferCounter > 0f)
            // {
            //     jumpBufferCounter -= Time.deltaTime;
            //     bool isGrounded = groundCheckCollider != null && groundCheckCollider.IsTouchingLayers(groundLayer);
            //     
            //     if (isGrounded)
            //     {
            //         jumpBufferCounter = 0f; // 重置計時器
            //         float appliedJumpForce = isGravityFlipped ? -basic.jumpForce : basic.jumpForce;
            //         rb.linearVelocity = new Vector2(rb.linearVelocity.x, appliedJumpForce);
            //         OnJumpSuccess.Invoke();
            //     }
            // }
        }
    }

    public void SetMovementInput(Vector2 input)
    {
        currentInput = input;
    }

    public void UpdateMousePosition(Vector2 mouseWorldPos)
    {
        lastMousePosition = mouseWorldPos;
    }

    // --- 原始跳躍 ---
    public void ExecuteJump()
    {
        bool isGrounded = groundCheckCollider != null && groundCheckCollider.IsTouchingLayers(groundLayer);

        if (isGrounded)
        {
            float appliedJumpForce = isGravityFlipped ? -basic.jumpForce : basic.jumpForce;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, appliedJumpForce);
            OnJumpSuccess.Invoke();
        }
        // else
        // {
        //     // [W2 施工區 - Jump Buffer 記錄按鍵]
        //     jumpBufferCounter = advanced.jumpBufferTime;
        // }
    }

    public void CancelJump()
    {
        // [W2 施工區 - 可變跳躍高度 (短按小跳)]
    }

    // --- 蓄力青蛙跳 (Jump King) ---
    public void ChargeJump()
    {
        bool isGrounded = groundCheckCollider != null && groundCheckCollider.IsTouchingLayers(groundLayer);
        if (!isGrounded) return;

        isChargingJump = true;
        currentChargeTimer += Time.deltaTime;
        currentChargeTimer = Mathf.Clamp(currentChargeTimer, 0f, special.maxChargeTime);
    }

    public void ExecuteChargedJump()
    {
        if (!isChargingJump) return;

        float chargeRatio = currentChargeTimer / special.maxChargeTime;
        float finalMultiplier = Mathf.Lerp(1f, special.maxChargeMultiplier, chargeRatio);

        float appliedJumpForce = isGravityFlipped ? -basic.jumpForce : basic.jumpForce;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, appliedJumpForce * finalMultiplier);

        OnJumpSuccess.Invoke();

        isChargingJump = false;
        currentChargeTimer = 0f;
    }

    // --- 特殊卡牌互動 ---
    public void ForceBounce(float bounceMultiplier = 1f)
    {
        float appliedJumpForce = isGravityFlipped ? -basic.jumpForce : basic.jumpForce;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, appliedJumpForce * bounceMultiplier);
    }

    public void ExecuteBounceBack(Transform attacker)
    {
        if (attacker == null) return;

        Vector2 bounceDirection = (transform.position - attacker.position).normalized;
        bounceDirection += Vector2.up * 0.5f;
        bounceDirection = bounceDirection.normalized;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(bounceDirection * special.bounceBackForce, ForceMode2D.Impulse);

        OnRecoilTriggered.Invoke();
    }

    public void ExecuteGravityFlip()
    {
        isGravityFlipped = !isGravityFlipped;
        rb.gravityScale = isGravityFlipped ? -originalGravityScale : originalGravityScale;
        transform.Rotate(180f, 0f, 0f);
        OnGravityFlipped.Invoke();
    }

    public void ExecuteRecoil()
    {
        Vector2 currentPos = transform.position;
        Vector2 aimDirection = (lastMousePosition - currentPos).normalized;
        Vector2 recoilDirection = -aimDirection;

        rb.AddForce(recoilDirection * special.recoilForce, ForceMode2D.Impulse);
        OnRecoilTriggered.Invoke();
    }

    // --- 物理移動邏輯 ---
    void FixedUpdate()
    {
        float currentVelocityX = rb.linearVelocity.x;
        float forceX = 0f;

        // 如果正在蓄力且設定為鎖定移動，強制不給左右推力
        if (isChargingJump && special.lockMoveWhileCharging)
        {
            currentInput.x = 0f;
        }

        if (Mathf.Abs(currentInput.x) > 0.01f)
        {
            inputTimer += Time.fixedDeltaTime;
            float timeRatio = Mathf.Clamp01(inputTimer / Mathf.Max(0.01f, advanced.timeToMaxSpeed));
            float speedMultiplier = advanced.accelerationCurve.Evaluate(timeRatio);

            float targetMoveVelocity = currentInput.x * basic.maxMoveSpeed * speedMultiplier;
            float deficit = targetMoveVelocity - currentVelocityX;

            if ((currentInput.x > 0f && deficit > 0f) || (currentInput.x < 0f && deficit < 0f))
            {
                forceX = deficit * advanced.accelerationForce;
            }
        }
        else
        {
            inputTimer = 0f;

            bool isGrounded = groundCheckCollider != null && groundCheckCollider.IsTouchingLayers(groundLayer);

            if (isGrounded && advanced.applyGroundFriction)
            {
                float deficit = 0f - currentVelocityX;
                forceX = deficit * advanced.decelerationForce;
            }
        }

        if (Mathf.Abs(forceX) > 0.01f)
        {
            rb.AddForce(new Vector2(forceX, 0f), ForceMode2D.Force);
        }
    }
}