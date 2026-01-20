using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager I { get; private set; }

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfx2DSource;

    private void Awake()
    {
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }
        I = this;

        // Über Szenen behalten
        DontDestroyOnLoad(gameObject);
    }

    // Musik starten/wechseln 
    public void PlayMusic(AudioClip clip, float volume = 1f, bool loop = true)
    {
        if (clip == null || musicSource == null) return;

        musicSource.clip = clip;
        musicSource.volume = volume;
        musicSource.loop = loop;

        if (!musicSource.isPlaying) musicSource.Play();
        else musicSource.Play(); // Clipwechsel sofort übernehmen
    }

    public void StopMusic()
    {
        if (musicSource == null) return;
        musicSource.Stop();
    }

    // 2D-SFX für UI, Pickup, etc.
    public void PlaySfx2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfx2DSource == null) return;
        sfx2DSource.PlayOneShot(clip, volume);
    }
}
