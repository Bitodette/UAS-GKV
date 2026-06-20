using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // BARIS INI YANG KURANG. Tanpa ini, Unity gak tahu apa itu 'optionPanel'
    [SerializeField] private GameObject optionPanel; 

    // Fungsi pindah scene lu tetap aman di sini
    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenOption()
    {
        if (optionPanel != null)
        {
            optionPanel.SetActive(true); // Menyalakan overlay
        }
    }

    public void CloseOption()
    {
        if (optionPanel != null)
        {
            optionPanel.SetActive(false); // Mematikan overlay
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}