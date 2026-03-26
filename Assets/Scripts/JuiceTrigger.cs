using UnityEngine;

public class JuiceTrigger : MonoBehaviour
{
    /// <summary>
    /// 呼叫全域的頓幀 (HitStop)
    /// </summary>
    public void CallHitStop()
    {
        if (HitStop.Instance != null)
        {
            HitStop.Instance.TriggerStop();
            Debug.Log("頓幀觸發");
        }
        else
        {
            Debug.LogWarning("場景中找不到 HitStop 管理器！");
        }
    }

    /// <summary>
    /// 呼叫全域的攝影機震動 (CameraImpulse)
    /// </summary>
    public void CallCameraShake()
    {
        if (CameraImpulse.Instance != null)
        {
            CameraImpulse.Instance.PlayDefaultShake();
        }
        else
        {
            Debug.LogWarning("場景中找不到 CameraImpulse 管理器！");
        }
    }
}