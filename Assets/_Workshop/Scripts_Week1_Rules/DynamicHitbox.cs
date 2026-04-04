using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class DynamicHitbox : MonoBehaviour
{
    [Header("打擊判定設定")]
    [Tooltip("要對哪個標籤的物件生效？ (留空代表全部都偵測)")]
    public string targetTag = "Enemy";

    [Tooltip("要修改對方的哪種資源？ (留空代表不修改數值，只做純觸發)")]
    public string targetResourceKey = "HP";

    [Tooltip("修改數值 (扣血填負數，補血填正數)")]
    public int modifyAmount = -1;

    [Header("環境碰撞設定 (防穿牆)")]
    [Tooltip("碰到哪些圖層(例如 Ground)會直接銷毀？")]
    public LayerMask obstacleLayer;

    [Header("打擊後的自身行為")]
    [Tooltip("打中目標或牆壁後，是否要摧毀自己？ (子彈通常勾選，刀光或尖刺不勾選)")]
    public bool destroyOnHit = true;

    [Tooltip("打中後，幾秒後摧毀自己?")]
    public float destroyDelay = 0;

    [Header("持續傷害設定")]
    [Tooltip("停留在範圍內是否會持續受到傷害？(搭配 ResourceManager 的無敵時間使用)")]
    public bool continuousDamage = false;

    [Header("廣播事件 (可接特效、音效或關卡機關)")]
    [Tooltip("打中目標或修改數值的瞬間觸發")]
    public UnityEvent OnHitSuccess;

    [Tooltip("只要目標【進入】範圍就會觸發 (適合做感應門、陷阱觸發)")]
    public UnityEvent OnEnterArea;

    [Tooltip("只要目標【離開】範圍就會觸發")]
    public UnityEvent OnExitArea;

    private void Awake()
    {
        // 防呆：確保 Collider 是 Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 判斷是否符合目標 (Tag 為空，或者 Tag 相符)
        if (string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag))
        {
            OnEnterArea.Invoke();
        }

        ProcessHit(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 如果有開啟持續傷害，就在停留時不斷嘗試攻擊
        if (continuousDamage)
        {
            ProcessHit(other);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 判斷離開的是否為目標
        if (string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag))
        {
            OnExitArea.Invoke();
        }
    }

    /// <summary>
    /// 將原本的判定邏輯抽出來，讓 Enter 和 Stay 都能共用
    /// </summary>
    private void ProcessHit(Collider2D other)
    {
        // --- 1. 優先檢查是否撞到牆壁/地板 ---
        if ((obstacleLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            if (destroyOnHit)
            {
                Destroy(gameObject, destroyDelay);
            }
            return;
        }

        // --- 2. 檢查是否撞到目標 (Tag 為空代表無差別攻擊/觸發) ---
        if (string.IsNullOrEmpty(targetTag) || other.CompareTag(targetTag))
        {
            // 如果有設定 ResourceKey，才去嘗試修改數值
            if (!string.IsNullOrEmpty(targetResourceKey))
            {
                ResourceManager targetResource = other.GetComponent<ResourceManager>();

                if (targetResource != null)
                {
                    // 執行數值修改
                    targetResource.ModifyResource(targetResourceKey, modifyAmount);
                    OnHitSuccess.Invoke();
                }
                else
                {
                    Debug.LogWarning($"[{gameObject.name}] 打到了 {other.name}，但對方身上沒有 ResourceManager！(若不需扣血，請將 targetResourceKey 清空)");
                }
            }
            else
            {
                // 如果沒有設定要修改數值 (純粹當觸發區)，依然呼叫命中事件
                OnHitSuccess.Invoke();
            }

            // 無論有沒有修改數值，只要撞到了目標，就處理自身銷毀
            if (destroyOnHit)
            {
                Destroy(gameObject, destroyDelay);
            }
        }
    }
}