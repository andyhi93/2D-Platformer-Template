using UnityEngine;
using UnityEngine.Events;

public class SignalListener : MonoBehaviour
{
    [Header("要監聽的信號名稱")]
    [Tooltip("請輸入與發射端完全相同的字串，例如：GravityFlip")]
    public string targetSignal = "GravityFlip";

    [Header("當收到信號時要執行的事件")]
    public UnityEvent OnSignalReceived;

    void Start()
    {
        // 遊戲開始時，自動向廣播站註冊「我要監聽這個訊號」
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.Subscribe(targetSignal, TriggerEvent);
        }
    }

    void OnDestroy()
    {
        // 當敵人死亡被銷毀時，務必取消註冊，否則電台會報錯！
        if (SignalManager.Instance != null)
        {
            SignalManager.Instance.Unsubscribe(targetSignal, TriggerEvent);
        }
    }

    private void TriggerEvent()
    {
        // 將 C# 內部的廣播，轉換成 Inspector 面板上的 UnityEvent
        OnSignalReceived.Invoke();
    }
}