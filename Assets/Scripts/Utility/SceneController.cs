using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    //Dynamic Scene Naming capabilities
    [SerializeField] private string MainMenu = "";

    void Start()
    {
        
    }

    // Closes out of the application
    public void Quit()
    {
        Application.Quit();
    }

    public void GoToScene(String sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void GoToMainMenu()
    {
        SceneManager.LoadScene(MainMenu);
    }

}
