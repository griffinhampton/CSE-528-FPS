using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeathTextRotator : MonoBehaviour
{
    private enum TriggerMode
    {
        OnDied = 0,
        OnHealthNonPositiveWhileDeathDisallowed = 1,
    }

    private enum OutputMode
    {
        SpawnPrefabs = 0,
        SetTMPText = 1,
    }

    [Header("Trigger")]
    [SerializeField] private TriggerMode triggerMode = TriggerMode.OnDied;

    [Header("Output")]
    [SerializeField] private OutputMode outputMode = OutputMode.SpawnPrefabs;

    [Tooltip("When Output Mode is SetTMPText, this is the TMP text to overwrite. If null, tries to use TMP_Text on this GameObject.")]
    [SerializeField] private TMP_Text targetText;

    [Tooltip("When Output Mode is SetTMPText, randomly picks one of these phrases.")]
    [SerializeField] private List<string> phrases = new List<string>();

    [Header("Prefabs")]
    [Tooltip("UI prefabs (texts, panels, etc.) to spawn after the player dies (used when Output Mode is SpawnPrefabs).")]
    [SerializeField] private List<GameObject> textPrefabs = new List<GameObject>();

    [Tooltip("Optional parent to spawn under (usually a Canvas or a panel). If null, spawns under this object.")]
    [SerializeField] private Transform parent;

    [Header("Timing")]
    [Tooltip("Seconds after death before the first text spawns.")]
    [Min(0f)]
    [SerializeField] private float initialDelaySeconds = 20f;

    [Tooltip("Seconds between texts after the first one spawns.")]
    [Min(0.1f)]
    [SerializeField] private float intervalSeconds = 20f;

    [Tooltip("If true, uses unscaled time (works when Time.timeScale = 0).")]
    [SerializeField] private bool useUnscaledTime = true;

    [Header("Behavior")]
    [Tooltip("If true, replaces the previous spawned text instead of stacking many.")]
    [SerializeField] private bool replacePrevious = true;

    [Tooltip("If > 0, destroys the spawned text after this many seconds.")]
    [Min(0f)]
    [SerializeField] private float destroyAfterSeconds = 0f;

    private Coroutine loop;
    private GameObject currentInstance;
    private int lastIndex = -1;

    private void OnEnable()
    {
        if (triggerMode == TriggerMode.OnDied)
        {
            PlayerHealth.Died += OnPlayerDied;
        }
        else
        {
            PlayerHealth.HealthChanged += OnHealthChanged;
            // Catch the case where we enable this after the player is already at/below 0.
            TryStartEvilLoop(PlayerHealth.GetHealth());
        }
    }

    private void OnDisable()
    {
        PlayerHealth.Died -= OnPlayerDied;
        PlayerHealth.HealthChanged -= OnHealthChanged;

        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }
    }

    private void OnPlayerDied()
    {
        if (loop != null) return;
        loop = StartCoroutine(Loop());
    }

    private void OnHealthChanged(PlayerHealth.HealthChange change)
    {
        TryStartEvilLoop(change.newHealth);
    }

    private void TryStartEvilLoop(int health)
    {
        if (triggerMode != TriggerMode.OnHealthNonPositiveWhileDeathDisallowed) return;
        if (loop != null) return;

        // Only start in "evil mode" style behavior: death is disabled.
        if (PlayerHealth.IsDeathAllowed()) return;

        // "After the player dies" doesn't happen in evil mode; use health <= 0 as the equivalent moment.
        if (health > 0) return;

        loop = StartCoroutine(Loop());
    }

    private IEnumerator Loop()
    {
        if (outputMode == OutputMode.SetTMPText)
        {
            if (targetText == null)
            {
                targetText = GetComponent<TMP_Text>();
            }
        }

        if (parent == null) parent = transform;

        if (initialDelaySeconds > 0f)
        {
            if (useUnscaledTime) yield return new WaitForSecondsRealtime(initialDelaySeconds);
            else yield return new WaitForSeconds(initialDelaySeconds);
        }

        while (true)
        {
            TickOutput();

            float wait = Mathf.Max(0.1f, intervalSeconds);
            if (useUnscaledTime) yield return new WaitForSecondsRealtime(wait);
            else yield return new WaitForSeconds(wait);
        }
    }

    private void TickOutput()
    {
        if (outputMode == OutputMode.SetTMPText)
        {
            SetRandomPhrase();
        }
        else
        {
            SpawnRandomPrefab();
        }
    }

    private void SpawnRandomPrefab()
    {
        if (textPrefabs == null || textPrefabs.Count == 0) return;

        int idx;
        if (textPrefabs.Count == 1)
        {
            idx = 0;
        }
        else
        {
            idx = Random.Range(0, textPrefabs.Count);
            if (idx == lastIndex)
            {
                idx = (idx + 1) % textPrefabs.Count;
            }
        }

        lastIndex = idx;
        GameObject prefab = textPrefabs[idx];
        if (prefab == null) return;

        if (replacePrevious && currentInstance != null)
        {
            Destroy(currentInstance);
            currentInstance = null;
        }

        currentInstance = Instantiate(prefab, parent);

        if (destroyAfterSeconds > 0f)
        {
            Destroy(currentInstance, destroyAfterSeconds);
        }
    }

    private void SetRandomPhrase()
    {
        if (targetText == null) return;
        if (phrases == null || phrases.Count == 0) return;

        int idx;
        if (phrases.Count == 1)
        {
            idx = 0;
        }
        else
        {
            idx = Random.Range(0, phrases.Count);
            if (idx == lastIndex)
            {
                idx = (idx + 1) % phrases.Count;
            }
        }

        lastIndex = idx;

        string phrase = phrases[idx] ?? string.Empty;
        if (phrase.Contains("{health}"))
        {
            phrase = phrase.Replace("{health}", PlayerHealth.GetHealth().ToString());
        }

        targetText.text = phrase;
    }
}
