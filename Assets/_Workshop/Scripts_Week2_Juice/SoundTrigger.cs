using UnityEngine;

public class SoundTrigger : MonoBehaviour
{
    // 讓按鈕的 OnClick 來呼叫這個方法
    public void PlaySound(string soundName)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFXByName(soundName);
        }
        else
        {
            Debug.LogWarning("場景中沒有 AudioManager！");
        }
    }
}