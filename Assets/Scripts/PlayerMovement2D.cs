using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("基礎移動數值")]
    public float maxMoveSpeed = 8f;
    public float jumpForce = 15f;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        // 儲存 Inspector 設定好的初始重力
        originalGravityScale = rb.gravityScale;
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
        if (groundCheckCollider != null && groundCheckCollider.IsTouchingLayers(groundLayer))
        {
            // 關鍵修正：跳躍力方向必須與重力方向相反
            float appliedJumpForce = isGravityFlipped ? -jumpForce : jumpForce;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, appliedJumpForce);
            OnJumpSuccess.Invoke();
        }
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
        // 沿著 X 軸旋轉 180 度，頭腳顛倒。
        // 好處：水平移動輸入不變，且 GroundCheckCollider 會自動移到頭頂（新的腳底）
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