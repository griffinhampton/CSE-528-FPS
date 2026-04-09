using UnityEngine;
using TMPro;

public class ScoreTotalTMPUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private string prefix = "Score: ";

    private int lastScore = int.MinValue;

    private void Awake()
    {
        if (scoreText == null)
        {
            scoreText = GetComponent<TMP_Text>();
        }
    }

    private void Update()
    {
        int score = PlayerScore.GetScore();
        if (score == lastScore) return;

        lastScore = score;

        if (scoreText != null)
        {
            scoreText.text = prefix + score;
        }
    }
}
