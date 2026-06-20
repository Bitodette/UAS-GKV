using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private Slider masterVolumeSlider;

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
}
