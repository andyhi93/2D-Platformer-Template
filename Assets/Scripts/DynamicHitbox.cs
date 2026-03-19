using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class DynamicHitbox : MonoBehaviour
{
    [Header("打擊判定設定")]
    [Tooltip("要對哪個標籤的物件生效？ (例如: Enemy 或 Player)")]
    public string targetTag = "Enemy";

    [Tooltip("要修改對方的哪種資源？ (例如: HP)")]
    public string targetResourceKey = "HP";

    [Tooltip("修改數值 (扣血填負數，補血填正數)")]
    public int modifyAmount = -1;

    [Header("環境碰撞設定 (防穿牆)")]
    [Tooltip("碰到哪些圖層(例如 Ground)會直接銷毀？")]
    public LayerMask obstacleLayer;

    [Header("打擊後的自身行為")]
    [Tooltip("打中目標或牆壁後，是否要摧毀自己？ (子彈通常勾選，刀光不勾選)")]
    public bool destroyOnHit = true;

    [Tooltip("打中後，幾秒後摧毀自己?")]
    public float destroyDelay = 0;

    [Header("廣播事件 (選填)")]
    [Tooltip("打中目標的瞬間觸發 (可用來播放子彈爆炸特效或音效)")]
    public UnityEvent OnHitSuccess;

    private void Awake()
    {
        // 防呆：確保 Collider 是 Trigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // --- 1. 優先檢查是否撞到牆壁/地板 ---
        // 利用位元運算檢查對方的 Layer 是否包含在我們設定的 obstacleLayer 中
        if ((obstacleLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            if (destroyOnHit)
            {
                Destroy(gameObject, destroyDelay);
            }
            return; // 撞到牆就直接結束程式，不再往下判斷
        }

        // --- 2. 檢查是否撞到目標 (例如 Enemy) ---
        if (other.CompareTag(targetTag))
        {
            // 嘗試獲取對方身上的資源管理器
            ResourceManager targetResource = other.GetComponent<ResourceManager>();

            if (targetResource != null)
            {
                // 執行數值修改 (精準扣血！)
                targetResource.ModifyResource(targetResourceKey, modifyAmount);
                // 廣播打擊成功事件
                OnHitSuccess.Invoke();
                //Debug.Log($"[{gameObject.name}] 打到了 {other.name}");
            }
            else
            {
                // 防呆提示：提醒學生雖然撞對了人，但對方沒有血條可以扣
                Debug.LogWarning($"[{gameObject.name}] 打到了 {other.name}，但對方身上沒有 ResourceManager！");
            }

            // 無論對方有沒有 ResourceManager，只要撞到了目標 Tag，就處理自身銷毀
            if (destroyOnHit)
            {
                Destroy(gameObject, destroyDelay);
            }
        }
    }
}