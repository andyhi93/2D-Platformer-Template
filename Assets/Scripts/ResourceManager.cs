using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// 宣告一個可序列化的類別，用來在 Inspector 面板中顯示
[System.Serializable]
public class ResourceData
{
    [Tooltip("資源的字串鍵值，例如：HP, Coin, Mana")]
    public string resourceKey;

    [Tooltip("遊戲開始時的初始數值")]
    public int initialValue;

    // 隱藏在面板中，由程式內部控管當前狀態
    [HideInInspector]
    public int currentValue;

    [Tooltip("當此數值發生增減時，向外廣播 (傳遞變動後的當前值)")]
    public UnityEvent<int> OnValueChanged;
}

public class ResourceManager : MonoBehaviour
{
    [Header("資源配置區 (可自由新增多種資源)")]
    public List<ResourceData> startingResources = new List<ResourceData>();

    // 內部運算使用的核心字典，確保查找效能為 O(1)
    private Dictionary<string, ResourceData> resourceDict = new Dictionary<string, ResourceData>();

    void Awake()
    {
        // 將面板設定的 List 轉換為 Runtime 的 Dictionary
        foreach (var res in startingResources)
        {
            res.currentValue = res.initialValue;

            // 防呆機制：避免學生不小心打了兩個一樣的 Key
            if (!resourceDict.ContainsKey(res.resourceKey))
            {
                resourceDict.Add(res.resourceKey, res);
            }
            else
            {
                Debug.LogWarning($"[{gameObject.name}] 發現重複的資源鍵值: {res.resourceKey}，請檢查設定！");
            }
        }
    }

    void Start()
    {
        // 遊戲開始時，主動廣播一次所有資源的初始值
        // 這樣可以確保 UI 介面一開始就能顯示正確的數字，而不會是預設的 "Text"
        foreach (var res in resourceDict.Values)
        {
            res.OnValueChanged.Invoke(res.currentValue);
        }
    }

    /// <summary>
    /// 公開方法：修改指定資源的數值 (給 AreaEventTrigger 等觸發器呼叫)
    /// </summary>
    /// <param name="key">資源鍵值 (例如 "HP")</param>
    /// <param name="amount">變動量 (正數為增加，負數為減少)</param>
    public void ModifyResource(string key, int amount)
    {
        if (resourceDict.TryGetValue(key, out ResourceData res))
        {
            res.currentValue += amount;
            res.OnValueChanged.Invoke(res.currentValue); // 廣播最新數值給 UI 或特效
        }
        else
        {
            Debug.LogWarning($"嘗試修改不存在的資源: {key}。請確認字串是否拼錯。");
        }
    }

    /// <summary>
    /// 公開方法：取得當前資源數值 (給進階邏輯判斷用，例如：魔力大於 50 才能開門)
    /// </summary>
    public int GetResource(string key)
    {
        if (resourceDict.TryGetValue(key, out ResourceData res))
        {
            return res.currentValue;
        }
        return 0;
    }
}