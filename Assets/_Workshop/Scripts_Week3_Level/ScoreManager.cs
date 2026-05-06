using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ScoreData
{
    [Tooltip("記分項目名稱，例如：Kills, Coins, Keys")]
    public string scoreKey;

    [Tooltip("起始數量 (通常為 0)")]
    public int currentValue = 0;

    [Tooltip("目標數量 (達到此數量會觸發事件，填 0 代表無上限純記分)")]
    public int targetValue = 10;

    [Tooltip("分數變動時廣播 (傳遞當前分數，用來接 ResourceDisplay 更新 UI)")]
    public UnityEvent<int> OnScoreChanged;

    [Tooltip("達到目標數量時觸發 (用來接通關、開門、生怪等)")]
    public UnityEvent OnTargetReached;

    // 內部記憶體，防止達標事件被重複觸發
    [HideInInspector]
    public bool hasReachedTarget = false;
}

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("目標與分數配置區")]
    public List<ScoreData> scoreObjectives = new List<ScoreData>();

    private Dictionary<string, ScoreData> scoreDict = new Dictionary<string, ScoreData>();

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
        // 將 List 轉換為 Dictionary，方便後續用字串 (Key) 快速尋找
        foreach (var score in scoreObjectives)
        {
            if (!scoreDict.ContainsKey(score.scoreKey))
            {
                scoreDict.Add(score.scoreKey, score);
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] 發現重複的記分鍵值: {score.scoreKey}，請檢查設定！");
            }
        }
    }

    void Start()
    {
        // 遊戲開始時，廣播一次初始數值，讓 UI 顯示為 0
        foreach (var score in scoreDict.Values)
        {
            score.OnScoreChanged.Invoke(score.currentValue);
        }
    }

    /// <summary>
    /// 給 UnityEvent 呼叫的便利方法 (預設加 1 分)
    /// 適合綁在敵人的死亡事件、金幣的拾取事件上
    /// </summary>
    public void AddOneScore(string key)
    {
        AddScore(key, 1);
    }

    /// <summary>
    /// 核心加分邏輯 (可自訂加分數量)
    /// </summary>
    public void AddScore(string key, int amount)
    {
        if (scoreDict.TryGetValue(key, out ScoreData score))
        {
            // 已經達標就不再處理邏輯 (如果希望達標後還能繼續記分，可將這行註解)
            // if (score.hasReachedTarget) return; 

            score.currentValue += amount;

            // 1. 廣播給 UI 更新
            score.OnScoreChanged.Invoke(score.currentValue);

            // 2. 判斷是否達到目標條件 (且目標值大於 0 才算數)
            if (score.targetValue > 0 && score.currentValue >= score.targetValue && !score.hasReachedTarget)
            {
                score.hasReachedTarget = true;
                score.OnTargetReached.Invoke();
                Debug.Log($"[{gameObject.name}] 目標達成: {key}!");
            }
        }
        else
        {
            Debug.LogWarning($"嘗試增加不存在的分數項目: {key}。請確認字串是否拼錯。");
        }
    }

    /// <summary>
    /// 讓企劃/學員可以直接從外部讀取目前分數
    /// </summary>
    public int GetScore(string key)
    {
        if (scoreDict.TryGetValue(key, out ScoreData score))
        {
            return score.currentValue;
        }
        return 0;
    }
}