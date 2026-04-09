using UnityEngine;
using System.Collections;

public class EnemyManager2 : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab1;
    [SerializeField] private GameObject enemyPrefab2;
    [SerializeField] private GameObject enemyPrefab3;

    [Header("Spawn Attachments")]
    [SerializeField] private GameObject splashPrefab;
    [SerializeField] private Vector3 splashLocalOffset = Vector3.zero;

    public int numEnemies = 6;
    private GameObject[] _enemy;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (splashPrefab == null)
        {
            splashPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/shader/Ripple + Water/Splash.prefab");
        }
    }
#endif

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

                 AttachSplash(_enemy[i]);
             }
        }
    }

    private void AttachSplash(GameObject enemyInstance)
    {
        if (enemyInstance == null) return;
        if (splashPrefab == null) return;

        Transform parent = enemyInstance.transform;
        GameObject splashInstance = Instantiate(splashPrefab, parent, false);
        splashInstance.transform.localPosition = splashLocalOffset;
        splashInstance.transform.localRotation = Quaternion.identity;
        splashInstance.transform.localScale = Vector3.one;

        if (splashInstance.GetComponent<DestroyOnLiveAndLetDieDeath>() == null)
        {
            splashInstance.AddComponent<DestroyOnLiveAndLetDieDeath>();
        }
    }
}