using UnityEngine;
using UnityEngine.Events;

public class SimpleTimer : MonoBehaviour
{
    [Header("計時器設定")]
    [Tooltip("計時長度 (秒)")]
    public float duration = 5f;

    [Tooltip("勾選後，物件一出現就會自動開始計時")]
    public bool autoStart = true;

    [Tooltip("勾選後，計時結束會自動重新開始 (適合做週期性生成或攻擊)")]
    public bool loop = false;

    [Header("狀態廣播 (UnityEvents)")]
    [Tooltip("每秒更新時廣播 (傳遞剩餘的整數秒數，可直接接上 ResourceDisplay 更新 UI)")]
    public UnityEvent<int> OnTimeUpdated;

    [Tooltip("當時間倒數歸零時觸發")]
    public UnityEvent OnTimerEnd;

    // 內部狀態
    private float currentTime;
    private bool isRunning = false;
    private int lastBroadcastTime = -1; // 記錄上次廣播的秒數，作為效能優化的開關

    void Start()
    {
        if (autoStart)
        {
            StartTimer();
        }
    }

    void Update()
    {
        if (!isRunning) return;

        // 倒數邏輯
        currentTime -= Time.deltaTime;

        // 效能優化：將精確時間轉為向上取整的秒數 (例如 4.2 秒會顯示 5)
        int currentSeconds = Mathf.CeilToInt(currentTime);

        // 只有在秒數發生變化，且大於等於 0 時才向外廣播
        if (currentSeconds != lastBroadcastTime && currentSeconds >= 0)
        {
            lastBroadcastTime = currentSeconds;
            OnTimeUpdated.Invoke(currentSeconds);
        }

        // 歸零判定
        if (currentTime <= 0f)
        {
            OnTimerEnd.Invoke();

            if (loop)
            {
                StartTimer(); // 重新開始並重置所有狀態
            }
            else
            {
                isRunning = false;
                currentTime = 0f;
            }
        }
    }

    // --- 控制方法 (給 UnityEvent 呼叫) ---

    public void StartTimer()
    {
        currentTime = duration;
        isRunning = true;
        lastBroadcastTime = -1; // 重置廣播記憶，確保一開始會立刻更新畫面
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    // --- 讀取接口 (給其他 C# 腳本呼叫) ---

    /// <summary>
    /// 取得精確的剩餘時間 (含小數點)
    /// </summary>
    public float GetRemainingTime()
    {
        return Mathf.Max(0f, currentTime);
    }

    /// <summary>
    /// 取得計時器的進度比例 (0 到 1)，1 代表剛開始，0 代表結束。適合用來做 UI 讀條。
    /// </summary>
    public float GetProgressRatio()
    {
        if (duration <= 0f) return 0f;
        return Mathf.Clamp01(currentTime / duration);
    }
}