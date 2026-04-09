using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("The root object (Panel) to show/hide for the pause menu.")]
    [SerializeField] private GameObject root;

    private CanvasGroup rootCanvasGroup;

    [Header("UI Interaction")]
    [Tooltip("If true, forces any Animator components under the pause menu root to use UnscaledTime while paused (fixes Button Transition=Animation when Time.timeScale=0).")]
    [SerializeField] private bool forceUIAnimatorsUnscaledWhilePaused = true;

    private Animator[] rootAnimators;
    private UnityEngine.UI.Selectable[] rootSelectables;

    [Header("Behavior")]
    [Tooltip("If true, sets Time.timeScale=0 while paused.")]
    [SerializeField] private bool freezeTimeWhilePaused = true;

    [Tooltip("If true, unlocks and shows the cursor while paused.")]
    [SerializeField] private bool unlockCursorWhilePaused = true;

    [Tooltip("If true, locks and hides the cursor when unpaused.")]
    [SerializeField] private bool lockCursorOnResume = true;

    [Tooltip("If true, this pause menu will not open if the player is dead.")]
    [SerializeField] private bool disableWhenPlayerDead = true;

    [Tooltip("If true, hides the pause menu on Awake.")]
    [SerializeField] private bool hideOnAwake = true;

    [Tooltip("If true, fixes cases where the pause menu root is accidentally scaled to (0,0,0) in the scene (which makes it impossible to see/hover).")]
    [SerializeField] private bool forceRootScaleOneWhenVisible = true;

    [Tooltip("If true, logs when pause is triggered (for debugging).")]
    [SerializeField] private bool debugLogs = false;

    [Header("Scene Names")]
    [Tooltip("Scene name for the main menu.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public static bool IsPaused { get; private set; }

    private void Awake()
    {
        if (root == null)
        {
            root = gameObject;
        }

        rootCanvasGroup = root.GetComponent<CanvasGroup>();
        if (rootCanvasGroup == null)
        {
            // If root is this GameObject (common setup), disabling it would stop Update().
            // Use CanvasGroup to visually hide + block UI input while keeping scripts active.
            rootCanvasGroup = root.AddComponent<CanvasGroup>();
        }

        if (hideOnAwake)
        {
            SetRootVisible(false);
        }

        if (forceRootScaleOneWhenVisible && root != null)
        {
            root.transform.localScale = Vector3.one;
        }

        rootAnimators = root != null ? root.GetComponentsInChildren<Animator>(true) : null;

        // Ensure UI hover/press tint still animates when timeScale is frozen.
        rootSelectables = root != null ? root.GetComponentsInChildren<UnityEngine.UI.Selectable>(true) : null;
        EnsureUnscaledTintHelpers();

        IsPaused = false;

        if (freezeTimeWhilePaused)
        {
            Time.timeScale = 1f;
        }

        if (debugLogs)
        {
            Debug.Log($"[PauseMenuUI] Awake on '{gameObject.name}'. Root='{(root != null ? root.name : "null")}'.");
        }
    }

    private void EnsureUnscaledTintHelpers()
    {
        if (rootSelectables == null || rootSelectables.Length == 0) return;

        for (int i = 0; i < rootSelectables.Length; i++)
        {
            UnityEngine.UI.Selectable s = rootSelectables[i];
            if (s == null) continue;
            if (s.GetComponent<UnscaledSelectableTint>() != null) continue;
            s.gameObject.AddComponent<UnscaledSelectableTint>();
        }
    }

    private void SetRootVisible(bool visible)
    {
        if (root == null) return;

        if (visible && forceRootScaleOneWhenVisible)
        {
            root.transform.localScale = Vector3.one;
        }

        if (rootCanvasGroup != null)
        {
            rootCanvasGroup.alpha = visible ? 1f : 0f;
            rootCanvasGroup.interactable = visible;
            rootCanvasGroup.blocksRaycasts = visible;
        }
        else
        {
            // Fallback.
            root.SetActive(visible);
        }
    }

    private void Update()
    {
        PlayInputHandler input = PlayInputHandler.Instance;

        // Fallback: if the Pause InputAction isn't wired correctly, Escape should still work.
        bool escapePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool pauseTriggered = input != null && input.PauseTriggered;

        if (pauseTriggered || escapePressed)
        {
            if (input != null)
            {
                input.ResetPause();
            }

            if (debugLogs)
            {
                Debug.Log($"[PauseMenuUI] Toggle pause (PauseAction={pauseTriggered}, Escape={escapePressed}).");
            }

            if (disableWhenPlayerDead && PlayerHealth.GetHealth() <= 0)
            {
                return;
            }

            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;

        SetRootVisible(true);

        SetRootAnimatorsUseUnscaledTime(true);

        if (freezeTimeWhilePaused)
        {
            Time.timeScale = 0f;
        }

        if (unlockCursorWhilePaused)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;

        SetRootVisible(false);

        SetRootAnimatorsUseUnscaledTime(false);

        if (freezeTimeWhilePaused)
        {
            Time.timeScale = 1f;
        }

        if (lockCursorOnResume)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void SetRootAnimatorsUseUnscaledTime(bool useUnscaled)
    {
        if (!forceUIAnimatorsUnscaledWhilePaused) return;
        if (rootAnimators == null || rootAnimators.Length == 0) return;

        AnimatorUpdateMode mode = useUnscaled ? AnimatorUpdateMode.UnscaledTime : AnimatorUpdateMode.Normal;
        for (int i = 0; i < rootAnimators.Length; i++)
        {
            Animator a = rootAnimators[i];
            if (a == null) continue;
            a.updateMode = mode;
        }
    }

    public void LoadMainMenu()
    {
        Resume();
        if (!string.IsNullOrWhiteSpace(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    public void RestartLevel()
    {
        Resume();
        Scene active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.buildIndex);
    }

    public void QuitGame()
    {
        Resume();
        Application.Quit();
    }
}
