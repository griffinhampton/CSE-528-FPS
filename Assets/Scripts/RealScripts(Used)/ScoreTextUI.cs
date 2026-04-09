using UnityEngine;
using UnityEngine.UI;

public class ScoreTextUI : MonoBehaviour
{
    [SerializeField] private Text scoreText;
    [SerializeField] private string prefix = "Score: ";

    private int lastScore = int.MinValue;

    private void Awake()
    {
        if (scoreText == null)
        {
            scoreText = GetComponent<Text>();
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
