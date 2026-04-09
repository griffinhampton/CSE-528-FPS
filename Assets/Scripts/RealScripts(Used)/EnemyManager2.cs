using UnityEngine;
using System.Collections;

public class EnemyManager2 : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab1;
    [SerializeField] private GameObject enemyPrefab2;
    [SerializeField] private GameObject enemyPrefab3;

    public int numEnemies = 6;
    private GameObject[] _enemy;

    void Start()
    {
        if (numEnemies < 1) numEnemies = 1;

        _enemy = new GameObject[numEnemies];
    }

    void Update()
    {
        for (int i = 0; i < numEnemies; i++)
        {
             if (_enemy[i] == null)
             {
                 float value = Random.value;
                 if (value < 1.0f / 3.0f)
                     _enemy[i] = Instantiate(enemyPrefab1) as GameObject;
                 else if (value < 2.0f / 3.0f)
                     _enemy[i] = Instantiate(enemyPrefab2) as GameObject;
                 else
                     _enemy[i] = Instantiate(enemyPrefab3) as GameObject;

                 _enemy[i].transform.position = new Vector3(0, 1, 0);
                 float angle = Random.Range(0, 360);
                 _enemy[i].transform.Rotate(0, angle, 0);
             }
        }
    }
}