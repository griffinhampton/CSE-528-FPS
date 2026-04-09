using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathScreenUI : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("The root object (Panel) to show/hide for the death screen.")]
    [SerializeField] private GameObject root;

    [Header("Behavior")]
    [Tooltip("If true, sets Time.timeScale=0 when the player dies.")]
    [SerializeField] private bool freezeTimeOnDeath = true;

    [Tooltip("If true, unlocks and shows the cursor on death.")]
    [SerializeField] private bool unlockCursorOnDeath = true;

    [Tooltip("If true, hides the death screen on Awake.")]
    [SerializeField] private bool hideOnAwake = true;

    [Header("Scene Names")]
    [Tooltip("Scene name for the main menu.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool shown;

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }

        if (hideOnAwake)
        {
            root.SetActive(false);
        }

        shown = false;
    }

    private void OnEnable()
    {
        PlayerHealth.Died += OnPlayerDied;
    }

    private void OnDisable()
    {
        PlayerHealth.Died -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        if (shown) return;
        shown = true;

        if (root != null)
        {
            root.SetActive(true);
        }

        if (freezeTimeOnDeath)
        {
            Time.timeScale = 0f;
        }

        if (unlockCursorOnDeath)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void RestartLevel()
    {
        ResumeTimeIfNeeded();

        PlayerScore.ResetScore();
        PlayerHealth.ResetHealthToMax();

        Scene active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.buildIndex);
    }

    public void LoadMainMenu()
    {
        ResumeTimeIfNeeded();

        PlayerScore.ResetScore();
        PlayerHealth.ResetHealthToMax();

        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void QuitGame()
    {
        ResumeTimeIfNeeded();
        Application.Quit();
    }

    private void ResumeTimeIfNeeded()
    {
        if (freezeTimeOnDeath)
        {
            Time.timeScale = 1f;
        }
    }
}
