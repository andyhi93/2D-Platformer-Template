using System.Collections;
using UnityEngine;

public class HitStop : MonoBehaviour
{
    // 全域單例
    public static HitStop Instance { get; private set; }

    [Header("預設頓幀設定")]
    public float defaultDuration = 0.05f;
    [Range(0f, 1f)]
    public float timeScaleDuringStop = 0f;

    private Coroutine currentHitStop;

    void Awake()
    {
        // 單例初始化
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void TriggerStop()
    {
        if (currentHitStop != null) StopCoroutine(currentHitStop);
        currentHitStop = StartCoroutine(HitStopRoutine(defaultDuration));
    }

    private IEnumerator HitStopRoutine(float duration)
    {
        Time.timeScale = timeScaleDuringStop;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        currentHitStop = null;
    }
}