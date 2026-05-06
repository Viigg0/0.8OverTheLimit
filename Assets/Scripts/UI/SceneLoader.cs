using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Loads a scene by name and resumes time if paused
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;

        if (sceneName == "BarScene" && GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ResetBAC();
        }
        
        SceneManager.LoadScene(sceneName);
    }

    // Quits the application
    public void QuitGame()
    {
        Application.Quit();
    }
}
