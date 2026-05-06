using UnityEngine;

public class ScoreTrigger : MonoBehaviour
{
    [Header("預設記分設定 (給無參數函式使用)")]
    [Tooltip("要加分的項目名稱，例如：Coins, Kills")]
    public string defaultScoreKey = "Coins";

    [Tooltip("要增加的數量 (吃大金幣可以填 5，扣分可以填負數)")]
    public int defaultAmount = 1;

    // ==========================================
    // 👇 給 UnityEvent 呼叫的介面區
    // ==========================================

    /// <summary>
    /// 方法一：快速加 1 分 (UnityEvent 傳入 1 個字串參數)
    /// 適合綁在普通怪物的死亡事件，直接填 "Kills"
    /// </summary>
    public void AddOneScore(string key)
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(key, 1);
        }
        else
        {
            Debug.LogWarning("場景中找不到 ScoreManager！");
        }
    }

    /// <summary>
    /// 方法二：增加自訂數量 (UnityEvent 不傳參數，讀取上方的變數設定)
    /// 適合掛在「大金幣」、「寶箱」身上，學員可以直接在面板設定要給幾分
    /// </summary>
    public void AddPresetScore()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(defaultScoreKey, defaultAmount);
        }
        else
        {
            Debug.LogWarning("場景中找不到 ScoreManager！");
        }
    }

    // ==========================================
    // 👇 給程式碼 (C#) 直接呼叫的進階介面
    // ==========================================

    /// <summary>
    /// 方法三：完整的加分功能 (給其他腳本用程式碼呼叫，Inspector 面板看不到此方法)
    /// </summary>
    public void AddCustomScore(string key, int amount)
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(key, amount);
        }
    }
}