using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

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

    [Header("Audio")]
    [Tooltip("Gunshot sound to play on fire. If empty, auto-loads Assets/General Assets/Audio/Lynch/gunshot.wav in the Unity Editor.")]
    [SerializeField] private AudioClip gunshotClip;
    [Range(0f, 1f)]
    [SerializeField] private float gunshotVolume = 1f;

    private float nextFireTime;
    private AudioSource audioSource;

#if UNITY_EDITOR
    private static AudioClip cachedEditorGunshot;
#endif

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        TryAutoPopulateGunshotInEditor();
    }

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
        if (PauseMenuUI.IsPaused) return false;

        // Escape is reserved for pause; ignore it for firing even if bindings include it.
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return false;
        }

        if (fireAction != null)
        {
            return fireAction.action.WasPressedThisFrame();
        }

        return Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
    }

    private void Fire()
    {
        if (gunshotClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(gunshotClip, Mathf.Clamp01(gunshotVolume));
        }

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

    private void TryAutoPopulateGunshotInEditor()
    {
        if (gunshotClip != null) return;

#if UNITY_EDITOR
        if (cachedEditorGunshot == null)
        {
            cachedEditorGunshot = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/General Assets/Audio/Lynch/gunshot.wav");
        }

        gunshotClip = cachedEditorGunshot;
#endif
    }
}
