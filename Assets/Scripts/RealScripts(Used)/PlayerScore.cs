using UnityEngine;
using System;

public class PlayerScore : MonoBehaviour
{
    [Serializable]
    public struct ScoreAward
    {
        public int basePoints;
        public float multiplier;
        [Tooltip("How many points the player actually gained (newTotal - oldTotal).")]
        public int finalPoints;

        public int oldTotal;
        public int newTotal;

        public ScoreAward(int basePoints, float multiplier, int finalPoints, int oldTotal, int newTotal)
        {
            this.basePoints = basePoints;
            this.multiplier = multiplier;
            this.finalPoints = finalPoints;
            this.oldTotal = oldTotal;
            this.newTotal = newTotal;
        }
    }

    public static event Action<ScoreAward> ScoreAwarded;

    [Tooltip("Current score for this player.")]
    [SerializeField] private int score;

    public int Score => score;

    public static PlayerScore Instance { get; private set; }

    // Allows score to still work (e.g., in a test scene) even if no PlayerScore exists.
    private static int fallbackScore;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Keep the first instance.
            return;
        }

        Instance = this;

        // If we were accumulating score before the PlayerScore existed, fold it in.
        if (fallbackScore != 0)
        {
            score += fallbackScore;
            fallbackScore = 0;
        }
    }

    public void Add(int points)
    {
        if (points <= 0) return;
        score += points;
    }

    private void SetTotal(int total)
    {
        score = Mathf.Max(0, total);
    }

    public static void AddScore(int points)
    {
        AddScoreWithMultiplier(points, 1f);
    }

    public static void AddScoreWithMultiplier(int basePoints, float multiplier)
    {
        basePoints = Mathf.Max(0, basePoints);
        if (basePoints == 0) return;

        // Multipliers are meant to be beneficial; clamp to at least 1.
        if (multiplier < 1f) multiplier = 1f;

        int oldTotal = GetScore();
        int newTotal = Mathf.RoundToInt((oldTotal + basePoints) * multiplier);
        if (newTotal < 0) newTotal = 0;

        int delta = newTotal - oldTotal;
        if (delta <= 0) return;

        ApplyTotalAndNotify(new ScoreAward(
            basePoints: basePoints,
            multiplier: multiplier,
            finalPoints: delta,
            oldTotal: oldTotal,
            newTotal: newTotal));
    }

    private static void ApplyTotalAndNotify(ScoreAward award)
    {
        if (Instance != null)
        {
            Instance.SetTotal(award.newTotal);
        }
        else
        {
            fallbackScore = Mathf.Max(0, award.newTotal);
        }

        try
        {
            ScoreAwarded?.Invoke(award);
        }
        catch (Exception ex)
        {
            // Ignore listener exceptions so scoring can't break gameplay,
            // but make them visible during development.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogException(ex);
#endif
        }
    }

    public static int GetScore()
    {
        return Instance != null ? Instance.Score : fallbackScore;
    }

    public static void ResetScore()
    {
        if (Instance != null)
        {
            Instance.SetTotal(0);
        }

        fallbackScore = 0;
    }
}
