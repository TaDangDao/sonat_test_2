using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<Sound> sounds;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ SoundManager tồn tại khi chuyển scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Tự động lấy hoặc tạo component AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    /// <summary>
    /// Phát một âm thanh dựa theo loại được chỉ định.
    /// </summary>
    /// <param name="type">Loại âm thanh cần phát.</param>
    public void PlaySound(SoundType type)
    {
        AudioClip clip = GetSoundClip(type);
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy âm thanh cho loại: " + type);
        }
    }

    private AudioClip GetSoundClip(SoundType type)
    {
        Sound sound = sounds.FirstOrDefault(s => s.type == type);
        return sound?.clip;
    }
}

[System.Serializable]
public class Sound
{
    public SoundType type;
    public AudioClip clip;
}

public enum SoundType
{
    BlockClick,
    BlockMove,
    BlockDestroy,
    Win,
    Lose,
}
