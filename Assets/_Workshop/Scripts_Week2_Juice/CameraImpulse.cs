using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==========================================
// 👇 定義震動設定檔的資料結構
// ==========================================
[System.Serializable]
public class ShakeProfile
{
    [Tooltip("用來呼叫的名稱 (例如：LightHit, Explosion, RecoilUp)")]
    public string profileName;

    [Tooltip("震動持續時間 (秒)")]
    public float duration = 0.2f;

    [Tooltip("震動強度")]
    public float magnitude = 0.3f;

    [Tooltip("震動方向。如果設為 (0,0)，將視為全向隨機震動")]
    public Vector2 direction = Vector2.zero;
}

public class CameraImpulse : MonoBehaviour
{
    public static CameraImpulse Instance { get; private set; }

    [Header("📖 震動設定檔列表 (企劃/學員可自由新增)")]
    public List<ShakeProfile> shakeProfiles = new List<ShakeProfile>();

    private Vector3 originalPos;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        originalPos = transform.localPosition;
    }

    // ==========================================
    // 👇 給 UnityEvent 呼叫的萬用接口
    // ==========================================

    /// <summary>
    /// 透過設定檔名稱來播放對應的震動
    /// </summary>
    public void PlayShakeProfile(string profileName)
    {
        ShakeProfile targetProfile = null;

        // 在列表中尋找符合名稱的設定檔
        foreach (var profile in shakeProfiles)
        {
            if (profile.profileName == profileName)
            {
                targetProfile = profile;
                break;
            }
        }

        // 如果找到了設定檔
        if (targetProfile != null)
        {
            // 判斷方向是否為 (0,0)，是的話就呼叫全向震動，否則呼叫方向震動
            if (targetProfile.direction == Vector2.zero)
            {
                PlayShake(targetProfile.duration, targetProfile.magnitude);
            }
            else
            {
                PlayDirectionalShake(targetProfile.direction, targetProfile.duration, targetProfile.magnitude);
            }
        }
        else
        {
            Debug.LogWarning($"[CameraImpulse] 找不到名為 '{profileName}' 的震動設定檔！請檢查 Inspector 是否有拼錯字。");
        }
    }

    // ==========================================
    // 👇 核心邏輯 (給程式碼直接 Call 或是內部呼叫的 API)
    // ==========================================

    public void PlayShake(float duration, float magnitude)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            transform.localPosition = originalPos;
        }
        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    public void PlayDirectionalShake(Vector2 direction, float duration, float magnitude)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            transform.localPosition = originalPos;
        }
        shakeCoroutine = StartCoroutine(DirectionalShakeRoutine(direction, duration, magnitude));
    }

    // ==========================================
    // 👇 協程處理區
    // ==========================================

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeCoroutine = null;
    }

    private IEnumerator DirectionalShakeRoutine(Vector2 direction, float duration, float magnitude)
    {
        float elapsed = 0f;
        Vector3 dir = new Vector3(direction.x, direction.y, 0f).normalized;

        while (elapsed < duration)
        {
            float percentComplete = elapsed / duration;
            float damper = 1.0f - Mathf.Clamp01(percentComplete);

            float randomAmount = Random.Range(-1f, 1f) * magnitude * damper;

            transform.localPosition = originalPos + (dir * randomAmount);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeCoroutine = null;
    }
}