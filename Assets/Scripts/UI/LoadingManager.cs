using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    private const string TargetSceneName = "RoseGarden";

    public void Load()
    {
        SceneManager.LoadScene(TargetSceneName);
    }

    public void Load(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError($"{nameof(LoadingManager)} received an empty scene name.", this);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}