using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// MainMenu class handles the main menu interactions, including changing scenes and quitting the game.
/// </summary>
public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// Changes the current scene to the specified scene name.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Quits the application.
    /// </summary>
    public void QuitGame()
    {
        Application.Quit();
    }
}
