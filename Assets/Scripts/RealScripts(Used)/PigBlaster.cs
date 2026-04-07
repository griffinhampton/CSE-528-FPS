using UnityEngine;
using UnityEngine.InputSystem;

public class PigBlaster : MonoBehaviour
{
    [Header("Projectile")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform shootOrigin;
    [SerializeField] private float muzzleVelocity = 25f;
    [SerializeField] private float spawnForwardOffset = 0.5f;

    [Header("Firing")]
    [SerializeField] private float fireCooldownSeconds = 0.15f;
    [SerializeField] private InputActionReference fireAction;

    private float nextFireTime;

    private void OnEnable()
    {
        if (fireAction != null)
        {
            fireAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (fireAction != null)
        {
            fireAction.action.Disable();
        }
    }

    private void Update()
    {
        if (Time.time < nextFireTime) return;
        if (!WasFirePressedThisFrame()) return;

        Fire();
        nextFireTime = Time.time + fireCooldownSeconds;
    }

    private bool WasFirePressedThisFrame()
    {
        if (fireAction != null)
        {
            return fireAction.action.WasPressedThisFrame();
        }

        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    private void Fire()
    {
        if (ballPrefab == null)
        {
            Debug.LogWarning("[PigBlaster] No ballPrefab assigned.", this);
            return;
        }

        Transform origin = shootOrigin;
        if (origin == null)
        {
            origin = Camera.main != null ? Camera.main.transform : transform;
        }

        Vector3 spawnPos = origin.position + origin.forward * spawnForwardOffset;
        GameObject ballInstance = Instantiate(ballPrefab, spawnPos, origin.rotation);

        if (ballInstance.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.linearVelocity = origin.forward * muzzleVelocity;
        }
        else
        {
            Debug.LogWarning("[PigBlaster] Spawned ball has no Rigidbody; it will not move unless driven by another script.", ballInstance);
        }

        BallDeathOnHit hitScript = ballInstance.GetComponent<BallDeathOnHit>();
        if (hitScript == null)
        {
            hitScript = ballInstance.AddComponent<BallDeathOnHit>();
        }

        // Prevent immediately killing the shooter if the ball overlaps the player's hierarchy.
        hitScript.SetIgnoreRoot(transform.root);
    }
}
