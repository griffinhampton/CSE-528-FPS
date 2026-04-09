using UnityEngine;
using TMPro;

public class ScoreAwardPopupTMPUI : MonoBehaviour
{
    [SerializeField] private TMP_Text awardText;

    [Tooltip("Seconds to keep the popup visible after a score award.")]
    [Min(0f)]
    [SerializeField] private float displaySeconds = 0.9f;

    [Tooltip("If true, shows '+<points> x<mult>' when mult != 1.")]
    [SerializeField] private bool showMultiplierSuffix = true;

    [Header("Debug")]
    [SerializeField] private bool logAwardsToConsole = false;

    private float hideTime;

    private void Awake()
    {
        if (awardText == null)
        {
            awardText = GetComponent<TMP_Text>();
        }

        if (awardText != null)
        {
            awardText.text = string.Empty;
        }
    }

    private void OnEnable()
    {
        PlayerScore.ScoreAwarded += OnScoreAwarded;
    }

    private void OnDisable()
    {
        PlayerScore.ScoreAwarded -= OnScoreAwarded;
    }

    private void Update()
    {
        if (awardText == null) return;
        if (displaySeconds <= 0f) return;

        if (Time.unscaledTime >= hideTime)
        {
            awardText.text = string.Empty;
        }
    }

    private void OnScoreAwarded(PlayerScore.ScoreAward award)
    {
        if (awardText == null) return;

        if (showMultiplierSuffix && (award.multiplier > 1.001f || award.multiplier < 0.999f))
        {
            awardText.text = "+" + award.finalPoints + " x" + award.multiplier.ToString("0.##");
        }
        else
        {
            awardText.text = "+" + award.finalPoints;
        }

        hideTime = Time.unscaledTime + Mathf.Max(0f, displaySeconds);

        if (logAwardsToConsole)
        {
            Debug.Log($"[ScoreAwardPopupTMPUI] Award: base={award.basePoints}, mult={award.multiplier:0.##}, final={award.finalPoints}, text='{awardText.text}'", this);
        }
    }
}
