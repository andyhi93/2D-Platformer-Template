using UnityEngine;
using UnityEngine.Events;

public class PlayerInputController : MonoBehaviour
{
    [Header("廣播事件 (UnityEvents)")]
    [Tooltip("當按下跳躍鍵時觸發")]
    public UnityEvent OnJumpPressed;

    [Tooltip("傳遞水平與垂直輸入值 (-1 到 1)")]
    public UnityEvent<Vector2> OnMovementInput;

    void Update()
    {
        // 1. 處理移動輸入
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // 廣播當前的輸入向量
        OnMovementInput.Invoke(new Vector2(moveX, moveY));

        // 2. 處理跳躍輸入
        if (Input.GetButtonDown("Jump"))
        {
            OnJumpPressed.Invoke();
        }
    }
}