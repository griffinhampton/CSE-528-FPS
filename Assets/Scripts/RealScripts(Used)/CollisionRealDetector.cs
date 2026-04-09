using UnityEngine;

public class CollisionRealDetector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private string targetTag = "Player";
    [SerializeField] private bool logOnlyOnce = false;

    [Header("Actions")]
    [SerializeField] private LiveAndLetDie liveAndLetDie;
    [SerializeField] private bool callDeathOnHit = true;

    [Tooltip("If true, damages the player when this (alive) pig touches them.")]
    [SerializeField] private bool damagePlayerOnHit = true;

    [Tooltip("How much damage to apply when the player is touched.")]
    [Min(0)]
    [SerializeField] private int touchDamage = 1;

    private bool hasLogged;
    private bool hasAppliedHit;

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
        if (other == null) return;
        if (!other.CompareTag(targetTag)) return;

        // Cache LiveAndLetDie once, if needed.
        if (liveAndLetDie == null)
        {
            liveAndLetDie = GetComponent<LiveAndLetDie>();
            if (liveAndLetDie == null) liveAndLetDie = GetComponentInChildren<LiveAndLetDie>(true);
            if (liveAndLetDie == null) liveAndLetDie = GetComponentInParent<LiveAndLetDie>();
        }

        // Only an ALIVE pig can apply the touch effect.
        if (liveAndLetDie != null && liveAndLetDie.IsDead) return;

        // Prevent multiple contacts from spamming damage/death.
        if (hasAppliedHit) return;
        hasAppliedHit = true;

        if (logOnlyOnce && hasLogged) return;

        hasLogged = true;
        Debug.Log($"[CollisionRealDetector] {eventName} on '{gameObject.name}' (tagged '{targetTag}')", this);

        if (damagePlayerOnHit && touchDamage > 0)
        {
            PlayerHealth.DamagePlayer(touchDamage, source: gameObject);
        }

        if (callDeathOnHit)
        {
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
