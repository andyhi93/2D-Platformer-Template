using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TimeNode
{
    [Tooltip("節點名稱 (用來給 SwitchTimer 呼叫，例如：Phase1, Wave2)")]
    public string nodeName = "New Node";

    [Tooltip("這段時間軸的持續時間 (秒)")]
    public float duration = 5f;

    [Header("生命週期事件")]
    [Tooltip("【開始】進入此節點的第一幀觸發 (適合：生成敵人、開門、亮警告燈)")]
    public UnityEvent OnEnter;

    [Tooltip("【持續】在此節點運作期間，每一幀都會觸發 (適合：持續扣血、雷射光追蹤)")]
    public UnityEvent OnStay;

    [Tooltip("【離開】當時間到、被 Skip、或被強制 Switch 時觸發 (適合：清理怪物、關掉特效)")]
    public UnityEvent OnExit;

    [Header("完成事件")]
    [Tooltip("【完成】只有當「時間跑完」或被「Skip」時才會觸發！(請把切換下一個節點的 SwitchTimer 放在這裡)")]
    public UnityEvent OnComplete;
}

public class TimeEventManager : MonoBehaviour
{
    [Header("時間軸清單")]
    public List<TimeNode> timeNodes = new List<TimeNode>();

    [Header("啟動設定")]
    [Tooltip("如果勾選，遊戲開始時會自動播放清單中的第一個節點")]
    public bool autoStartFirstNode = false;

    // 內部狀態記憶體
    private TimeNode currentNode = null;
    private float timer = 0f;
    private bool isRunning = false;

    void Start()
    {
        // 遊戲開始時，如果勾選自動播放，就抓清單的第一個名稱來啟動
        if (autoStartFirstNode && timeNodes.Count > 0)
        {
            SwitchTimer(timeNodes[0].nodeName);
        }
    }

    void Update()
    {
        if (!isRunning || currentNode == null) return;

        // 1. 執行持續事件 (每幀)
        currentNode.OnStay.Invoke();

        // 2. 計時器推進
        timer += Time.deltaTime;

        // 3. 判斷時間是否結束
        if (timer >= currentNode.duration)
        {
            CompleteCurrentTimer();
        }
    }

    // ==========================================
    // 公開方法 (給 UnityEvent 或學員呼叫的按鈕)
    // ==========================================

    /// <summary>
    /// 核心功能：切換到指定名稱的時間節點，並重新開始計時
    /// </summary>
    public void SwitchTimer(string nodeName)
    {
        // 先找尋有沒有這個名字的節點
        TimeNode nextNode = timeNodes.Find(node => node.nodeName == nodeName);

        if (nextNode == null)
        {
            Debug.LogWarning($"[TimeEventManager] 找不到名為 '{nodeName}' 的時間節點！請檢查拼字。");
            return;
        }

        // 如果當前有正在跑的節點，先觸發它的 Exit (讓舊節點清理狀態)
        if (currentNode != null)
        {
            currentNode.OnExit.Invoke();
        }

        // 切換並初始化新節點
        currentNode = nextNode;
        timer = 0f;
        isRunning = true;

        // 觸發新節點的 Enter
        currentNode.OnEnter.Invoke();
    }

    /// <summary>
    /// 暫停當前的計時器
    /// </summary>
    public void StopTimer()
    {
        isRunning = false;
    }

    /// <summary>
    /// 恢復當前的計時器
    /// </summary>
    public void ResumeTimer()
    {
        if (currentNode != null)
        {
            isRunning = true;
        }
    }

    /// <summary>
    /// 重置當前的計時器進度 (回到該節點的 0 秒)
    /// </summary>
    public void ResetTimer()
    {
        timer = 0f;
    }

    /// <summary>
    /// 跳過當前節點，瞬間視為「時間到」，並觸發 Complete 串接
    /// </summary>
    public void SkipTimer()
    {
        if (currentNode != null && isRunning)
        {
            CompleteCurrentTimer();
        }
    }

    // ==========================================
    // 內部私有邏輯
    // ==========================================
    private void CompleteCurrentTimer()
    {
        isRunning = false;

        // 暫存目前的節點，這是一個很重要的防呆機制
        TimeNode finishedNode = currentNode;

        // 1. 先觸發 Complete：學員通常會在這裡放 SwitchTimer("下一個節點")
        finishedNode.OnComplete.Invoke();

        // 2. 觸發 Exit：清理這個節點產生的東西
        finishedNode.OnExit.Invoke();

        // 【防呆邏輯】：
        // 如果學員在 OnComplete 裡沒有呼叫 SwitchTimer 換下一個，currentNode 就不會變。
        // 這時代表整個時間軸結束了，我們要把 currentNode 清空，停止 Update。
        // 但如果學員有呼叫 SwitchTimer，currentNode 就已經是新節點了，這時就絕對不能清空！
        if (currentNode == finishedNode)
        {
            currentNode = null;
        }
    }
}