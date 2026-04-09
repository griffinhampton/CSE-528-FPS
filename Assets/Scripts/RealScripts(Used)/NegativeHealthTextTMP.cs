using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class NegativeHealthTextTMP : MonoBehaviour
{
    [Tooltip("Text to display when health is negative. You may use {health} as a placeholder.")]
    [TextArea]
    [SerializeField] private string negativeHealthMessage = "";

    [Tooltip("If true, clears the text when health is 0 or higher. If false, restores the original text.")]
    [SerializeField] private bool clearWhenNotNegative = true;

    [Tooltip("If > 0, the negative-health message will hide after this many seconds (even if health stays negative).")]
    [Min(0f)]
    [SerializeField] private float hideAfterSeconds = 10f;

    private TMP_Text text;
    private string originalText;

    private bool wasNegative;
    private float hideAtUnscaledTime;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        originalText = text != null ? text.text : string.Empty;
    }

    private void OnEnable()
    {
        PlayerHealth.HealthChanged += OnHealthChanged;
        Refresh(PlayerHealth.GetHealth());
    }

    private void OnDisable()
    {
        PlayerHealth.HealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(PlayerHealth.HealthChange change)
    {
        Refresh(change.newHealth);
    }

    private void Refresh(int health)
    {
        if (text == null) return;

        if (health < 0)
        {
            if (!wasNegative)
            {
                wasNegative = true;
                if (hideAfterSeconds > 0f)
                {
                    hideAtUnscaledTime = Time.unscaledTime + hideAfterSeconds;
                }
                else
                {
                    hideAtUnscaledTime = 0f;
                }
            }

            if (hideAfterSeconds > 0f && Time.unscaledTime >= hideAtUnscaledTime)
            {
                text.text = string.Empty;
                return;
            }

            string msg = negativeHealthMessage ?? string.Empty;
            if (msg.Contains("{health}"))
            {
                msg = msg.Replace("{health}", health.ToString());
            }
            text.text = msg;
        }
        else
        {
            wasNegative = false;
            text.text = clearWhenNotNegative ? string.Empty : originalText;
        }
    }
}
