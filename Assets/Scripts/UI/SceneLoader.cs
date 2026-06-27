using UnityEngine;
using UnityEngine.SceneManagement;

namespace SurvivalMoba.UI
{
    /// <summary>
    /// Thin wrapper over scene loading for menu and end-screen buttons. Hook the
    /// public methods to Button OnClick events in the inspector.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private string gameSceneName = "GameScene";
        [SerializeField] private string menuSceneName = "MainMenu";

        public void PlayGame()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(gameSceneName);
        }

        public void RestartMatch()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void GoToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(menuSceneName);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
