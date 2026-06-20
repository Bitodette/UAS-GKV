using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField] private AudioClip[] tracks;
    [SerializeField] private int startingTrack = 0;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loop = true;
    [SerializeField] private float volume = 1f;

    private AudioSource audioSource;
    private int currentTrack = -1;

    [RuntimeInitializeOnLoadMethod]
    private static void AutoInitialize()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("MusicManager");
            go.AddComponent<MusicManager>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (tracks == null || tracks.Length == 0)
            tracks = Resources.LoadAll<AudioClip>("Musik");
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.loop = loop;

        volume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        audioSource.volume = volume;

        if (playOnStart && tracks != null && tracks.Length > 0)
            PlayTrack(startingTrack);
    }

    public void SetVolume(float v)
    {
        volume = v;
        if (audioSource != null)
            audioSource.volume = v;
        PlayerPrefs.SetFloat("MusicVolume", v);
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
