using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("基礎移動數值")]
    public float maxMoveSpeed = 8f;
    public float jumpForce = 15f;

    // ==========================================
    // 👇 Week 2 施工區：進階跳躍手感變數
    // ==========================================
    //[Header("進階跳躍手感 (Week 2 施工區)")]
    //[Tooltip("【郊狼時間】離開邊緣後，還能起跳的寬容時間 (秒)")]
    //public float coyoteTime = 0.15f;
    // [W2 施工區] 提示：請在這裡新增一個計時器變數來倒數 (例如：private float coyoteTimer;)

    //[Tooltip("【短按小跳】放開跳躍鍵時，向上的速度保留比例 (0~1)")]
    //public float jumpCutMultiplier = 0.5f;
    // ==========================================

    [Header("進階移動手感 (Game Feel)")]
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

    [Header("重力翻轉設定")]
    [Tooltip("記錄初始的重力大小")]
    private float originalGravityScale;

    [Tooltip("當前重力是否顛倒")]
    private bool isGravityFlipped = false;

    [Header("後座力設定")]
    [Tooltip("每次點擊觸發的反推力量大小")]
    public float recoilForce = 15f;

    [Header("地板偵測 (Ground Check)")]
    public Collider2D groundCheckCollider;
    public LayerMask groundLayer;

    [Header("狀態廣播 (UnityEvents)")]
    public UnityEvent OnJumpSuccess;
    public UnityEvent OnRecoilTriggered;
    [Tooltip("當重力成功翻轉時廣播")]
    public UnityEvent OnGravityFlipped;

    private Rigidbody2D rb;
    private Vector2 currentInput;
    private Vector2 lastMousePosition;

    private float inputTimer = 0f;

    [Header("彈珠台模式設定")]
    [Tooltip("被敵人撞擊時彈飛的力量")]
    public float bounceBackForce = 20f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        // 儲存 Inspector 設定好的初始重力
        originalGravityScale = rb.gravityScale;
    }

    void Update()
    {
        // ==========================================
        // 👇 [W2 施工區 - 郊狼時間計時器]
        // ==========================================
        // 提示 1：使用 groundCheckCollider.IsTouchingLayers(groundLayer) 檢查是否踩在地上
        // 提示 2：如果踩在地上，把 coyoteTimer 設為滿的 (coyoteTime)
        // 提示 3：如果在空中，讓 coyoteTimer 隨著時間減少 (減去 Time.deltaTime)
        // ==========================================
    }

    public void SetMovementInput(Vector2 input)
    {
        currentInput = input;
    }

    public void UpdateMousePosition(Vector2 mouseWorldPos)
    {
        lastMousePosition = mouseWorldPos;
    }

    public void ExecuteJump()
    {
        // [W1 原始邏輯] 嚴格的地板判定
        bool isGrounded = groundCheckCollider != null && groundCheckCollider.IsTouchingLayers(groundLayer);

        // ==========================================
        // 👇 [W2 施工區 - 郊狼時間起跳判定]
        // 提示：把下方的 if (isGrounded) 改成判斷 coyoteTimer 是否大於 0
        // ==========================================

        if (isGrounded)
        {
            // 跳躍力方向必須與重力方向相反
            float appliedJumpForce = isGravityFlipped ? -jumpForce : jumpForce;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, appliedJumpForce);

            // ==========================================
            // 👇 [W2 施工區 - 消耗郊狼時間]
            // 提示：起跳成功後，記得把 coyoteTimer 歸零，否則玩家會在空中無限連跳！
            // ==========================================

            OnJumpSuccess.Invoke();
        }
    }

    // ==========================================
    // 👇 [W2 施工區 - 可變跳躍高度 (短按小跳)]
    // ==========================================
    /// <summary>
    /// 當玩家在空中「放開」跳躍鍵時呼叫此方法 (請綁定在 PlayerInputController 的 OnKeyUp)
    /// </summary>
    public void CancelJump()
    {
        // 提示 1：檢查角色目前是否正在往上飛？ (正常重力下 rb.linearVelocity.y > 0)
        // 提示 2：如果是，就把 rb.linearVelocity.y 乘以 jumpCutMultiplier
        // 注意：如果你有實作重力翻轉 (isGravityFlipped)，往上飛的判定方向會相反喔！
    }
    // ==========================================

    /// <summary>
    /// 強制給予一個向上的反彈力 (適合踩怪或彈簧)
    /// </summary>
    public void ForceBounce(float bounceMultiplier = 1f)
    {
        float appliedJumpForce = isGravityFlipped ? -jumpForce : jumpForce;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, appliedJumpForce * bounceMultiplier);
    }

    /// <summary>
    /// 彈珠台模式核心：傳入撞擊來源的位置，將玩家向反方向彈飛
    /// 適合綁定在 DynamicHitbox 的 OnEnterArea 或 OnHitSuccess
    /// </summary>
    public void ExecuteBounceBack(Transform attacker)
    {
        if (attacker == null) return;

        // 1. 計算從攻擊者指向玩家的向量 (彈開方向)
        Vector2 bounceDirection = (transform.position - attacker.position).normalized;

        // 2. 為了讓彈開感更明顯，稍微加強 Y 軸向上的力 (防止只在地上摩擦)
        bounceDirection += Vector2.up * 0.5f;
        bounceDirection = bounceDirection.normalized;

        // 3. 瞬間清除原本的速度（選用），增加「被擊中」的頓挫感
        rb.linearVelocity = Vector2.zero;

        // 4. 施加衝量
        rb.AddForce(bounceDirection * bounceBackForce, ForceMode2D.Impulse);

        // 5. 觸發反饋 (可以直接借用後座力的事件，或另建新事件)
        OnRecoilTriggered.Invoke();
    }

    /// <summary>
    /// 執行垂直重力翻轉 (適合綁定在特定按鍵上)
    /// </summary>
    public void ExecuteGravityFlip()
    {
        isGravityFlipped = !isGravityFlipped;

        // 1. 物理重力翻轉
        rb.gravityScale = isGravityFlipped ? -originalGravityScale : originalGravityScale;

        // 2. 視覺與物理偵測器翻轉
        transform.Rotate(180f, 0f, 0f);

        // 3. 廣播事件
        OnGravityFlipped.Invoke();
    }

    public void ExecuteRecoil()
    {
        Vector2 currentPos = transform.position;
        Vector2 aimDirection = (lastMousePosition - currentPos).normalized;
        Vector2 recoilDirection = -aimDirection;

        rb.AddForce(recoilDirection * recoilForce, ForceMode2D.Impulse);
        OnRecoilTriggered.Invoke();
    }

    void FixedUpdate()
    {
        float currentVelocityX = rb.linearVelocity.x;
        float forceX = 0f;

        if (Mathf.Abs(currentInput.x) > 0.01f)
        {
            inputTimer += Time.fixedDeltaTime;
            float timeRatio = Mathf.Clamp01(inputTimer / Mathf.Max(0.01f, timeToMaxSpeed));
            float speedMultiplier = accelerationCurve.Evaluate(timeRatio);

            float targetMoveVelocity = currentInput.x * maxMoveSpeed * speedMultiplier;
            float deficit = targetMoveVelocity - currentVelocityX;

            if ((currentInput.x > 0f && deficit > 0f) || (currentInput.x < 0f && deficit < 0f))
            {
                forceX = deficit * accelerationForce;
            }
        }
        else
        {
            inputTimer = 0f;

            bool isGrounded = groundCheckCollider != null && groundCheckCollider.IsTouchingLayers(groundLayer);

            if (isGrounded && applyGroundFriction)
            {
                float deficit = 0f - currentVelocityX;
                forceX = deficit * decelerationForce;
            }
        }

        if (Mathf.Abs(forceX) > 0.01f)
        {
            rb.AddForce(new Vector2(forceX, 0f), ForceMode2D.Force);
        }
    }
}