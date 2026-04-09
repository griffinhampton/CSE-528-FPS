using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(CharacterController))]
public class PlayerFootstepAudio : MonoBehaviour
{
    [Header("Clips")]
    [Tooltip("If empty, this will try to auto-populate from Assets/General Assets/Audio/WalkSounds while in the Unity Editor.")]
    [SerializeField] private AudioClip[] walkClips;

    [Header("Timing")]
    [Tooltip("Minimum time between steps while moving.")]
    [SerializeField] private float stepIntervalSeconds = 0.45f;

    [Tooltip("Extra speed scaling. 1 = normal cadence, >1 = faster steps.")]
    [SerializeField] private float cadenceMultiplier = 1f;

    [Header("Audio")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.4f;

    [Tooltip("3D audio if true; 2D if false")]
    [SerializeField] private bool spatial3D = false;

    [Tooltip("If true, only plays steps while grounded.")]
    [SerializeField] private bool requireGrounded = true;

    private AudioSource audioSource;
    private CharacterController characterController;
    private PlayInputHandler input;

    private float nextStepTime;
    private int lastClipIndex = -1;

#if UNITY_EDITOR
    private static AudioClip[] cachedEditorWalkClips;
#endif

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = spatial3D ? 1f : 0f;

        input = PlayInputHandler.Instance;

        TryAutoPopulateClipsInEditor();
    }

    private void Update()
    {
        if (PauseMenuUI.IsPaused) return;
        if (Time.time < nextStepTime) return;

        if (requireGrounded && characterController != null && !characterController.isGrounded)
        {
            return;
        }

        if (!IsPlayerMoving())
        {
            return;
        }

        PlayRandomStep();

        float interval = Mathf.Max(0.05f, stepIntervalSeconds);
        float multiplier = Mathf.Max(0.01f, cadenceMultiplier);
        nextStepTime = Time.time + (interval / multiplier);
    }

    private bool IsPlayerMoving()
    {
        if (input == null)
        {
            input = PlayInputHandler.Instance;
        }

        if (input != null)
        {
            return input.MoveInput.sqrMagnitude > 0.01f;
        }

        // Fallback to controller velocity.
        if (characterController != null)
        {
            Vector3 v = characterController.velocity;
            v.y = 0f;
            return v.sqrMagnitude > 0.01f;
        }

        return false;
    }

    private void PlayRandomStep()
    {
        if (walkClips == null || walkClips.Length == 0) return;

        int idx;
        if (walkClips.Length == 1)
        {
            idx = 0;
        }
        else
        {
            // Avoid repeating the same clip twice in a row when possible.
            idx = Random.Range(0, walkClips.Length);
            if (idx == lastClipIndex)
            {
                idx = (idx + 1) % walkClips.Length;
            }
        }

        lastClipIndex = idx;
        AudioClip clip = walkClips[idx];
        if (clip == null) return;

        audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    private void TryAutoPopulateClipsInEditor()
    {
        if (walkClips != null && walkClips.Length > 0) return;

#if UNITY_EDITOR
        if (cachedEditorWalkClips == null || cachedEditorWalkClips.Length == 0)
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/General Assets/Audio/WalkSounds" });
            if (guids != null && guids.Length > 0)
            {
                cachedEditorWalkClips = new AudioClip[guids.Length];
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    cachedEditorWalkClips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                }
            }
            else
            {
                cachedEditorWalkClips = System.Array.Empty<AudioClip>();
            }
        }

        walkClips = cachedEditorWalkClips;
#endif
    }
}
