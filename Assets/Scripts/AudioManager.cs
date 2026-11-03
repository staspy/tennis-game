using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Звуки")]
    public AudioClip racketHitSound;
    public AudioClip groundBounceSound;

    private AudioSource audioSource;

    void Awake()
    {
        // Создаем синглтон (чтобы к нему можно было обратиться из любого скрипта)
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Музыка и звуки не прерываются при смене сцен
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.8f;
    }

    // Метод для воспроизведения звука удара ракеткой
    public void PlayRacketHit()
    {
        if (racketHitSound != null)
            audioSource.PlayOneShot(racketHitSound, 1.0f);
    }

    // Метод для воспроизведения звука отскока
    public void PlayGroundBounce()
    {
        if (groundBounceSound != null)
            audioSource.PlayOneShot(groundBounceSound, 0.6f); // Чуть тише
    }
}