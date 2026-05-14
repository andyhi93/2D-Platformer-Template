using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EasyAnimatorController : MonoBehaviour
{
    [Header("核心組件綁定")]
    [Tooltip("用來讀取速度的剛體 (判斷是否在移動)")]
    public Rigidbody2D rb;

    [Header("地板偵測 (判斷是否在跳躍)")]
    [Tooltip("請把放在玩家腳底的 BoxCollider2D 拖進來")]
    public Collider2D groundCheckCollider;
    [Tooltip("請選擇哪些圖層代表地板")]
    public LayerMask groundLayer;

    [Header("請輸入你的 AnimationClip 名稱 (注意大小寫)")]
    public string idleClipName = "Idle";
    public string runClipName = "Run";
    public string jumpClipName = "Jump";

    private Animator animator;
    private string currentState;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = GetComponentInParent<Rigidbody2D>();
    }

    void Update()
    {
        // 如果必備的組件真的找不到，就直接跳過不執行，保護遊戲不崩潰
        if (animator == null || rb == null || groundCheckCollider == null) return;

        // --- 1. 決定目前應該處於什麼狀態 ---
        bool isGrounded = groundCheckCollider.IsTouchingLayers(groundLayer);
        string newState = idleClipName; // 預設是站立 (Idle) 動畫

        // 優先判斷是否在空中：只要離開地板，就切換成跳躍動畫
        if (!isGrounded)
        {
            newState = jumpClipName;
        }
        // 判斷是否在跑步：使用大於 0.1f 而不是不等於 0
        // 因為 Unity 物理系統有時候會有微小滑動（例如速度是 0.0001），這樣寫比較精準
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            newState = runClipName;
        }

        // --- 2. 執行切換 (最重要的一步！) ---
        // 為什麼要檢查 newState != currentState？
        // 如果不檢查，程式「每一幀 (Frame)」都會瘋狂叫 Animator 從頭播放動畫。
        // 結果就是動畫永遠卡在「第一張圖片」，角色看起來會像全身抽筋一樣
        if (newState != currentState)
        {
            animator.Play(newState); // 強制播放新動畫
            currentState = newState; // 把現在的狀態記下來，下一幀就不會重複播放了
        }
    }
}