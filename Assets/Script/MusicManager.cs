using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField] private AudioClip[] tracks;
    [SerializeField] private int startingTrack = 0;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;
    [SerializeField] [Range(0f, 1f)] private float maxVolume = 0.5f;

    private AudioSource audioSource;
    private int currentTrack = -1;
    private float currentSliderValue = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
    }

    private static float sfxVolume = 1f;

    public static float SFXVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.Save();
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = loop;

        currentSliderValue = PlayerPrefs.GetFloat("MusicVolume", 1f);
        audioSource.volume = MapSliderToVolume(currentSliderValue);

        if (playOnStart && tracks != null && tracks.Length > 0)
            PlayTrack(startingTrack);
    }

    private float MapSliderToVolume(float sliderValue)
    {
        return Mathf.Clamp01(sliderValue) * Mathf.Clamp01(sliderValue) * maxVolume;
    }

    public void SetVolume(float sliderValue)
    {
        currentSliderValue = sliderValue;
        if (audioSource != null)
            audioSource.volume = MapSliderToVolume(sliderValue);
        PlayerPrefs.SetFloat("MusicVolume", sliderValue);
        PlayerPrefs.Save();
    }

    public void PlayTrack(int index)
    {
        if (tracks == null || index < 0 || index >= tracks.Length) return;
        if (tracks[index] == null) return;

        currentTrack = index;
        audioSource.clip = tracks[index];
        audioSource.Play();
    }

    public void NextTrack()
    {
        if (tracks == null || tracks.Length == 0) return;
        int next = (currentTrack + 1) % tracks.Length;
        PlayTrack(next);
    }

    public void PreviousTrack()
    {
        if (tracks == null || tracks.Length == 0) return;
        int prev = (currentTrack - 1 + tracks.Length) % tracks.Length;
        PlayTrack(prev);
    }

    public void Stop()
    {
        audioSource.Stop();
    }

    public void Pause()
    {
        audioSource.Pause();
    }

    public void Resume()
    {
        audioSource.UnPause();
    }

    public int CurrentTrackIndex => currentTrack;
    public string CurrentTrackName => tracks != null && currentTrack >= 0 && currentTrack < tracks.Length ? tracks[currentTrack].name : "";
}
