// ==========================================
// 檔案名稱：PlayerInputController.cs
// ==========================================
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class KeyBinding
{
    [Tooltip("幫這個按鍵設定一個名稱備註，方便辨識 (例如：跳躍、開火)")]
    public string actionName = "New Action";

    [Tooltip("請輸入按鍵的英文名稱 (不分大小寫)。例如：W, Space, Mouse0, Escape")]
    public string keyName = "Space";

    // 隱藏變數：用來儲存遊戲開始時，從字串轉換成功的實際 KeyCode
    [HideInInspector]
    public KeyCode parsedKey = KeyCode.None;

    [Header("事件觸發器")]
    public UnityEvent OnKeyDown;
    public UnityEvent OnKeyHold;
    public UnityEvent OnKeyUp;
}

public class PlayerInputController : MonoBehaviour
{
    [Header("連續性輸入 (物理移動與瞄準)")]
    public UnityEvent<Vector2> OnMovementInput;
    public UnityEvent<Vector2> OnMousePositionUpdated;

    [Header("強制移動設定 (卡牌擴充)")]
    public bool autoRunRight = false;

    [Header("動態按鍵綁定區")]
    public List<KeyBinding> customKeyBindings = new List<KeyBinding>();

    private Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;

        // --- 字串解析：將學員輸入的文字轉為 Unity 看得懂的 KeyCode ---
        foreach (var binding in customKeyBindings)
        {
            // 使用 Enum.TryParse 嘗試轉換，忽略大小寫 (true)
            if (System.Enum.TryParse(binding.keyName, true, out KeyCode result))
            {
                binding.parsedKey = result;
            }
            else
            {
                // 如果打錯字，在 Console 報錯並將按鍵設為 None
                Debug.LogError($"[{gameObject.name}] 按鍵綁定錯誤：找不到名為 '{binding.keyName}' 的按鍵，請檢查拼字！");
                binding.parsedKey = KeyCode.None;
            }
        }
    }

    void Update()
    {
        // --- 1. 處理物理移動與轉向 ---
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        if (autoRunRight) moveX = 1f;

        OnMovementInput.Invoke(new Vector2(moveX, moveY));

        Vector3 currentEuler = transform.eulerAngles;

        if (moveX > 0)
        {
            transform.eulerAngles = new Vector3(currentEuler.x, 0, currentEuler.z);
        }
        else if (moveX < 0)
        {
            transform.eulerAngles = new Vector3(currentEuler.x, 180, currentEuler.z);
        }

        // --- 2. 處理動態按鍵陣列 ---
        foreach (var binding in customKeyBindings)
        {
            // 如果按鍵解析失敗(None)，則跳過不執行
            if (binding.parsedKey == KeyCode.None) continue;

            if (Input.GetKeyDown(binding.parsedKey))
            {
                binding.OnKeyDown.Invoke();
            }

            if (Input.GetKey(binding.parsedKey))
            {
                binding.OnKeyHold.Invoke();
            }

            if (Input.GetKeyUp(binding.parsedKey))
            {
                binding.OnKeyUp.Invoke();
            }
        }

        // --- 3. 處理滑鼠世界座標 ---
        if (mainCam != null)
        {
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(Input.mousePosition);
            OnMousePositionUpdated.Invoke(new Vector2(mouseWorldPos.x, mouseWorldPos.y));
        }
    }
}