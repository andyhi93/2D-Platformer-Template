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

    /// <summary>
    /// 公開方法：給 UnityEvent 呼叫，執行生成邏輯
    /// </summary>
    public void Spawn()
    {
        if (prefabToSpawn == null || spawnPoint == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 的 ObjectSpawner 缺少 Prefab 或 SpawnPoint 設定！");
            return;
        }

        // 1. 生成物件，並對齊生成點的位置與旋轉角度
        GameObject spawnedObj = Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation);

        // 2. 處理初始速度 (針對子彈)
        if (spawnSpeed > 0f)
        {
            Rigidbody2D rb = spawnedObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 因為我們用 Transform 旋轉來控制面向，所以 spawnPoint.right 永遠是「物件正前方」
                rb.linearVelocity = spawnPoint.right * spawnSpeed;
            }
            else
            {
                Debug.LogWarning("你想給予生成物速度，但該 Prefab 上沒有 Rigidbody2D！");
            }
        }

        // 3. 處理自動銷毀 (針對刀光、粒子)
        if (autoDestroyTime > 0f)
        {
            Destroy(spawnedObj, autoDestroyTime);
        }
    }
}