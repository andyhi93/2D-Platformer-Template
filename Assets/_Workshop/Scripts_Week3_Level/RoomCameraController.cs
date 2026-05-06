using UnityEngine;
using System.Collections;

public class RoomCameraController : MonoBehaviour
{
    public static RoomCameraController Instance { get; private set; }

    [Header("鏡頭切換設定")]
    public TransitionMode mode = TransitionMode.SmoothLerp;
    public float transitionSpeed = 8f;

    [Header("邊界死亡設定 (學員專用)")]
    public bool killPlayerOutOfBounds = false;
    public float outOfBoundsTolerance = 0.1f;

    public enum TransitionMode { Instant, SmoothLerp }

    private bool isTransitioning = false;
    private Coroutine transitionCoroutine;
    private Transform playerTransform;
    private Camera cam;
    private GameFlowManager gameFlowManager;
    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;
        gameFlowManager = FindAnyObjectByType<GameFlowManager>();
    }

    /// <summary>
    /// 給 RoomTrigger 呼叫的方法：開始 Celeste 風格的過場
    /// </summary>
    public void MoveCameraTo(Vector2 newTargetXY)
    {
        if (isTransitioning) return;

        Vector3 targetPos = new Vector3(newTargetXY.x, newTargetXY.y, transform.position.z);
        transitionCoroutine = StartCoroutine(RoomTransitionRoutine(targetPos));
    }
    private IEnumerator RoomTransitionRoutine(Vector3 targetPos)
    {
        isTransitioning = true;

        // 1. 【暫停世界】
        // 這樣怪就不會動，玩家也不會亂跑，也不會觸發任何受擊震動
        Time.timeScale = 0f;

        // 2. 【移動相機】
        if (mode == TransitionMode.Instant)
        {
            transform.position = targetPos;
        }
        else if (mode == TransitionMode.SmoothLerp)
        {
            // 當距離目標還很遠時，持續移動
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                // ⚠️ 關鍵：因為 timeScale 是 0，這裡必須用 Time.unscaledDeltaTime
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.unscaledDeltaTime * transitionSpeed);
                yield return null; // 等待下一個 frame
            }
            // 確保最後精準對齊
            transform.position = targetPos;
        }

        // 3. 【更新震動腳本的原點】(回答你的第一個問題！)
        // 等相機確實停在新的房間中心後，告訴 CameraImpulse 這裡就是新家
        if (CameraImpulse.Instance != null)
        {
            CameraImpulse.Instance.UpdateOriginalPosition(transform.position);
        }

        // 4. 【恢復世界】
        Time.timeScale = 1f;
        isTransitioning = false;
    }

    void LateUpdate()
    {
        // 安全的邊界死亡判定 (只有在沒有轉場時才判定)
        if (killPlayerOutOfBounds && !isTransitioning && playerTransform != null && gameFlowManager != null)
        {
            Vector3 viewportPos = cam.WorldToViewportPoint(playerTransform.position);
            if (viewportPos.x < 0 - outOfBoundsTolerance || viewportPos.x > 1 + outOfBoundsTolerance ||
                viewportPos.y < 0 - outOfBoundsTolerance || viewportPos.y > 1 + outOfBoundsTolerance)
            {
                Debug.Log("玩家飛出畫面，觸發死亡！");
                gameFlowManager.TriggerGameOver();
            }
        }
    }
    /// <summary>
    /// 強制解除轉場狀態 (給傳送門專用，中斷目前的過場並恢復時間)
    /// </summary>
    public void ResetCooldown()
    {
        // 如果過場動畫演到一半被傳送門打斷，我們必須強制停掉它
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
            transitionCoroutine = null;
        }
        // 重置狀態，允許下一次的鏡頭移動
        isTransitioning = false;

        // 不然玩家傳送完會發現整個世界永遠停住了
        Time.timeScale = 1f;
    }
}