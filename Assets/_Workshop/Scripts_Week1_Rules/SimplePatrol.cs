using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SimplePatrol : MonoBehaviour
{
    [Header("巡邏設定")]
    public float moveSpeed = 3f;
    [Tooltip("勾選表示初始向右走，取消勾選則向左走")]
    public bool movingRight = true;

    [Header("感測器 (請掛載子物件 BoxCollider2D 並勾選 IsTrigger)")]
    [Tooltip("放在敵人正前方的感測器，用來偵測牆壁")]
    public Collider2D wallSensor;

    [Tooltip("放在敵人前方偏下（預判腳步）的感測器，用來偵測懸崖邊緣")]
    public Collider2D edgeSensor;

    [Tooltip("定義哪些圖層算是牆壁或地板")]
    public LayerMask groundLayer;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; // 避免物理滾動

        // 如果初始設定向左，確保一開始就轉向
        if (!movingRight)
        {
            ApplyFlipTransform();
        }
    }

    void FixedUpdate()
    {
        // 1. 狀態偵測
        // 撞到牆 = 牆壁感測器碰到了圖層
        bool hitWall = wallSensor != null && wallSensor.IsTouchingLayers(groundLayer);

        // 遇到懸崖 = 邊緣感測器「沒有」碰到圖層
        bool isEdge = edgeSensor != null && !edgeSensor.IsTouchingLayers(groundLayer);

        // 2. 判斷是否需要轉向
        if (hitWall || isEdge)
        {
            Flip();
        }

        // 3. 執行移動
        float currentSpeed = movingRight ? moveSpeed : -moveSpeed;
        // 保持原本的 Y 軸速度（確保重力正常運作），只改變 X 軸速度
        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }

    /// <summary>
    /// 處理轉向邏輯
    /// </summary>
    private void Flip()
    {
        movingRight = !movingRight;
        ApplyFlipTransform();
    }

    /// <summary>
    /// 實際旋轉物件
    /// </summary>
    private void ApplyFlipTransform()
    {
        // 這裡極度重要：必須旋轉 Transform，不能只翻轉 SpriteRenderer (flipX)
        // 因為旋轉 Y 軸 180 度，掛在子物件的感測器才會跟著轉到另一邊！
        transform.Rotate(0f, 180f, 0f);
    }
}