using System.Collections;
using UnityEngine;

public class VisualModifier : MonoBehaviour
{
    [Header("目標設定")]
    [Tooltip("如果不填，程式會自動抓取同物件或子物件上的 SpriteRenderer")]
    public SpriteRenderer targetSprite;

    [Header("受擊閃紅 (Color Flash)")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.15f;

    [Header("擠壓與伸展 (Squash & Stretch)")]
    public Vector2 squashScale = new Vector2(1.3f, 0.7f); // 變扁的比例
    public float squashDuration = 0.2f;

    [Header("無敵閃爍 (I-Frame Flicker)")]
    [Tooltip("閃爍的頻率 (秒數越小閃越快)")]
    public float flickerSpeed = 0.08f;

    // 內部狀態記憶（極度重要）
    private Color originalColor;
    private Vector3 originalScale;

    private Coroutine currentFlashCoroutine;
    private Coroutine currentSquashCoroutine;
    private Coroutine currentFlickerCoroutine; // 新增：記錄閃爍協程

    void Awake()
    {
        // 自動抓取與初始化狀態
        if (targetSprite == null) targetSprite = GetComponent<SpriteRenderer>();
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
        if (currentSquashCoroutine != null)
        {
            StopCoroutine(currentSquashCoroutine);
            transform.localScale = originalScale;
        }

        currentSquashCoroutine = StartCoroutine(SquashRoutine());
    }

    /// <summary>
    /// 新增！公開方法：播放無敵閃爍特效 (接在 ResourceManager 的 OnInvincibilityStarted 上)
    /// </summary>
    public void PlayFlicker(float duration)
    {
        if (targetSprite == null) return;

        // 核心防護：如果有正在執行的閃爍，先停止它，並確保圖片是被打開的
        if (currentFlickerCoroutine != null)
        {
            StopCoroutine(currentFlickerCoroutine);
            targetSprite.enabled = true;
        }

        currentFlickerCoroutine = StartCoroutine(FlickerRoutine(duration));
    }

    // --- 以下為協程實作細節 ---

    private IEnumerator FlashRoutine()
    {
        targetSprite.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        targetSprite.color = originalColor;

        currentFlashCoroutine = null;
    }

    private IEnumerator SquashRoutine()
    {
        float elapsed = 0f;

        while (elapsed < squashDuration)
        {
            elapsed += Time.deltaTime;
            float percent = elapsed / squashDuration;

            float curve = Mathf.Sin(percent * Mathf.PI);

            float currentX = Mathf.Lerp(originalScale.x, originalScale.x * squashScale.x, curve);
            float currentY = Mathf.Lerp(originalScale.y, originalScale.y * squashScale.y, curve);

            transform.localScale = new Vector3(currentX, currentY, originalScale.z);
            yield return null;
        }

        transform.localScale = originalScale;
        currentSquashCoroutine = null;
    }

    //無敵閃爍協程 ---
    private IEnumerator FlickerRoutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 反轉圖片的顯示狀態 (開變關、關變開)
            targetSprite.enabled = !targetSprite.enabled;

            yield return new WaitForSeconds(flickerSpeed);
            elapsed += flickerSpeed;
        }

        // 確保時間結束後，圖片一定是被打開的，不然角色會永遠隱形！
        targetSprite.enabled = true;
        currentFlickerCoroutine = null;
    }
}