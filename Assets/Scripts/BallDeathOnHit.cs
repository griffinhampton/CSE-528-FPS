using UnityEngine;

public class BallDeathOnHit : MonoBehaviour
{
    [Header("Behavior")]
    [SerializeField] private bool destroyOnHit = true;
    [SerializeField] private float destroyAfterHitSeconds = 5f;
    [SerializeField] private float destroyIfNoHitAfterSeconds = 30f;

    [Header("Filtering")]
    [SerializeField] private Transform ignoreRoot;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    private bool hasHit;
    private Coroutine noHitDespawnRoutine;

    private void Awake()
    {
        if (destroyIfNoHitAfterSeconds > 0f)
        {
            noHitDespawnRoutine = StartCoroutine(DespawnIfNoHitAfterDelay(destroyIfNoHitAfterSeconds));
        }
    }

    public void SetIgnoreRoot(Transform root)
    {
        ignoreRoot = root;
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleHit(collision.gameObject, "OnCollisionEnter");
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other.gameObject, "OnTriggerEnter");
    }

    private void HandleHit(GameObject other, string eventName)
    {
        if (hasHit) return;

        if (ignoreRoot != null && other.transform.IsChildOf(ignoreRoot))
        {
            return;
        }

        hasHit = true;

        LiveAndLetDie liveAndLetDie = other.GetComponent<LiveAndLetDie>();
        if (liveAndLetDie == null) liveAndLetDie = other.GetComponentInChildren<LiveAndLetDie>(true);
        if (liveAndLetDie == null) liveAndLetDie = other.GetComponentInParent<LiveAndLetDie>();

        if (liveAndLetDie != null)
        {
            if (debugLog)
            {
                Debug.Log($"[BallDeathOnHit] {eventName} hit '{other.name}' -> calling LiveAndLetDie.Death()", this);
            }
            liveAndLetDie.Death();
        }
        else
        {
            if (debugLog)
            {
                Debug.Log($"[BallDeathOnHit] {eventName} hit '{other.name}' (no LiveAndLetDie found)", this);
            }
        }

        if (destroyOnHit)
        {
            if (noHitDespawnRoutine != null)
            {
                StopCoroutine(noHitDespawnRoutine);
                noHitDespawnRoutine = null;
            }

            StartCoroutine(DespawnAfterDelay(Mathf.Max(0f, destroyAfterHitSeconds)));
        }
    }

    private System.Collections.IEnumerator DespawnIfNoHitAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (!hasHit)
        {
            Destroy(gameObject);
        }
    }

    private System.Collections.IEnumerator DespawnAfterDelay(float seconds)
    {
        if (seconds > 0f)
        {
            yield return new WaitForSeconds(seconds);
        }
        Destroy(gameObject);
    }
}
