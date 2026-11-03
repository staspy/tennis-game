using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    public AudioClip musicClip; 
    private AudioSource audioSource;
    private bool isMuted = false; // Запоминаем, выключена ли музыка

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject); 

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.volume = 0.3f;
        audioSource.ignoreListenerPause = true; 
        audioSource.Play();

        audioSource.mute = isMuted;
    }

    // Метод для переключения музыки
    public void ToggleMusic()
    {
        isMuted = !isMuted;
        audioSource.mute = isMuted;
    }

    // Возвращает текущее состояние (для текста на кнопке)
    public bool IsMusicMuted()
    {
        return isMuted;
    }
}