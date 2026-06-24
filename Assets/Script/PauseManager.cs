using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;
    public static bool IsPaused { get; private set; }     // static, dicek script lain buat blokir input

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject optionPanel;
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);                            // cegah duplicate
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        AutoFindUI();

        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionPanel != null) optionPanel.SetActive(false);

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
            WireSFXVolumeSlider(optionPanel.transform);
        }
    }

    private void WireButton(Transform root, string name, UnityEngine.Events.UnityAction action)
    {
        Transform t = FindDeepChild(root, name);
        if (t != null)
        {
            Button btn = t.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(action);
                AddHoverSound(btn.gameObject);
            }
        }
    }

    private void AddHoverSound(GameObject go)
    {
        if (hoverClip == null) hoverClip = Resources.Load<AudioClip>("Audio/hover");
        if (clickClip == null) clickClip = Resources.Load<AudioClip>("Audio/click");
        var hs = go.GetComponent<ButtonHoverSound>();
        if (hs == null) hs = go.AddComponent<ButtonHoverSound>();
        if (hoverClip != null) hs.hoverClip = hoverClip;
        if (clickClip != null) hs.clickClip = clickClip;
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

    private void WireSFXVolumeSlider(Transform root)
    {
        Transform t = FindDeepChild(root, "Slider SFX");
        if (t != null)
        {
            Slider sl = t.GetComponent<Slider>();
            if (sl != null)
            {
                sl.value = PlayerPrefs.GetFloat("SFXVolume", 1f);
                sl.onValueChanged.AddListener(OnSFXVolumeChanged);
            }
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        MusicManager.SFXVolume = value;
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
        Time.timeScale = 0f;                               // freeze game

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (pausePanel != null) pausePanel.SetActive(true);
        if (optionPanel != null) optionPanel.SetActive(false);

        InventoryManager inv = FindFirstObjectByType<InventoryManager>();
        if (inv != null) inv.CloseInventory();
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionPanel != null) optionPanel.SetActive(false);
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
        SaveManager.Save();                                  // simpan dulu
        IsPaused = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);                           // kembali ke main menu
    }
}
