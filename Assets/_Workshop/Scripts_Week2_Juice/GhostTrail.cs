using System.Collections;
using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    [Header("目標設定")]
    [Tooltip("要產生殘影的目標圖片 (如果不填，自動抓取身上的 SpriteRenderer)")]
    public SpriteRenderer targetSprite;

    [Header("殘影設定")]
    [Tooltip("殘影的生成間隔 (秒)。越小越密集")]
    public float spawnInterval = 0.05f;

    [Tooltip("每個殘影存活的時間 (秒)。時間到會完全透明並銷毀")]
    public float ghostLifetime = 0.5f;

    [Tooltip("殘影的初始顏色與透明度")]
    public Color ghostColor = new Color(1f, 1f, 1f, 0.5f);

    [Tooltip("可選：替換殘影的材質 (例如套用 GlowMaterial 變成純色發光殘影)")]
    public Material overrideMaterial;

    private bool isTrailing = false;
    private float timer = 0f;

    void Awake()
    {
        if (targetSprite == null) targetSprite = GetComponent<SpriteRenderer>();
        if (targetSprite == null) targetSprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // 只有在開啟狀態時才會計時並生成殘影
        if (!isTrailing) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnGhost();
            timer = 0f;
        }
    }

    // --- 給 UnityEvent 呼叫的公開開關 ---
    public void StartTrail() { isTrailing = true; timer = spawnInterval; /* 確保一開啟立刻生成一個 */ }
    public void StopTrail() { isTrailing = false; }

    // --- 核心生成邏輯 ---
    private void SpawnGhost()
    {
        if (targetSprite == null || targetSprite.sprite == null) return;

        // 1. 建立一個空的 GameObject 作為殘影
        GameObject ghostObj = new GameObject("Ghost");
        ghostObj.transform.position = targetSprite.transform.position;
        ghostObj.transform.rotation = targetSprite.transform.rotation;

        // 使用 lossyScale 確保殘影大小與受擠壓變形後的角色完全一致
        ghostObj.transform.localScale = targetSprite.transform.lossyScale;

        // 2. 幫它加上 SpriteRenderer 並複製目標的狀態
        SpriteRenderer sr = ghostObj.AddComponent<SpriteRenderer>();
        sr.sprite = targetSprite.sprite;
        sr.color = ghostColor;
        sr.flipX = targetSprite.flipX;
        sr.flipY = targetSprite.flipY;

        // 確保殘影渲染在角色後方 (Sorting Order 減 1)
        sr.sortingLayerID = targetSprite.sortingLayerID;
        sr.sortingOrder = targetSprite.sortingOrder - 1;

        if (overrideMaterial != null)
        {
            sr.material = overrideMaterial;
        }
        else
        {
            sr.material = targetSprite.material;
        }

        // 3. 呼叫協程讓它自動淡出並銷毀
        StartCoroutine(FadeAndDestroy(sr, ghostLifetime));
    }

    private IEnumerator FadeAndDestroy(SpriteRenderer sr, float lifetime)
    {
        float elapsed = 0f;
        Color startColor = sr.color;

        while (elapsed < lifetime)
        {
            elapsed += Time.deltaTime;
            // 根據時間比例 (0~1) 讓 Alpha 值從初始值降到 0
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / lifetime);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            yield return null;
        }

        Destroy(sr.gameObject);
    }
}