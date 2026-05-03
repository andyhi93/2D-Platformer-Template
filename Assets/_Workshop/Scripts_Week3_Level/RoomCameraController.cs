using UnityEngine;

public class RoomCameraController : MonoBehaviour
{
    public static RoomCameraController Instance { get; private set; }

    [Header("鏡頭切換設定")]
    public TransitionMode mode = TransitionMode.SmoothLerp;
    public float transitionSpeed = 8f;

    [Tooltip("轉場冷卻時間(秒)：避免玩家在邊界反覆橫跳導致鏡頭鬼畜")]
    public float transitionCooldown = 0.5f;

    [Header("邊界死亡設定 (學員專用)")]
    [Tooltip("勾選後，玩家只要離開攝影機畫面範圍就會觸發 GameOver")]
    public bool killPlayerOutOfBounds = false;
    [Tooltip("容錯範圍(0~1)，0.1代表容許玩家稍微飛出畫面邊緣10%才死")]
    public float outOfBoundsTolerance = 0.1f;

    public enum TransitionMode { Instant, SmoothLerp }

    private Vector3 targetPosition;
    private bool isTransitioning = false;
    private float cooldownTimer = 0f;
    private Transform playerTransform;
    private Camera cam;
    private GameFlowManager gameFlowManager; // 抓取全域管理器

    void Awake()
    {
        Instance = this;
        targetPosition = transform.position;
        cam = GetComponent<Camera>();
    }

    void Start()
    {
        // 快取玩家和 GameFlowManager，避免每幀尋找效能過低
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTransform = player.transform;

        gameFlowManager = FindAnyObjectByType<GameFlowManager>();
    }

    /// <summary>
    /// 給 RoomTrigger 呼叫的方法：設定新的鏡頭目標點
    /// </summary>
    public void MoveCameraTo(Vector2 newTargetXY)
    {
        // 【防抖動核心】如果還在冷卻時間內，無視新的切換請求
        if (isTransitioning) return;

        targetPosition = new Vector3(newTargetXY.x, newTargetXY.y, transform.position.z);

        // 觸發冷卻時間
        isTransitioning = true;
        cooldownTimer = transitionCooldown;
    }

    void LateUpdate()
    {
        // 1. 處理轉場冷卻計時
        if (isTransitioning)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                isTransitioning = false; // 冷卻結束，可以再次切換房間
            }
        }

        // 2. 執行相機移動
        if (transform.position != targetPosition)
        {
            if (mode == TransitionMode.Instant)
                transform.position = targetPosition;
            else if (mode == TransitionMode.SmoothLerp)
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * transitionSpeed);
        }

        // 3. 【安全的邊界死亡判定】
        // 只有在「開啟功能」、「玩家存活」、且「相機沒有在轉場」時才判定！
        if (killPlayerOutOfBounds && !isTransitioning && playerTransform != null && gameFlowManager != null)
        {
            // 將玩家的世界座標轉換為相機視角比例 (0~1 之間代表在畫面內)
            Vector3 viewportPos = cam.WorldToViewportPoint(playerTransform.position);

            // 加上容錯值 (Tolerance) 進行判斷
            if (viewportPos.x < 0 - outOfBoundsTolerance || viewportPos.x > 1 + outOfBoundsTolerance ||
                viewportPos.y < 0 - outOfBoundsTolerance || viewportPos.y > 1 + outOfBoundsTolerance)
            {
                Debug.Log("玩家飛出畫面，觸發死亡！");
                gameFlowManager.TriggerGameOver(); // 呼叫你原本寫好的 GameFlowManager[cite: 1]
            }
        }
    }

    /// <summary>
    /// 強制解除轉場冷卻 (給傳送門專用，確保目的地房間一定能接管相機)
    /// </summary>
    public void ResetCooldown()
    {
        isTransitioning = false;
        cooldownTimer = 0f;
    }
}