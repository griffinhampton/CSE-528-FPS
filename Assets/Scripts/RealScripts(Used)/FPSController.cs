using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FPSController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintMultiplier = 2.0f;

    [Header("Jump Parameters")]
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private float gravity = 9.81f;

    [Header("Look Sensitivity")]
    [SerializeField] private float mouseSensitivity = 2.0f;
    [SerializeField] private float upDownRange = 90.0f;

    [Header("Health")]
    [Min(0)]
    [SerializeField] private int pigTouchDamage = 1;

    [SerializeField]private bool killPigAndDamageOnTouch = true;

    [Header("Audio")]
    [SerializeField] private AudioClip[] footstepClips;

    private CharacterController characterController;
    private Camera mainCamera;
    private PlayInputHandler inputHandler;
    private Vector3 currentMovement;
    private float verticalRotation;

    LayerMask layerMask;
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        mainCamera = Camera.main;
        layerMask = LayerMask.GetMask("Default"); 
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (footstepClips != null && footstepClips.Length > 0) return;

        string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/General Assets/Audio/WalkSounds" });
        if (guids != null && guids.Length > 0)
        {
            footstepClips = new AudioClip[guids.Length];
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                footstepClips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            }
        }
        else
        {
            footstepClips = System.Array.Empty<AudioClip>();
        }
    }
#endif

    private void Start()
    {
        inputHandler = PlayInputHandler.Instance;

        PlayerFootstepAudio steps = GetComponent<PlayerFootstepAudio>();
        if (steps == null)
        {
            steps = gameObject.AddComponent<PlayerFootstepAudio>();
        }

        if (steps != null && footstepClips != null && footstepClips.Length > 0)
        {
            steps.SetClips(footstepClips);
        }
    }

    private void Update()
    {
        if (inputHandler == null)
        {
            inputHandler = PlayInputHandler.Instance;
        }

        if (Time.timeScale == 0f)
        {
            if (inputHandler != null)
            {
                inputHandler.ResetJump();
            }
            return;
        }

        if (inputHandler == null) return;
        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        float speed = walkSpeed * (inputHandler.SprintValue > 0 ? sprintMultiplier : 1f);

        Vector3 inputDirection = new Vector3(inputHandler.MoveInput.x, 0f, inputHandler.MoveInput.y);
        Vector3 worldDirection = transform.TransformDirection(inputDirection);
        worldDirection.Normalize();

        currentMovement.x = worldDirection.x * speed;
        currentMovement.z = worldDirection.z * speed;

        HandleJumping();

        characterController.Move(currentMovement * Time.deltaTime);
    }

    void HandleJumping()
    {
        Ray jumpray = new Ray(mainCamera.transform.position, Vector3.down);
        RaycastHit jumphit;
        if( Physics.Raycast(jumpray, out jumphit, 2.2f, layerMask ))
        {
            currentMovement.y -= gravity * Time.deltaTime;

            if (inputHandler.JumpTriggered)
            {
                currentMovement.y = jumpForce;
            }
        }
        else 
        {
            currentMovement.y -= gravity * Time.deltaTime;
        }

        inputHandler.ResetJump();
    }
    void HandleRotation()
    {
        float mouseXRotation = inputHandler.LookInput.x * mouseSensitivity;
        transform.Rotate(0, mouseXRotation, 0);

        verticalRotation -= inputHandler.LookInput.y * mouseSensitivity;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        mainCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0,0);
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!killPigAndDamageOnTouch) return;
        if (hit == null) return;
        HandlePigTouch(hit.collider);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!killPigAndDamageOnTouch) return;
        HandlePigTouch(other);
    }

    private void HandlePigTouch(Collider other)
    {
        if (other == null) return;

        EnemyAI pigAI = other.GetComponentInParent<EnemyAI>();
        if (pigAI == null) return;

        LiveAndLetDie liveAndLetDie = pigAI.GetComponent<LiveAndLetDie>();
        if (liveAndLetDie == null) liveAndLetDie = pigAI.GetComponentInChildren<LiveAndLetDie>(true);
        if (liveAndLetDie == null) liveAndLetDie = pigAI.GetComponentInParent<LiveAndLetDie>();

        if (liveAndLetDie != null && !liveAndLetDie.IsDead)
        {
            liveAndLetDie.Death();
            PlayerHealth.DamagePlayer(pigTouchDamage, source: pigAI.gameObject);
        }
    }

}
