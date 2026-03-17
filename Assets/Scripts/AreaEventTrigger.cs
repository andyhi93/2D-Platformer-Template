using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class AreaEventTrigger : MonoBehaviour
{
    [Header("觸發條件設定")]
    [Tooltip("只有帶有此標籤的物件進入才會觸發 (例如: Player 或 Enemy)")]
    public string targetTag = "Player";

    [Tooltip("勾選後，此觸發器只會生效一次 (適用於：吃金幣、單次對話、過關判定)")]
    public bool triggerOnlyOnce = false;

    [Header("廣播事件 (UnityEvents)")]
    public UnityEvent OnTriggerEntered;
    public UnityEvent OnTriggerExited;

    // 內部狀態紀錄
    private bool hasTriggered = false;

    private void Awake()
    {
        // 防呆機制：確保掛載此腳本的 Collider2D 有勾選 IsTrigger
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"[{gameObject.name}] 的 AreaEventTrigger 需要 IsTrigger = true，已自動勾選。");
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 如果設定為單次觸發，且已經觸發過，則直接攔截
        if (triggerOnlyOnce && hasTriggered) return;

        // 使用 CompareTag 效能比 directly accessing other.tag 更好
        if (other.CompareTag(targetTag))
        {
            hasTriggered = true;
            OnTriggerEntered.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 如果是單次觸發的物件（如吃掉的金幣），通常不需要處理離開事件
        if (triggerOnlyOnce) return;

        if (other.CompareTag(targetTag))
        {
            OnTriggerExited.Invoke();
        }
    }

    /// <summary>
    /// 公開方法：讓外部事件可以重置此觸發器 (例如：玩家死亡重生後，金幣要能再次被吃)
    /// </summary>
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}