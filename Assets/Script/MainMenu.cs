using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    private void Awake()
    {
        if (hoverClip == null)
            hoverClip = Resources.Load<AudioClip>("Audio/hover");
        if (clickClip == null)
            clickClip = Resources.Load<AudioClip>("Audio/click");

        var allButtons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var btn in allButtons)
            AddButtonSound(btn.gameObject);
    }

    private void Start()
    {
        AutoWireSliders();
    }

    private void AutoWireSliders()
    {
        if (optionPanel == null) return;
        Transform panel = optionPanel.transform;

        // Master volume slider
        if (masterVolumeSlider == null)
        {
            Transform t = FindDeepChild(panel, "Slider master volume");
            if (t != null) masterVolumeSlider = t.GetComponent<Slider>();
        }
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        // SFX volume slider
        if (sfxVolumeSlider == null)
        {
            Transform t = FindDeepChild(panel, "Slider SFX");
            if (t != null) sfxVolumeSlider = t.GetComponent<Slider>();
        }
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            MusicManager.SFXVolume = sfxVolumeSlider.value;
        }
    }

    private static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        MusicManager.SFXVolume = value;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.N))
        {
            SaveManager.DeleteSave();
            Debug.Log("Save data deleted!");
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenOption()
    {
        if (optionPanel != null)
            optionPanel.SetActive(true);
    }

    public void CloseOption()
    {
        if (optionPanel != null)
            optionPanel.SetActive(false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void AddButtonSound(GameObject go)
    {
        var hs = go.GetComponent<ButtonHoverSound>();
        if (hs == null)
            hs = go.AddComponent<ButtonHoverSound>();
        if (hoverClip != null) hs.hoverClip = hoverClip;
        if (clickClip != null) hs.clickClip = clickClip;
    }
}
