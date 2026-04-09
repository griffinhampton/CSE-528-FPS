using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    [Tooltip("Scene name to load when Play is clicked.")]
    [SerializeField] private string gameSceneName = "Stage";

    [Tooltip("If true, cursor is unlocked and visible while in the menu.")]
    [SerializeField] private bool unlockCursorInMenu = true;

    private void Start()
    {
        Time.timeScale = 1f;

        if (unlockCursorInMenu)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void Play()
    {
        if (string.IsNullOrWhiteSpace(gameSceneName)) return;
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
