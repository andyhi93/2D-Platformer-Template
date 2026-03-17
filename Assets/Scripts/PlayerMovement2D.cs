using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("數值設定")]
    public float moveSpeed = 8f;
    public float jumpForce = 15f;

    [Header("地板偵測 (Ground Check)")]
    [Tooltip("請把放在玩家腳底的獨立 BoxCollider2D 拖進來 (勾選 IsTrigger)")]
    public Collider2D groundCheckCollider;

    [Header("狀態廣播 (UnityEvents)")]
    [Tooltip("只有在『成功起跳』的那一瞬間才會廣播 (適合接音效、變形、揚塵特效)")]
    public UnityEvent OnJumpSuccess;

    [Tooltip("請選擇哪些圖層 (Layer) 代表地板")]
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Vector2 currentInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 鎖定 Z 軸旋轉，避免物理碰撞時角色跌倒
        rb.freezeRotation = true;
    }

    /// <summary>
    /// 接收來自 InputController 的輸入向量
    /// </summary>
    public void SetMovementInput(Vector2 input)
    {
        currentInput = input;
    }

    /// <summary>
    /// 執行跳躍邏輯 (包含地板判斷)
    /// </summary>
    public void ExecuteJump()
    {
        // 防呆：確保有掛載 Collider，並且該 Collider 正接觸到指定的 Layer
        if (groundCheckCollider != null && groundCheckCollider.IsTouchingLayers(groundLayer))
        {
            // 將垂直速度覆寫為 jumpForce，保留當前的水平速度
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            //廣播跳躍成功事件
            OnJumpSuccess.Invoke();
        }
    }

    void FixedUpdate()
    {
        // 物理移動：直接改變 velocity 以獲得最即時的操控手感
        rb.linearVelocity = new Vector2(currentInput.x * moveSpeed, rb.linearVelocity.y);
    }
}