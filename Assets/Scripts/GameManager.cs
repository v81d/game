using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private string sceneName = "NextScene";

    public void SwitchScene()
    {
        if (ScreenFader.Instance != null)
            ScreenFader.Instance.FadeToScene(sceneName);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();

        /* Most testing is done in the Unity editor, so we should probably keep this statement.
         * This block simply adds support for quitting the game if it was started in the Unity editor.
         */
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
    }
}
