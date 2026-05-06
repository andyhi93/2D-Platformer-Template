using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ScoreData
{
    [Tooltip("記分項目名稱，例如：Kills, Coins, Keys")]
    public string scoreKey;

    [Tooltip("遊戲初始分數 (通常為 0)")]
    public int initialValue = 0;

    [Tooltip("【重要設定】是否要跨關卡保留？\n勾選(金幣)：死掉重載會回到上次過關的金額\n不勾(鑰匙)：死掉重載直接回到 initialValue")]
    public bool isPersistent = false;

    [Tooltip("目標數量 (達到此數量會觸發事件，填 0 代表無上限純記分)")]
    public int targetValue = 0;

    [Tooltip("分數變動時廣播 (傳遞當前分數，用來接 ResourceDisplay 更新 UI)")]
    public UnityEvent<int> OnScoreChanged;

    [Tooltip("達到目標數量時觸發 (用來接通關、開門、生怪等)")]
    public UnityEvent OnTargetReached;

    // 隱藏變數：當前實際分數
    [HideInInspector] public int currentValue = 0;

    // 隱藏變數：防止達標事件被重複觸發
    [HideInInspector] public bool hasReachedTarget = false;
}

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("目標與分數配置區")]
    public List<ScoreData> scoreObjectives = new List<ScoreData>();

    private Dictionary<string, ScoreData> scoreDict = new Dictionary<string, ScoreData>();

    // 核心魔法：全域靜態記憶體。只要遊戲沒關掉，裡面的資料就會一直活著，不怕切換場景！
    private static Dictionary<string, int> persistentSavedScores = new Dictionary<string, int>();

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        foreach (var score in scoreObjectives)
        {
            if (!scoreDict.ContainsKey(score.scoreKey))
            {
                // ==========================================
                // 讀檔邏輯：判斷這個分數需不需要「繼承」之前的記憶
                // ==========================================
                if (score.isPersistent)
                {
                    // 如果全域記憶體裡有存過這個項目，就讀檔；沒有的話，就把初始值存進去當第一筆紀錄
                    if (persistentSavedScores.TryGetValue(score.scoreKey, out int savedVal))
                    {
                        score.currentValue = savedVal;
                    }
                    else
                    {
                        score.currentValue = score.initialValue;
                        persistentSavedScores[score.scoreKey] = score.currentValue;
                    }
                }
                else
                {
                    // 如果不保留 (例如這關的鑰匙)，每次載入場景都乖乖回到初始值
                    score.currentValue = score.initialValue;
                }

                scoreDict.Add(score.scoreKey, score);
            }
        }
    }

    void Start()
    {
        // 遊戲開始時，廣播一次當前數值，讓 UI 更新畫面
        foreach (var score in scoreDict.Values)
        {
            score.OnScoreChanged.Invoke(score.currentValue);
        }
    }

    /// <summary>
    /// 加 1 分 (給 UnityEvent 呼叫)
    /// </summary>
    public void AddOneScore(string key) { AddScore(key, 1); }

    /// <summary>
    /// 核心加分邏輯
    /// </summary>
    public void AddScore(string key, int amount)
    {
        if (scoreDict.TryGetValue(key, out ScoreData score))
        {
            score.currentValue += amount;
            score.OnScoreChanged.Invoke(score.currentValue);

            if (score.targetValue > 0 && score.currentValue >= score.targetValue && !score.hasReachedTarget)
            {
                score.hasReachedTarget = true;
                score.OnTargetReached.Invoke();
            }
        }
    }

    // ==========================================
    // 存檔與重置管理區
    // ==========================================

    /// <summary>
    /// 【過關存檔】在走到下一關的傳送門時呼叫！
    /// 它會把目前的金幣數量寫入靜態記憶體中。
    /// </summary>
    public void SavePersistentScores()
    {
        foreach (var score in scoreDict.Values)
        {
            if (score.isPersistent)
            {
                persistentSavedScores[score.scoreKey] = score.currentValue;
            }
        }
        Debug.Log("[ScoreManager] 已儲存所有跨關卡分數進度！");
    }

    /// <summary>
    /// 【徹底重置】如果玩家回到遊戲主選單，或者你希望一切從零開始時呼叫。
    /// </summary>
    public void ClearAllPersistentScores()
    {
        persistentSavedScores.Clear();

        foreach (var score in scoreDict.Values)
        {
            score.currentValue = score.initialValue;
            score.hasReachedTarget = false;
            score.OnScoreChanged.Invoke(score.currentValue);
        }
        Debug.Log("[ScoreManager] 已徹底清空所有全域分數記憶！");
    }

    public int GetScore(string key)
    {
        if (scoreDict.TryGetValue(key, out ScoreData score)) return score.currentValue;
        return 0;
    }
}