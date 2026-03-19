using UnityEngine;
using UnityEngine.SceneManagement; // 處理關卡切換必備
using UnityEngine.Events;

public class GameFlowManager : MonoBehaviour
{
    [Header("遊戲狀態廣播 (用來觸發 UI 顯示與隱藏)")]
    [Tooltip("關卡載入完成，遊戲開始時觸發")]
    public UnityEvent OnGameStart;

    [Tooltip("遊戲暫停時觸發 (可用來打開暫停選單 UI)")]
    public UnityEvent OnGamePaused;

    [Tooltip("遊戲恢復時觸發 (可用來關閉暫停選單 UI)")]
    public UnityEvent OnGameResumed;

    [Tooltip("遊戲失敗時觸發 (可用來打開 Game Over UI)")]
    public UnityEvent OnGameOver;

    [Tooltip("關卡過關時觸發 (可用來打開勝利結算 UI)")]
    public UnityEvent OnLevelComplete;

    // 內部狀態記憶
    private bool isPaused = false;

    void Start()
    {
        // 極度重要防呆：確保每次進入新關卡時，時間都是正常流動的
        Time.timeScale = 1f;
        isPaused = false;

        // 廣播遊戲開始 (如果有些機關或動畫要開局才運作，可以接在這裡)
        OnGameStart.Invoke();
    }

    //核心控制方法 (給 UnityEvent 或按鍵呼叫)

    /// <summary>
    /// 切換暫停/恢復狀態 (適合綁定在 Esc 鍵)
    /// </summary>
    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // 凍結物理運算與 Update 時間
        OnGamePaused.Invoke();
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // 恢復時間
        OnGameResumed.Invoke();
    }

    /// <summary>
    /// 觸發遊戲結束 (適合接在主角的 HP OnResourceEmpty 上)
    /// </summary>
    public void TriggerGameOver()
    {
        OnGameOver.Invoke();
        Time.timeScale = 0f; 
    }

    /// <summary>
    /// 觸發關卡勝利 (適合接在走到終點的 AreaEventTrigger 上)
    /// </summary>
    public void TriggerLevelComplete()
    {
        OnLevelComplete.Invoke();
        Time.timeScale = 0f; // 勝利通常會凍結背景
    }

    //場景切換方法

    /// <summary>
    /// 載入指定名稱的關卡
    /// </summary>
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f; // 跨場景前務必解凍時間
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 重新載入當前關卡 (Restart)
    /// </summary>
    public void ReloadCurrentScene()
    {
        Time.timeScale = 1f; // 跨場景前務必解凍時間
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// 離開遊戲
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();

        // 這行讓你在 Unity 編輯器測試時也能發揮作用，不用傻傻等
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}