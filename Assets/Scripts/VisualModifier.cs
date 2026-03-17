using System.Collections;
using UnityEngine;

public class VisualModifier : MonoBehaviour
{
    [Header("目標設定")]
    [Tooltip("如果不填，程式會自動抓取同物件上的 SpriteRenderer")]
    public SpriteRenderer targetSprite;

    [Header("受擊閃爍 (Color Flash)")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.15f;

    [Header("擠壓與伸展 (Squash & Stretch)")]
    public Vector2 squashScale = new Vector2(1.3f, 0.7f); // 變扁的比例
    public float squashDuration = 0.2f;

    // 內部狀態記憶（極度重要）
    private Color originalColor;
    private Vector3 originalScale;

    private Coroutine currentFlashCoroutine;
    private Coroutine currentSquashCoroutine;

    void Awake()
    {
        // 自動抓取與初始化狀態
        if (targetSprite == null) targetSprite = GetComponentInChildren<SpriteRenderer>();

        if (targetSprite != null)
        {
            originalColor = targetSprite.color;
        }
        originalScale = transform.localScale;
    }

    /// <summary>
    /// 公開方法：播放閃爍特效 (接在扣血觸發器或 ResourceManager 的 OnValueChanged 上)
    /// </summary>
    public void PlayFlash()
    {
        if (targetSprite == null) return;

        // 核心防護：如果有正在執行的閃爍，先停止它，並把顏色「強制重置」回原樣
        if (currentFlashCoroutine != null)
        {
            StopCoroutine(currentFlashCoroutine);
            targetSprite.color = originalColor;
        }

        currentFlashCoroutine = StartCoroutine(FlashRoutine());
    }

    /// <summary>
    /// 公開方法：播放擠壓特效 (接在跳躍或落地的 UnityEvent 上)
    /// </summary>
    public void PlaySquash()
    {
        // 核心防護：如果有正在執行的形變，先停止它，並把大小「強制重置」回原樣
        if (currentSquashCoroutine != null)
        {
            StopCoroutine(currentSquashCoroutine);
            transform.localScale = originalScale;
        }

        currentSquashCoroutine = StartCoroutine(SquashRoutine());
    }

    // --- 以下為協程實作細節 ---

    private IEnumerator FlashRoutine()
    {
        targetSprite.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        targetSprite.color = originalColor;

        currentFlashCoroutine = null; // 執行完畢清空紀錄
    }

    private IEnumerator SquashRoutine()
    {
        float elapsed = 0f;

        // 為了讓數學簡單直觀，我們用 Mathf.Sin 畫一個漂亮的半圓弧線來做形變過渡
        // 進階解法是開放 AnimationCurve 讓學生自己拉曲線，但初期用 Sin 函數最穩
        while (elapsed < squashDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / squashDuration;

            // 利用 Sin(percent * PI) 產生 0 -> 1 -> 0 的平滑數值
            float curve = Mathf.Sin(percent * Mathf.PI);

            // 根據曲線計算當前的 X 和 Y 縮放
            float currentX = Mathf.Lerp(originalScale.x, originalScale.x * squashScale.x, curve);
            float currentY = Mathf.Lerp(originalScale.y, originalScale.y * squashScale.y, curve);

            transform.localScale = new Vector3(currentX, currentY, originalScale.z);
            yield return null;
        }

        // 確保最後精準回到原始大小
        transform.localScale = originalScale;
        currentSquashCoroutine = null;
    }
}