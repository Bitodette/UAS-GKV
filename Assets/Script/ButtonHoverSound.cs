using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonHoverSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    public AudioClip hoverClip;
    public AudioClip clickClip;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.enabled = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverClip == null || audioSource == null) return;
        if (MusicManager.SFXVolume <= 0.0001f)
        {
            audioSource.enabled = false;
            audioSource.mute = true;
            return;
        }
        audioSource.enabled = true;
        audioSource.mute = false;
        audioSource.volume = MusicManager.SFXVolume;
        audioSource.PlayOneShot(hoverClip);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickClip == null || audioSource == null) return;
        if (MusicManager.SFXVolume <= 0.0001f)
        {
            audioSource.enabled = false;
            audioSource.mute = true;
            return;
        }
        audioSource.enabled = true;
        audioSource.mute = false;
        audioSource.volume = MusicManager.SFXVolume;
        audioSource.PlayOneShot(clickClip);
    }
}
