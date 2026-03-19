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

    void Update()
    {
        //處理移動輸入
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // 廣播當前的輸入向量
        OnMovementInput.Invoke(new Vector2(moveX, moveY));
        //隨移動方向轉向，用於攻擊方向判定
        if (moveX > 0)
        {
            // 面向右邊，Y 軸轉回 0 度
            transform.eulerAngles = new Vector3(0, 0, 0);
        }
        else if (moveX < 0)
        {
            // 面向左邊，Y 軸轉到 180 度
            transform.eulerAngles = new Vector3(0, 180, 0);
        }

        //處理跳躍輸入
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
    }
}