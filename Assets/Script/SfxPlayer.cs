using UnityEngine;

[System.Serializable]
public class SfxEntry
{
    public string id;
    public AudioClip clip;
    [Tooltip("Delay in seconds before the sound plays (to sync with animation)")]
    public float delay;
}

public class SfxPlayer : MonoBehaviour
{
    [SerializeField] private SfxEntry[] entries;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void Play()
    {
        if (entries == null || entries.Length == 0) return;

        SfxEntry entry = entries[Random.Range(0, entries.Length)];
        PlayEntry(entry);
    }

    public void Play(string id)
    {
        if (entries == null || entries.Length == 0) return;

        int count = 0;
        for (int i = 0; i < entries.Length; i++)
            if (entries[i].id == id) count++;

        if (count == 0) return;

        int pick = Random.Range(0, count);
        for (int i = 0; i < entries.Length; i++)
        {
            if (entries[i].id == id)
            {
                if (pick == 0)
                {
                    PlayEntry(entries[i]);
                    return;
                }
                pick--;
            }
        }
    }

    private void PlayEntry(SfxEntry entry)
    {
        if (entry.clip == null) return;

        if (entry.delay > 0f)
            StartCoroutine(PlayDelayed(entry));
        else
            audioSource.PlayOneShot(entry.clip);
    }

    private System.Collections.IEnumerator PlayDelayed(SfxEntry entry)
    {
        yield return new WaitForSeconds(entry.delay);
        audioSource.PlayOneShot(entry.clip);
    }
}
