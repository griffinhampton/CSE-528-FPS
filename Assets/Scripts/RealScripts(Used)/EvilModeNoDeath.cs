using UnityEngine;

public class EvilModeNoDeath : MonoBehaviour
{
    [Tooltip("If true, disables PlayerHealth.Died events while this object is enabled.")]
    [SerializeField] private bool disableDeathWhileEnabled = true;

    private bool applied;

    private void OnEnable()
    {
        if (!disableDeathWhileEnabled) return;

        if (!applied)
        {
            PlayerHealth.RequestDeathDisallowed();
            applied = true;
        }
    }

    private void OnDisable()
    {
        Release();
    }

    private void OnDestroy()
    {
        Release();
    }

    private void Release()
    {
        if (!applied) return;
        PlayerHealth.ReleaseDeathDisallowed();
        applied = false;
    }
}
