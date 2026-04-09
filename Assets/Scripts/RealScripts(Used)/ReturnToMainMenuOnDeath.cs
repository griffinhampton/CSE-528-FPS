using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMainMenuOnDeath : MonoBehaviour
{
    [Tooltip("If empty, tries common menu scene names (e.g. 'mainmenu 1', 'MainMenu').")]
    [SerializeField] private string mainMenuSceneName = "mainmenu 1";

    [Tooltip("If true, resets score and health before loading the menu.")]
    [SerializeField] private bool resetScoreAndHealth = true;

    [Tooltip("If true, forces Time.timeScale to 1 and unlocks cursor when the menu scene loads.")]
    [SerializeField] private bool forceUnpauseOnMenuLoad = true;

    private bool handled;

    private void OnEnable()
    {
        PlayerHealth.Died += OnDied;
    }

    private void OnDisable()
    {
        PlayerHealth.Died -= OnDied;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnDied()
    {
        if (handled) return;
        handled = true;

        if (resetScoreAndHealth)
        {
            PlayerScore.ResetScore();
            PlayerHealth.ResetHealthToMax();
        }

        string sceneToLoad = ResolveMenuSceneName();
        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogWarning("[ReturnToMainMenuOnDeath] Could not resolve a main menu scene name. Add your menu scene to Build Settings and/or set 'Main Menu Scene Name'.", this);
            return;
        }

        if (forceUnpauseOnMenuLoad)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        // Make sure we don't get stuck paused from a death screen or pause menu.
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Always unpause after loading the menu to avoid cases where another listener
        // sets Time.timeScale=0 after we request the load.
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private string ResolveMenuSceneName()
    {
        if (!string.IsNullOrWhiteSpace(mainMenuSceneName) && Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            return mainMenuSceneName;
        }

        string[] candidates = new string[] { "mainmenu 1", "MainMenu", "mainmenu", "Menu" };
        for (int i = 0; i < candidates.Length; i++)
        {
            if (Application.CanStreamedLevelBeLoaded(candidates[i]))
            {
                return candidates[i];
            }
        }

        return null;
    }
}
