using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundData
{
    [Tooltip("音效的代號，例如：Jump, Hit, Coin, Explosion")]
    public string soundName;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音軌設定 (請掛載 AudioSource)")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("打擊感優化 (Game Feel)")]
    public bool randomizePitch = true;
    [Range(0f, 0.5f)]
    public float pitchRange = 0.1f;

    [Header("音效庫 (Sound Library)")]
    [Tooltip("在這裡集中管理所有的音效檔案")]
    public List<SoundData> sfxLibrary = new List<SoundData>();

    // 內部查詢字典 O(1)
    private Dictionary<string, AudioClip> sfxDict = new Dictionary<string, AudioClip>();
    private float originalPitch;

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }

        if (sfxSource != null)
        {
            originalPitch = sfxSource.pitch;
        }

        // 初始化字典
        foreach (var sound in sfxLibrary)
        {
            if (!sfxDict.ContainsKey(sound.soundName))
            {
                sfxDict.Add(sound.soundName, sound.clip);
            }
            else
            {
                Debug.LogWarning($"[AudioManager] 發現重複的音效名稱: {sound.soundName}！");
            }
        }
    }

    /// <summary>
    /// 保留原本的直接傳入 AudioClip 播放方式 (給喜歡直接拉線的人)
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;

        sfxSource.pitch = randomizePitch ? originalPitch + Random.Range(-pitchRange, pitchRange) : originalPitch;
        sfxSource.PlayOneShot(clip);
    }

    /// <summary>
    /// 新增！公開方法：透過「字串名稱」播放音效 (極度推薦使用)
    /// </summary>
    public void PlaySFXByName(string soundName)
    {
        if (sfxDict.TryGetValue(soundName, out AudioClip clip))
        {
            PlaySFX(clip);
            Debug.Log("PlayClip");
        }
        else
        {
            Debug.LogWarning($"[AudioManager] 找不到名為 '{soundName}' 的音效！");
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null) return;
        if (bgmSource.clip == clip) return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }
}