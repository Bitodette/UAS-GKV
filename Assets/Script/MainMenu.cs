using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private Slider masterVolumeSlider;
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
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetVolume(value);
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
        Application.Quit();
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
