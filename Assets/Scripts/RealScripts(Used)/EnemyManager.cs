using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour 
{
	[SerializeField] private GameObject enemyPrefab;
	[SerializeField] private Transform[] spawnPoints;
	[SerializeField] private int initialSpawnCount = 6;
	[SerializeField] private float spawnIntervalSeconds = 3f;
	[SerializeField] private bool randomYawOnSpawn = true;
	[SerializeField] private bool facePlayerOnSpawn = true;
	[SerializeField] private bool faceArenaCenterOnSpawn = true;
	[SerializeField] private Transform arenaCenter;

	[Header("Spawn Attachments")]
	[SerializeField] private GameObject splashPrefab;
	[SerializeField] private Vector3 splashLocalOffset = Vector3.zero;

	[Header("Dead Body Cleanup")]
	[SerializeField] private int maxDeadBodiesOnGround = 25;
	[SerializeField] private float deadBodyCleanupIntervalSeconds = 1.0f;
	
	[Header("Spawn Safety")]
	[SerializeField] private bool preventSpawnIfBlocked = true;
	[SerializeField] private LayerMask spawnBlockMask = ~0;
	[SerializeField] private float spawnClearRadius = 0.6f;
	[SerializeField] private int spawnNudgeAttempts = 8;
	[SerializeField] private float spawnNudgeStep = 0.75f;
	[SerializeField] private string playerTag = "Player";
	[SerializeField] private Transform playerTransform;
	[SerializeField] private bool snapToGround = true;
	[SerializeField] private LayerMask groundMask = ~0;
	[SerializeField] private float groundRayStartHeight = 5f;
	[SerializeField] private float groundRayDistance = 50f;
	[SerializeField] private float spawnYOffset = 0.25f;

	private Queue<GameObject> _spawnedEnemies = new Queue<GameObject>();
	private Coroutine _cleanupLoop;

	private Coroutine _spawnLoop;

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

	private Transform GetPlayerTransform()
	{
		if (playerTransform != null) return playerTransform;
		if (string.IsNullOrWhiteSpace(playerTag)) return null;

		GameObject player = GameObject.FindGameObjectWithTag(playerTag);
		if (player == null) return null;
		playerTransform = player.transform;
		return playerTransform;
	}

	private void Start()
	{
		CacheSpawnPointsFromChildrenIfEmpty();

		if (enemyPrefab == null)
		{
			Debug.LogError("EnemyManager: enemyPrefab is not assigned.", this);
			enabled = false;
			return;
		}

		if (spawnPoints == null || spawnPoints.Length == 0)
		{
			Debug.LogError("EnemyManager: No spawn points assigned (and none found as children).", this);
			enabled = false;
			return;
		}

		SpawnInitialEnemies(initialSpawnCount);

		if (deadBodyCleanupIntervalSeconds > 0f)
		{
			_cleanupLoop = StartCoroutine(DeadBodyCleanupLoop());
		}

		if (spawnIntervalSeconds > 0f)
		{
			_spawnLoop = StartCoroutine(SpawnLoop());
		}
	}

	private IEnumerator DeadBodyCleanupLoop()
	{
		var wait = new WaitForSeconds(deadBodyCleanupIntervalSeconds);
		while (true)
		{
			yield return wait;
			RemoveExcessDeadPigBodies();
		}
	}

	private void SpawnInitialEnemies(int count)
	{
		count = Mathf.Max(0, count);
		if (count == 0 || spawnPoints == null || spawnPoints.Length == 0)
		{
			return;
		}

		// Shuffle indices so the first batch spreads across distinct spawnpoints.
		int n = spawnPoints.Length;
		int[] indices = new int[n];
		for (int i = 0; i < n; i++)
		{
			indices[i] = i;
		}
		for (int i = 0; i < n - 1; i++)
		{
			int j = Random.Range(i, n);
			int tmp = indices[i];
			indices[i] = indices[j];
			indices[j] = tmp;
		}

		for (int i = 0; i < count; i++)
		{
			Transform spawnPoint = spawnPoints[indices[i % n]];
			SpawnEnemyAtPoint(spawnPoint);
		}
	}

	private IEnumerator SpawnLoop()
	{
		var wait = new WaitForSeconds(spawnIntervalSeconds);
		while (true)
		{
			yield return wait;
			SpawnEnemyAtRandomPoint();
		}
	}

	private void SpawnEnemyAtRandomPoint()
	{
		if (spawnPoints == null || spawnPoints.Length == 0)
		{
			return;
		}

		Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
		SpawnEnemyAtPoint(spawnPoint);
	}

	private void SpawnEnemyAtPoint(Transform spawnPoint)
	{
		if (spawnPoint == null)
		{
			return;
		}

		Vector3 position = GetSpawnPosition(spawnPoint.position);
		if (!TryFindClearSpawnPosition(spawnPoint, position, out Vector3 clearPosition))
		{
			if (preventSpawnIfBlocked)
			{
				return;
			}
		}
		else
		{
			position = clearPosition;
		}
		Quaternion rotation = spawnPoint.rotation;

		if (faceArenaCenterOnSpawn && arenaCenter != null)
		{
			Vector3 toCenter = arenaCenter.position - position;
			toCenter.y = 0f;
			if (toCenter.sqrMagnitude > 0.0001f)
			{
				rotation = Quaternion.LookRotation(toCenter.normalized, Vector3.up);
			}
		}
		else if (facePlayerOnSpawn)
		{
			Transform player = GetPlayerTransform();
			if (player != null)
			{
				Vector3 toPlayer = player.position - position;
				toPlayer.y = 0f;
				if (toPlayer.sqrMagnitude > 0.0001f)
				{
					rotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
				}
			}
		}
		else if (randomYawOnSpawn)
		{
			rotation *= Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
		}

		GameObject enemyInstance = Instantiate(enemyPrefab, position, rotation);
		_spawnedEnemies.Enqueue(enemyInstance);
		RemoveExcessDeadPigBodies();
		AttachSplash(enemyInstance);

		// Play a random Lynch clip once on death.
		if (enemyInstance.GetComponent<PigDeathAudio>() == null)
		{
			enemyInstance.AddComponent<PigDeathAudio>();
		}

		// If the spawn point defines a path, pass it to the enemy AI.
		EnemyPath path = spawnPoint.GetComponent<EnemyPath>();
		if (path != null && path.Waypoints != null && path.Waypoints.Length > 0)
		{
			EnemyAI ai = enemyInstance.GetComponent<EnemyAI>();
			if (ai == null) ai = enemyInstance.GetComponentInChildren<EnemyAI>(true);
			if (ai != null)
			{
				ai.SetSpawnPath(path.Waypoints);
			}
		}

		// Pass arena center to help enemies exit cubbies reliably.
		if (arenaCenter != null)
		{
			EnemyAI ai = enemyInstance.GetComponent<EnemyAI>();
			if (ai == null) ai = enemyInstance.GetComponentInChildren<EnemyAI>(true);
			if (ai != null)
			{
				ai.SetArenaCenter(arenaCenter);
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

		DestroyOnLiveAndLetDieDeath destroyOnDeath = splashInstance.GetComponent<DestroyOnLiveAndLetDieDeath>();
		if (destroyOnDeath == null)
		{
			destroyOnDeath = splashInstance.AddComponent<DestroyOnLiveAndLetDieDeath>();
		}
		if (TryGetLiveAndLetDie(enemyInstance, out LiveAndLetDie live) && live != null)
		{
			destroyOnDeath.SetTarget(live);
		}
	}

	private bool TryGetLiveAndLetDie(GameObject enemyRoot, out LiveAndLetDie live)
	{
		live = null;
		if (enemyRoot == null) return false;
		live = enemyRoot.GetComponent<LiveAndLetDie>();
		if (live == null) live = enemyRoot.GetComponentInChildren<LiveAndLetDie>(true);
		if (live == null) live = enemyRoot.GetComponentInParent<LiveAndLetDie>();
		return live != null;
	}

	// Removes dead pig bodies if there are too many on the ground.
	// Uses spawn order to always remove the oldest spawned dead pig first.
	public void RemoveExcessDeadPigBodies()
	{
		int maxDead = Mathf.Max(0, maxDeadBodiesOnGround);
		if (maxDead <= 0)
		{
			// If set to 0, remove all dead bodies.
			maxDead = 0;
		}

		// Count how many dead bodies currently exist.
		int deadCount = 0;
		foreach (GameObject enemy in _spawnedEnemies)
		{
			if (enemy == null) continue;
			if (TryGetLiveAndLetDie(enemy, out LiveAndLetDie live) && live != null && live.IsDead)
			{
				deadCount++;
			}
		}

		if (deadCount <= maxDead)
		{
			return;
		}

		int toRemove = deadCount - maxDead;
		if (toRemove <= 0) return;

		// Rebuild the queue while destroying the oldest spawned dead pigs first.
		Queue<GameObject> rebuilt = new Queue<GameObject>(_spawnedEnemies.Count);
		int removed = 0;

		while (_spawnedEnemies.Count > 0)
		{
			GameObject enemy = _spawnedEnemies.Dequeue();
			if (enemy == null) continue;

			if (removed < toRemove && TryGetLiveAndLetDie(enemy, out LiveAndLetDie live) && live != null && live.IsDead)
			{
				Destroy(enemy);
				removed++;
				continue;
			}

			rebuilt.Enqueue(enemy);
		}

		_spawnedEnemies = rebuilt;
	}

	private bool TryFindClearSpawnPosition(Transform spawnPoint, Vector3 basePosition, out Vector3 clearPosition)
	{
		clearPosition = basePosition;
		float r = Mathf.Max(0f, spawnClearRadius);
		if (r <= 0f) return true;

		// Direction to nudge: prefer facing toward arena center, otherwise spawnPoint forward.
		Vector3 nudgeDir = spawnPoint != null ? spawnPoint.forward : Vector3.forward;
		if (arenaCenter != null)
		{
			Vector3 toCenter = arenaCenter.position - basePosition;
			toCenter.y = 0f;
			if (toCenter.sqrMagnitude > 0.0001f)
			{
				nudgeDir = toCenter.normalized;
			}
		}
		if (nudgeDir.sqrMagnitude < 0.0001f) nudgeDir = Vector3.forward;

		int attempts = Mathf.Max(1, spawnNudgeAttempts);
		float step = Mathf.Max(0.01f, spawnNudgeStep);

		for (int i = 0; i < attempts; i++)
		{
			Vector3 candidate = basePosition + nudgeDir * (i * step);
			Collider[] overlaps = Physics.OverlapSphere(candidate, r, spawnBlockMask, QueryTriggerInteraction.Ignore);
			bool blocked = false;
			for (int j = 0; j < overlaps.Length; j++)
			{
				Collider c = overlaps[j];
				if (c == null) continue;
				// Ignore trigger volumes; we're checking physical blockage.
				if (c.isTrigger) continue;
				blocked = true;
				break;
			}

			if (!blocked)
			{
				clearPosition = candidate;
				return true;
			}
		}

		return false;
	}

	private Vector3 GetSpawnPosition(Vector3 basePosition)
	{
		Vector3 position = basePosition;

		if (snapToGround)
		{
			Vector3 rayStart = basePosition + Vector3.up * Mathf.Max(0f, groundRayStartHeight);
			if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, groundRayDistance, groundMask, QueryTriggerInteraction.Ignore))
			{
				position = hit.point;
			}
		}

		position += Vector3.up * spawnYOffset;
		return position;
	}

	private void CacheSpawnPointsFromChildrenIfEmpty()
	{
		if (spawnPoints != null && spawnPoints.Length > 0)
		{
			return;
		}

		int childCount = transform.childCount;
		if (childCount == 0)
		{
			return;
		}

		var points = new List<Transform>(childCount);
		for (int i = 0; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (child != null)
			{
				points.Add(child);
			}
		}
		spawnPoints = points.ToArray();
	}
}
