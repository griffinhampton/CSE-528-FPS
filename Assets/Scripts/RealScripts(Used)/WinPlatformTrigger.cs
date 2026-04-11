using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WinPlatformTrigger : MonoBehaviour
{
    [Header("Collision")]
    [SerializeField] private string playerTag = "Player";

    [Header("UI")]
    [Tooltip("TextMeshPro text to show the win message on. If null, tries TMP_Text on this GameObject.")]
    [SerializeField] private TMP_Text targetText;

    [TextArea]
    [SerializeField] private string winMessage = "You Win!";

    [Tooltip("Seconds to keep the message on screen before returning to menu.")]
    [Min(0f)]
    [SerializeField] private float displaySeconds = 30f;

    [Tooltip("If true, uses unscaled time (works when Time.timeScale = 0).")]
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Main Menu")]
    [Tooltip("If empty, tries common menu scene names (e.g. 'mainmenu 1', 'MainMenu').")]
    [SerializeField] private string mainMenuSceneName = "mainmenu 1";

    [Tooltip("If true, forces Time.timeScale to 1 and unlocks cursor when loading the menu.")]
    [SerializeField] private bool forceUnpauseOnMenuLoad = true;

    private bool handled;
    private Coroutine routine;
    private string previousText;
    private bool previousActive;

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;
        TryHandle(collision.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        TryHandle(other.gameObject);
    }

    private void TryHandle(GameObject other)
    {
        if (handled) return;
        if (other == null) return;

        if (!string.IsNullOrWhiteSpace(playerTag))
        {
            if (!other.CompareTag(playerTag)) return;
        }

        handled = true;
        routine = StartCoroutine(WinAndReturnToMenu());
    }

    private IEnumerator WinAndReturnToMenu()
    {
        if (targetText == null)
        {
            targetText = GetComponent<TMP_Text>();
        }

        if (targetText != null)
        {
            previousText = targetText.text;
            previousActive = targetText.gameObject.activeSelf;

            targetText.text = winMessage ?? string.Empty;
            if (!targetText.gameObject.activeSelf)
            {
                targetText.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogWarning("[WinPlatformTrigger] No TMP_Text assigned (and none found on this GameObject). Assign a TextMeshProUGUI/TMP_Text in the inspector.", this);
        }

        float wait = Mathf.Max(0f, displaySeconds);
        if (wait > 0f)
        {
            if (useUnscaledTime) yield return new WaitForSecondsRealtime(wait);
            else yield return new WaitForSeconds(wait);
        }

        string sceneToLoad = ResolveMenuSceneName();
        if (string.IsNullOrWhiteSpace(sceneToLoad))
        {
            Debug.LogWarning("[WinPlatformTrigger] Could not resolve a main menu scene name. Add your menu scene to Build Settings and/or set 'Main Menu Scene Name'.", this);
            yield break;
        }

        if (forceUnpauseOnMenuLoad)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        SceneManager.LoadScene(sceneToLoad);

        // Restore text state if this object persists across scenes.
        if (targetText != null)
        {
            targetText.text = previousText;
            targetText.gameObject.SetActive(previousActive);
        }
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
