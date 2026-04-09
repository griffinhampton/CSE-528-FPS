using UnityEngine;
using TMPro;

public class ScoreTMPUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [Tooltip("Optional: a TMP text element positioned above the score to show +points or *multiplier on each award.")]
    [SerializeField] private TMP_Text awardText;
    [SerializeField] private string prefix = "Score: ";

    [Tooltip("Seconds to keep the award text visible after a score event.")]
    [Min(0f)]
    [SerializeField] private float awardDisplaySeconds = 0.9f;

    [Tooltip("If true, the award popup shows the multiplier too (e.g., '+20 x2').")]
    [SerializeField] private bool showMultiplierSuffix = true;

    [Header("Debug")]
    [SerializeField] private bool logAwardsToConsole = false;

    private int lastScore = int.MinValue;
    private float awardHideTime;

    private void Awake()
    {
        if (scoreText == null)
        {
            scoreText = GetComponent<TMP_Text>();
        }

        if (scoreText != null && awardText != null && ReferenceEquals(scoreText, awardText))
        {
            Debug.LogWarning("[ScoreTMPUI] 'Score Text' and 'Award Text' reference the same TMP object. " +
                             "This will prevent the +/* award line from showing. " +
                             "Fix: put ScoreTMPUI on the main score text (e.g., 'total') and assign the separate award TMP text to 'Award Text'.",
                             this);
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
        int score = PlayerScore.GetScore();
        if (score != lastScore)
        {
            lastScore = score;
            if (scoreText != null)
            {
                scoreText.text = prefix + score;
            }
        }

        if (awardText != null && awardDisplaySeconds > 0f && Time.unscaledTime >= awardHideTime)
        {
            awardText.text = string.Empty;
        }
    }

    private void OnScoreAwarded(PlayerScore.ScoreAward award)
    {
        if (awardText == null) return;

        // Popup always shows how many points THIS kill was worth.
        if (showMultiplierSuffix && (award.multiplier > 1.001f || award.multiplier < 0.999f))
        {
            awardText.text = "+" + award.finalPoints + " x" + award.multiplier.ToString("0.##");
        }
        else
        {
            awardText.text = "+" + award.finalPoints;
        }

        awardHideTime = Time.unscaledTime + Mathf.Max(0f, awardDisplaySeconds);

        if (logAwardsToConsole)
        {
            Debug.Log($"[ScoreTMPUI] Award: base={award.basePoints}, mult={award.multiplier:0.##}, final={award.finalPoints}, text='{awardText.text}'", this);
        }
    }
}
