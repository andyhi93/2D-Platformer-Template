using System.Collections;
using UnityEngine;

public class CameraImpulse : MonoBehaviour
{
    // 利用單例模式 (Singleton) 讓畫面上的任何觸發器都能輕易找到相機
    public static CameraImpulse Instance { get; private set; }

    private Vector3 originalPos;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }

        originalPos = transform.localPosition;
    }

    /// <summary>
    /// 公開方法：給 UnityEvent 呼叫。填入震動強度與時間。
    /// </summary>
    public void PlayShake(float duration = 0.2f, float magnitude = 0.3f)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            transform.localPosition = originalPos; // 防堆疊卡死
        }
        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    // 方便 Inspector 直接呼叫的無參數版本 (給 UnityEvent 點選用)
    public void PlayDefaultShake()
    {
        PlayShake(0.2f, 0.3f);
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 產生隨機的位移量
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeCoroutine = null;
    }
}