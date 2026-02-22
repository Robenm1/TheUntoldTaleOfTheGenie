using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class BGMManager : MonoBehaviour
{
    [Header("Music Settings")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField][Range(0f, 1f)] private float volume = 0.5f;
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private float fadeInDuration = 2f;
    [SerializeField] private float fadeOutDuration = 1f;

    [Header("Persistence")]
    [SerializeField] private bool dontDestroyOnLoad = false;

    private AudioSource audioSource;
    private float targetVolume;
    private bool isFading = false;

    private static BGMManager instance;

    private void Awake()
    {
        if (dontDestroyOnLoad)
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        SetupAudioSource();

        if (playOnAwake && backgroundMusic != null)
        {
            PlayMusic();
        }
    }

    private void SetupAudioSource()
    {
        audioSource.clip = backgroundMusic;
        audioSource.loop = loop;
        audioSource.playOnAwake = false;
        targetVolume = volume;
        audioSource.volume = 0f;
    }

    public void PlayMusic()
    {
        if (audioSource == null || backgroundMusic == null) return;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        if (fadeInDuration > 0 && !isFading)
        {
            StartCoroutine(FadeIn());
        }
        else
        {
            audioSource.volume = volume;
            Debug.Log($"<color=cyan>BGM started: {backgroundMusic.name}</color>");
        }
    }

    public void PlayNewMusic(AudioClip newClip, bool crossfade = true)
    {
        if (newClip == null) return;

        if (crossfade && audioSource.isPlaying)
        {
            StartCoroutine(CrossfadeToNewTrack(newClip));
        }
        else
        {
            audioSource.clip = newClip;
            backgroundMusic = newClip;
            PlayMusic();
        }
    }

    private IEnumerator FadeIn()
    {
        isFading = true;
        float elapsed = 0f;
        float startVolume = audioSource.volume;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeInDuration);
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, progress);
            yield return null;
        }

        audioSource.volume = targetVolume;
        isFading = false;
        Debug.Log("<color=green>BGM fade-in complete!</color>");
    }

    private IEnumerator FadeOut()
    {
        isFading = true;
        float elapsed = 0f;
        float startVolume = audioSource.volume;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeOutDuration);
            audioSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
        isFading = false;
        Debug.Log("<color=yellow>BGM faded out</color>");
    }

    private IEnumerator CrossfadeToNewTrack(AudioClip newClip)
    {
        yield return StartCoroutine(FadeOut());

        audioSource.clip = newClip;
        backgroundMusic = newClip;

        yield return StartCoroutine(FadeIn());
    }

    public void StopMusic(bool fade = true)
    {
        if (audioSource == null) return;

        if (fade && fadeOutDuration > 0)
        {
            StartCoroutine(FadeOut());
        }
        else
        {
            audioSource.Stop();
            audioSource.volume = 0f;
            Debug.Log("<color=yellow>BGM stopped</color>");
        }
    }

    public void PauseMusic()
    {
        if (audioSource != null)
        {
            audioSource.Pause();
            Debug.Log("<color=yellow>BGM paused</color>");
        }
    }

    public void ResumeMusic()
    {
        if (audioSource != null)
        {
            audioSource.UnPause();
            Debug.Log("<color=cyan>BGM resumed</color>");
        }
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        targetVolume = volume;

        if (audioSource != null && !isFading)
        {
            audioSource.volume = volume;
        }
    }

    public bool IsPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
