using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("生成設定")]
    [Tooltip("要生成的物件 (子彈、刀光、灰塵粒子、掉落金幣)")]
    public GameObject prefabToSpawn;

    [Tooltip("物件生成的位置 (請在場景中拉一個空的子物件當作槍口或揮刀點)")]
    public Transform spawnPoint;

    [Header("動態屬性 (選填)")]
    [Tooltip("如果大於 0，物件生成後會自動加上往前的推力 (適合子彈)")]
    public float spawnSpeed = 0f;

    [Tooltip("如果大於 0，物件會在指定秒數後自動銷毀 (適合刀光或特效)")]
    public float autoDestroyTime = 0f;

    [Tooltip("最大隨機散射角度 (例如填入 15，代表會在正負 15 度之間隨機偏移)")]
    public float maxScatterAngle = 0f;

    [Header("頻率限制 (攻速)")]
    [Tooltip("兩次生成之間的最小間隔時間 (秒)。0 代表無限制。")]
    public float cooldownTime = 0.25f;

    [Header("後座力設定 (選填)")]
    [Tooltip("開火或揮刀時，給予自身的反向推力大小 (0 代表無後座力)")]
    public float recoilForce = 0f;

    [Tooltip("承受後座力的目標剛體 (若留空，會自動尋找父物件身上的 Rigidbody2D)")]
    public Rigidbody2D wielderRb;

    // 內部記憶體：紀錄下一次可以生成的時間點
    private float nextSpawnTime = 0f;

    private void Awake()
    {
        // 防呆機制：如果企劃沒有手動拖曳，腳本會嘗試自動往上(父物件)尋找玩家的剛體
        if (wielderRb == null)
        {
            wielderRb = GetComponentInParent<Rigidbody2D>();
        }
    }

    /// <summary>
    /// 公開方法：給 UnityEvent 呼叫，執行生成邏輯
    /// </summary>
    public void Spawn()
    {
        // --- 0. 頻率限制檢查 ---
        // 若當前時間尚未到達下一次允許生成的時間，直接中斷執行
        if (Time.time < nextSpawnTime)
        {
            return;
        }

        if (prefabToSpawn == null || spawnPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 的 ObjectSpawner 缺少 Prefab 或 SpawnPoint 設定！");
            return;
        }

        // 更新下一次可生成的時間
        nextSpawnTime = Time.time + cooldownTime;

        // --- 1. 計算散射旋轉與生成 ---
        Quaternion spawnRotation = spawnPoint.rotation;
        if (maxScatterAngle > 0f)
        {
            float randomAngle = Random.Range(-maxScatterAngle, maxScatterAngle);
            spawnRotation *= Quaternion.Euler(0f, 0f, randomAngle);
        }

        GameObject spawnedObj = Instantiate(prefabToSpawn, spawnPoint.position, spawnRotation);

        // --- 2. 處理初始速度 (針對子彈) ---
        if (spawnSpeed > 0f)
        {
            Rigidbody2D rb = spawnedObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 改用 spawnedObj 自身的 right，才能朝著散射後的方向飛行
                rb.linearVelocity = spawnedObj.transform.right * spawnSpeed;
            }
            else
            {
                Debug.LogWarning("你想給予生成物速度，但該 Prefab 上沒有 Rigidbody2D！");
            }
        }

        // --- 3. 處理自動銷毀 (針對刀光、粒子) ---
        if (autoDestroyTime > 0f)
        {
            Destroy(spawnedObj, autoDestroyTime);
        }

        // --- 4. 處理後座力 (針對玩家/施放者) ---
        if (recoilForce > 0f && wielderRb != null)
        {
            // 取得生成點的正後方方向
            Vector2 recoilDirection = -spawnPoint.right;

            // 施加瞬間推力 (Impulse)
            wielderRb.AddForce(recoilDirection * recoilForce, ForceMode2D.Impulse);
        }
    }
}