using UnityEngine;
using UnityEngine.Events;

public class PlayerInputController : MonoBehaviour
{
    [Header("廣播事件 (UnityEvents)")]
    [Tooltip("當按下跳躍鍵時觸發")]
    public UnityEvent OnJumpPressed;

    [Tooltip("傳遞水平與垂直輸入值 (-1 到 1)")]
    public UnityEvent<Vector2> OnMovementInput;

    [Tooltip("當按下左鍵時觸發")]
    public UnityEvent OnLeftButtonPressed;
    [Tooltip("當按下右鍵時觸發")]
    public UnityEvent OnRightButtonPressed;
    [Tooltip("當按下 Esc 鍵時觸發")]
    public UnityEvent OnPausePressed;

    [Tooltip("傳遞滑鼠當前的世界座標 (X, Y)")]
    public UnityEvent<Vector2> OnMousePositionUpdated;

    private Camera mainCam;

    void Awake()
    {
        // 預先快取主攝影機，避免在 Update 中頻繁呼叫 Camera.main 造成效能損耗
        mainCam = Camera.main;
    }

    void Update()
    {
        // 處理移動輸入
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // 廣播當前的輸入向量
        OnMovementInput.Invoke(new Vector2(moveX, moveY));

        Vector3 currentEuler = transform.eulerAngles;

        // 隨移動方向轉向，用於攻擊方向判定
        if (moveX > 0)
        {
            // 面向右邊，Y 軸轉回 0 度，X 軸保持不變
            transform.eulerAngles = new Vector3(currentEuler.x, 0, currentEuler.z);
        }
        else if (moveX < 0)
        {
            // 面向左邊，Y 軸轉到 180 度，X 軸保持不變
            transform.eulerAngles = new Vector3(currentEuler.x, 180, currentEuler.z);
        }

        // 處理跳躍輸入
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnJumpPressed.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            OnLeftButtonPressed.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            OnRightButtonPressed.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            OnPausePressed.Invoke();
        }

        // 處理滑鼠世界座標
        if (mainCam != null)
        {
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            // 確保只回傳 2D 平面的 X 與 Y 值，捨棄 Z 軸深度
            OnMousePositionUpdated.Invoke(new Vector2(mouseWorldPos.x, mouseWorldPos.y));
        }
    }
}