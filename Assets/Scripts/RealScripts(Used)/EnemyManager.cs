using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour 
{
	[SerializeField] private GameObject enemyPrefab;
	private GameObject _enemy;
    void Start()
    {
        for(int i = 0; i<10; i++)
		{
			_enemy = Instantiate(enemyPrefab) as GameObject;
			_enemy.transform.position = new Vector3(0, 1, 0);
			float angle = Random.Range(0, 360);
			_enemy.transform.Rotate(0, angle, 0);
		}
    }
    void Update() 
    {
		
		if (_enemy == null) {
			_enemy = Instantiate(enemyPrefab) as GameObject;
			_enemy.transform.position = new Vector3(0, 1, 0);
			float angle = Random.Range(0, 360);
			_enemy.transform.Rotate(0, angle, 0);
		}
	}
}
