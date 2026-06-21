using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    public static bool IsPaused { get; private set; }

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        AutoFindUI();

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (optionPanel != null)
            optionPanel.SetActive(false);

        AutoWire();
    }

    private void AutoFindUI()
    {
        if (pausePanel == null)
        {
            GameObject pp = GameObject.Find("PausePanel");
            if (pp != null && pp.scene.name != null)
                pausePanel = pp;
        }

        if (optionPanel == null)
        {
            GameObject op = GameObject.Find("OptionPanel");
            if (op != null && op.scene.name != null)
                optionPanel = op;
        }
    }

    private void AutoWire()
    {
        if (pausePanel != null)
        {
            WireButton(pausePanel.transform, "ContinueBtn", ResumeGame);
            WireButton(pausePanel.transform, "Option", OpenOptions);
            WireButton(pausePanel.transform, "Save and Quit", SaveAndQuit);
        }

        if (optionPanel != null)
        {
            WireButton(optionPanel.transform, "BackButton", CloseOptions);
            WireVolumeSlider(optionPanel.transform);
        }
    }

    private void WireButton(Transform root, string name, UnityEngine.Events.UnityAction action)
    {
        Transform t = FindDeepChild(root, name);
        if (t != null)
        {
            Button btn = t.GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(action);
        }
    }

    private void WireVolumeSlider(Transform root)
    {
        Transform t = FindDeepChild(root, "Slider master volume");
        if (t != null)
        {
            Slider sl = t.GetComponent<Slider>();
            if (sl != null)
            {
                sl.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
                sl.onValueChanged.AddListener(OnMasterVolumeChanged);
            }
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        if (MusicManager.Instance != null)
            MusicManager.Instance.SetVolume(value);
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        if (IsPaused) ResumeGame();
        else PauseGame();
    }

    private void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pausePanel != null)
            pausePanel.SetActive(true);

        if (optionPanel != null)
            optionPanel.SetActive(false);

        InventoryManager inv = FindFirstObjectByType<InventoryManager>();
        if (inv != null)
            inv.CloseInventory();
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (optionPanel != null)
            optionPanel.SetActive(false);
    }

    public void OpenOptions()
    {
        if (optionPanel != null)
            optionPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionPanel != null)
            optionPanel.SetActive(false);
    }

    public void SaveAndQuit()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }
}
