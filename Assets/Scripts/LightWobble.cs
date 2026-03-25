// ==========================================
// 檔案名稱：LightWobble.cs (支援動態變色版)
// ==========================================
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Light2D))]
public class LightWobble : MonoBehaviour
{
    [Header("亮度閃爍設定")]
    [Tooltip("光線最暗的強度")]
    public float minIntensity = 0.8f;
    [Tooltip("光線最亮的強度")]
    public float maxIntensity = 1.2f;
    [Tooltip("閃爍的速度 (越大閃越快)")]
    public float flickerSpeed = 5f;

    [Header("顏色變化設定")]
    [Tooltip("是否啟用顏色動態變化？")]
    public bool enableColorChange = false;

    [Tooltip("請點擊右側色帶設定顏色順序 (0% 到 100% 的變化)")]
    public Gradient colorGradient;

    [Tooltip("顏色循環的速度 (越大變色越快)")]
    public float colorSpeed = 1f;

    private Light2D targetLight;
    private float randomOffset;
    private float colorTimer = 0f;

    void Awake()
    {
        targetLight = GetComponent<Light2D>();
        // 給每個光一個隨機起點，避免同場景的火把同步閃爍
        randomOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (targetLight == null) return;

        // 1. 處理亮度閃爍 (維持使用 PerlinNoise 產生平滑亂數)
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, randomOffset);
        targetLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

        // 2. 處理顏色循環
        if (enableColorChange)
        {
            // 累加計時器
            colorTimer += Time.deltaTime * colorSpeed;

            // 使用 Mathf.Repeat 讓數值永遠在 0 到 1 之間無窮迴圈
            float colorProgress = Mathf.Repeat(colorTimer, 1f);

            // 根據進度比例 (0~1) 從 Gradient 取出對應的顏色，並覆寫給光源
            targetLight.color = colorGradient.Evaluate(colorProgress);
        }
    }
}