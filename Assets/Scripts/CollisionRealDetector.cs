using UnityEngine;

public class CollisionRealDetector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private bool logOnlyOnce = false;

    [Header("Actions")]
    [SerializeField] private LiveAndLetDie liveAndLetDie;
    [SerializeField] private bool callDeathOnHit = true;

    private bool hasLogged;

    private void OnCollisionEnter(Collision collision)
    {
        TryLogHit(collision.gameObject, $"OnCollisionEnter with '{collision.gameObject.name}'");
    }

    private void OnTriggerEnter(Collider other)
    {
        TryLogHit(other.gameObject, $"OnTriggerEnter with '{other.gameObject.name}'");
    }

    private void TryLogHit(GameObject other, string eventName)
    {
        if (logOnlyOnce && hasLogged) return;
        if (!other.CompareTag(targetTag)) return;

        hasLogged = true;
        Debug.Log($"[CollisionRealDetector] {eventName} on '{gameObject.name}' (tagged '{targetTag}')", this);

        if (callDeathOnHit)
        {
            if (liveAndLetDie == null)
            {
                liveAndLetDie = GetComponent<LiveAndLetDie>();
                if (liveAndLetDie == null)
                {
                    liveAndLetDie = GetComponentInChildren<LiveAndLetDie>(true);
                }
                if (liveAndLetDie == null)
                {
                    liveAndLetDie = GetComponentInParent<LiveAndLetDie>();
                }
            }

            if (liveAndLetDie != null)
            {
                liveAndLetDie.Death();
            }
            else
            {
                Debug.LogWarning($"[CollisionRealDetector] callDeathOnHit is enabled but no LiveAndLetDie was found on '{gameObject.name}' or its parents.", this);
            }
        }
    }
}
