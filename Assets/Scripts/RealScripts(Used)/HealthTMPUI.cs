using UnityEngine;
using TMPro;

public class HealthTMPUI : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [Tooltip("Optional: a TMP text element positioned near the health to show -damage or +heals.")]
    [SerializeField] private TMP_Text changeText;
    [SerializeField] private string prefix = "Health: ";

    [Tooltip("Seconds to keep the change text visible after a health event.")]
    [Min(0f)]
    [SerializeField] private float changeDisplaySeconds = 0.9f;

    [Header("Debug")]
    [SerializeField] private bool logChangesToConsole = false;

    private int lastHealth = int.MinValue;
    private float changeHideTime;

    private void Awake()
    {
        if (healthText == null)
        {
            healthText = GetComponent<TMP_Text>();
        }

        if (healthText != null && changeText != null && ReferenceEquals(healthText, changeText))
        {
            Debug.LogWarning("[HealthTMPUI] 'Health Text' and 'Change Text' reference the same TMP object. " +
                             "This will prevent the +/- change line from showing. " +
                             "Fix: put HealthTMPUI on the main health text and assign the separate change TMP text to 'Change Text'.",
                             this);
        }

        if (changeText != null)
        {
            changeText.text = string.Empty;
        }
    }

    private void OnEnable()
    {
        PlayerHealth.HealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        PlayerHealth.HealthChanged -= OnHealthChanged;
    }

    private void Update()
    {
        int health = PlayerHealth.GetHealth();
        if (health != lastHealth)
        {
            lastHealth = health;
            if (healthText != null)
            {
                healthText.text = prefix + health;
            }
        }

        if (changeText != null && changeDisplaySeconds > 0f && Time.unscaledTime >= changeHideTime)
        {
            changeText.text = string.Empty;
        }
    }

    private void OnHealthChanged(PlayerHealth.HealthChange change)
    {
        if (changeText == null) return;

        if (change.delta < 0)
        {
            changeText.text = change.delta.ToString();
        }
        else if (change.delta > 0)
        {
            changeText.text = "+" + change.delta;
        }
        else
        {
            // delta == 0: ignore
            return;
        }

        changeHideTime = Time.unscaledTime + Mathf.Max(0f, changeDisplaySeconds);

        if (logChangesToConsole)
        {
            Debug.Log($"[HealthTMPUI] Health: {change.oldHealth} -> {change.newHealth} (delta={change.delta})", this);
        }
    }
}
