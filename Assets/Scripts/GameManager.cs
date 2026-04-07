using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public float timScale = 1.0f;
    //public GameObject Text_EnemyHit;
    public TextMeshProUGUI Text_EnemyHit;

    int numberOfEnemyHit = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = timScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnemyHit()
    {
        numberOfEnemyHit++;
        //Text_EnemyHit.GetComponent<Text>().text =
        //    numberOfEnemyHit.ToString();
        Text_EnemyHit.text = numberOfEnemyHit.ToString();
    }
}
