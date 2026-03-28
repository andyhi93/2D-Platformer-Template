using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ResourceData
{
    [Tooltip("資源的字串鍵值，例如：HP, Coin, Mana")]
    public string resourceKey;

    [Tooltip("遊戲開始時的初始數值")]
    public int initialValue;

    [Tooltip("數值上限 (填 0 表示無上限，適合用在金幣)")]
    public int maxValue = 0;

    // --- 新增：無敵時間設定 ---
    [Tooltip("扣減此數值時，是否觸發無敵時間？(通常用於 HP)")]
    public bool triggerInvincibility = false;

    [Tooltip("無敵時間長度 (秒)")]
    public float iFrameDuration = 1.0f;
    // ------------------------

    [Tooltip("遊戲當前數值")]
    public int currentValue;

    [Tooltip("當此數值發生增減時，向外廣播 (傳遞變動後的當前值，用來更新 UI)")]
    public UnityEvent<int> OnValueChanged;

    [Tooltip("當數值「減少」時廣播 (專門用來接受擊閃紅、相機震動、受傷音效)")]
    public UnityEvent OnDamageTaken;

    [Tooltip("當數值歸零時觸發 (例如：HP歸零=死亡，Mana歸零=無法施法)")]
    public UnityEvent OnResourceEmpty;
}

public class ResourceManager : MonoBehaviour
{
    [Header("資源配置區 (可自由新增多種資源)")]
    public List<ResourceData> startingResources = new List<ResourceData>();

    [Header("狀態廣播")]
    [Tooltip("當進入無敵狀態時廣播 (傳遞無敵的秒數)，可用來接閃爍特效")]
    public UnityEvent<float> OnInvincibilityStarted;

    private Dictionary<string, ResourceData> resourceDict = new Dictionary<string, ResourceData>();

    // 內部記憶體：紀錄目前是否處於無敵狀態
    private bool isInvincible = false;

    void Awake()
    {
        foreach (var res in startingResources)
        {
            res.currentValue = res.initialValue;

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
        foreach (var res in resourceDict.Values)
        {
            res.OnValueChanged.Invoke(res.currentValue);
        }
    }

    public void ModifyResource(string key, int amount)
    {
        if (resourceDict.TryGetValue(key, out ResourceData res))
        {
            // 無敵狀態防護
            if (amount < 0 && isInvincible) return;

            res.currentValue += amount;

            // 數值範圍限制
            if (res.maxValue > 0) res.currentValue = Mathf.Clamp(res.currentValue, 0, res.maxValue);
            else res.currentValue = Mathf.Max(0, res.currentValue);

            // 1. 廣播給 UI 更新 (開局也會呼叫這裡，但現在特效不會綁在這裡了)
            res.OnValueChanged.Invoke(res.currentValue);

            // 2. 歸零判定
            if (res.currentValue <= 0)
            {
                res.OnResourceEmpty.Invoke();
            }

            // 專屬的受擊廣播
            if (amount < 0)
            {
                // 只要是扣血，就觸發受擊事件 (用來接閃紅光、震動)
                res.OnDamageTaken.Invoke();

                // 觸發無敵時間
                if (res.triggerInvincibility && res.currentValue > 0)
                {
                    StartCoroutine(InvincibilityRoutine(res.iFrameDuration));
                }
            }
        }
        else
        {
            Debug.LogWarning($"嘗試修改不存在的資源: {key}。請確認字串是否拼錯。");
        }
    }

    public int GetResource(string key)
    {
        if (resourceDict.TryGetValue(key, out ResourceData res))
        {
            return res.currentValue;
        }
        return 0;
    }

    //無敵計時器 (協程) ---
    private IEnumerator InvincibilityRoutine(float duration)
    {
        isInvincible = true;                     // 鎖上無敵護盾
        OnInvincibilityStarted.Invoke(duration); // 告訴外界「我無敵了，請幫我播閃爍特效」

        yield return new WaitForSeconds(duration); // 等待 N 秒

        isInvincible = false;                    // 解除無敵護盾
    }

    public void DestroySelf() { Destroy(gameObject); }
    public void DestroySelfWithDelay(float delay) { Destroy(gameObject, delay); }
    public void DisableComponent(MonoBehaviour componentToDisable) { if (componentToDisable != null) componentToDisable.enabled = false; }
}