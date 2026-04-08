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

        // 計算散射旋轉
        Quaternion spawnRotation = spawnPoint.rotation;
        if (maxScatterAngle > 0f)
        {
            // 產生正負範圍內的隨機角度
            float randomAngle = Random.Range(-maxScatterAngle, maxScatterAngle);
            // 由於原腳本使用 Rigidbody2D 與 .right，推斷為 2D 環境，故旋轉 Z 軸
            spawnRotation *= Quaternion.Euler(0f, 0f, randomAngle);
        }

        // 1. 生成物件，並對齊生成點的位置與「計算後的」旋轉角度
        GameObject spawnedObj = Instantiate(prefabToSpawn, spawnPoint.position, spawnRotation);

        // 2. 處理初始速度 (針對子彈)
        if (spawnSpeed > 0f)
        {
            Rigidbody2D rb = spawnedObj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // 必須改用 spawnedObj 自身的 right，才能朝著散射後的方向飛行
                rb.linearVelocity = spawnedObj.transform.right * spawnSpeed;
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