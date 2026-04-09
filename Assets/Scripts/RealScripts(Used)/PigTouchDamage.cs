using UnityEngine;

public class PigTouchDamage : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Damage applied to the player when this (alive) pig touches them.")]
    [Min(0)]
    [SerializeField] private int touchDamage = 1;

    [Tooltip("If true, this pig will die (ragdoll) when it touches the player.")]
    [SerializeField] private bool dieOnTouch = true;

    private LiveAndLetDie liveAndLetDie;

    private void Awake()
    {
        liveAndLetDie = GetComponent<LiveAndLetDie>();
        if (liveAndLetDie == null) liveAndLetDie = GetComponentInChildren<LiveAndLetDie>(true);
        if (liveAndLetDie == null) liveAndLetDie = GetComponentInParent<LiveAndLetDie>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;
        HandleTouch(collision.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleTouch(other);
    }

    private void HandleTouch(Collider other)
    {
        if (other == null) return;

        // Only alive pigs can trigger damage.
        if (liveAndLetDie != null && liveAndLetDie.IsDead) return;

        bool isPlayer = false;

        // Prefer component check (works even if tags differ).
        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();
        if (playerHealth != null)
        {
            isPlayer = true;
        }
        else if (!string.IsNullOrWhiteSpace(playerTag))
        {
            // Fallback: tag check.
            if (other.CompareTag(playerTag) || other.transform.root.CompareTag(playerTag))
            {
                isPlayer = true;
            }
        }

        if (!isPlayer) return;

        if (dieOnTouch && liveAndLetDie != null)
        {
            liveAndLetDie.Death();
        }

        PlayerHealth.DamagePlayer(touchDamage, source: gameObject);
    }
}
