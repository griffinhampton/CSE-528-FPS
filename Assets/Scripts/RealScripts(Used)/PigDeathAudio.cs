using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PigDeathAudio : MonoBehaviour
{
    [Header("Clips")]
    [Tooltip("If empty, this will try to auto-populate from Assets/General Assets/Audio/Lynch while in the Unity Editor.")]
    [SerializeField] private AudioClip[] lynchClips;

    [Header("Audio")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Tooltip("3D audio if true; 2D if false")]
    [SerializeField] private bool spatial3D = true;

    private AudioSource audioSource;
    private LiveAndLetDie liveAndLetDie;
    private bool played;

#if UNITY_EDITOR
    private static AudioClip[] cachedEditorLynchClips;
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
        audioSource.spatialBlend = spatial3D ? 1f : 0f;

        liveAndLetDie = GetComponent<LiveAndLetDie>();
        if (liveAndLetDie == null) liveAndLetDie = GetComponentInChildren<LiveAndLetDie>(true);
        if (liveAndLetDie == null) liveAndLetDie = GetComponentInParent<LiveAndLetDie>();

        TryAutoPopulateClipsInEditor();
    }

    private void Update()
    {
        if (played) return;
        if (liveAndLetDie == null) return;

        if (liveAndLetDie.IsDead)
        {
            played = true;
            PlayRandomClipOnce();
        }
    }

    private void PlayRandomClipOnce()
    {
        if (lynchClips == null || lynchClips.Length == 0)
        {
            // If this is a build, clips must be assigned manually (or moved to Resources and loaded differently).
            return;
        }

        int idx = Random.Range(0, lynchClips.Length);
        AudioClip clip = lynchClips[idx];
        if (clip == null) return;

        audioSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    private void TryAutoPopulateClipsInEditor()
    {
        if (lynchClips != null && lynchClips.Length > 0) return;

#if UNITY_EDITOR
        if (cachedEditorLynchClips == null || cachedEditorLynchClips.Length == 0)
        {
            string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/General Assets/Audio/Lynch" });
            if (guids != null && guids.Length > 0)
            {
                cachedEditorLynchClips = new AudioClip[guids.Length];
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    cachedEditorLynchClips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                }
            }
            else
            {
                cachedEditorLynchClips = System.Array.Empty<AudioClip>();
            }
        }

        lynchClips = cachedEditorLynchClips;
#endif
    }
}
