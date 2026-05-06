using System;
using System.Collections.Generic;
using UnityEngine;

public class SignalManager : MonoBehaviour
{
    public static SignalManager Instance { get; private set; }

    // 內部使用 C# 原生的 Action 來儲存訂閱者 (效能好且安全)
    private Dictionary<string, Action> signalDictionary = new Dictionary<string, Action>();

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    /// <summary>
    /// 發射信號：讓玩家的 UnityEvent 呼叫這個方法
    /// </summary>
    public void SendSignal(string signalName)
    {
        if (signalDictionary.TryGetValue(signalName, out Action thisSignal))
        {
            thisSignal?.Invoke();
            Debug.Log($"[SignalManager] 發送全域廣播信號: {signalName}");
        }
    }

    // ==========================================
    // 以下給 Listener 腳本在內部自動呼叫，學員不需手動操作
    // ==========================================
    public void Subscribe(string signalName, Action listener)
    {
        if (!signalDictionary.ContainsKey(signalName))
        {
            signalDictionary.Add(signalName, null);
        }
        signalDictionary[signalName] += listener;
    }

    public void Unsubscribe(string signalName, Action listener)
    {
        if (signalDictionary.ContainsKey(signalName))
        {
            signalDictionary[signalName] -= listener;
        }
    }
}