using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    // This public function can be called by the button
    public void QuitGame()
    {
        Debug.Log("Quit game requested"); // Just for debugging

        // This will only work in a built application
        Application.Quit();

        // This will close the editor play mode (comment out for final build)
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #endif
    }

    // Get the next scene to load from the inspector
    [Header("Scene to Load")]
    [SerializeField] private string sceneName = "NextScene";

    // This function will load the scene provided in the inspector with a fade transition
    public void SwitchScene()
    {
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeToScene(sceneName);
        }
        else
        {
            // Fallback if no ScreenFader exists in the scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}
