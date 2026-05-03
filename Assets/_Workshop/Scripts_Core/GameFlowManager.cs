using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameFlowManager : MonoBehaviour
{
    [Header("遊戲狀態廣播 (用來觸發 UI 顯示與隱藏)")]
    public UnityEvent OnGameStart;
    public UnityEvent OnGamePaused;
    public UnityEvent OnGameResumed;
    public UnityEvent OnGameOver;
    public UnityEvent OnLevelComplete;

    // 內部狀態記憶
    private bool isPaused = false;

    //用來鎖死狀態，防止死掉後還能按暫停
    private bool isGameEnded = false;

    private static Vector3? globalRespawnPoint = null;

    void Start()
    {
        Time.timeScale = 1f;
        isPaused = false;
        isGameEnded = false; // 初始化確保沒被鎖住

        //場景一開始，檢查是否有存檔的重生點，有的話把玩家抓過去
        if (globalRespawnPoint.HasValue)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = globalRespawnPoint.Value;
            }
        }

        OnGameStart.Invoke();
    }

    //給 RoomTrigger 呼叫的方法，用來更新重生點
    public static void UpdateRespawnPoint(Vector3 newPoint)
    {
        globalRespawnPoint = newPoint;
        Debug.Log($"[GameFlowManager] 重生點已更新至: {newPoint}");
    }

    //清除存檔的重生點 (如果玩家回到主選單或切換到全新關卡時可以用)
    public void ClearRespawnPoint()
    {
        globalRespawnPoint = null;
    }

    //核心控制方法 (給 UnityEvent 或按鍵呼叫)

    /// <summary>
    /// 切換暫停/恢復狀態 (適合綁定在 Esc 鍵)
    /// </summary>
    public void TogglePause()
    {
        //如果遊戲已經結束(死掉或贏了)，直接無視暫停按鍵
        if (isGameEnded) return;

        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (isGameEnded) return; // 雙重防護

        isPaused = true;
        Time.timeScale = 0f;
        OnGamePaused.Invoke();
    }

    public void ResumeGame()
    {
        if (isGameEnded) return; // 雙重防護

        isPaused = false;
        Time.timeScale = 1f;
        OnGameResumed.Invoke();
    }

    /// <summary>
    /// 觸發遊戲結束 (適合接在主角的 HP OnResourceEmpty 上)
    /// </summary>
    public void TriggerGameOver()
    {
        // 避免被重複觸發多次 (例如被兩根尖刺同時戳到)
        if (isGameEnded) return;

        isGameEnded = true;
        OnGameOver.Invoke();
        Time.timeScale = 0f;
    }

    /// <summary>
    /// 觸發關卡勝利 (適合接在走到終點的 AreaEventTrigger 上)
    /// </summary>
    public void TriggerLevelComplete()
    {
        if (isGameEnded) return;

        isGameEnded = true;
        OnLevelComplete.Invoke();
        Time.timeScale = 0f;
    }

    //場景切換方法

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void ReloadCurrentScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}