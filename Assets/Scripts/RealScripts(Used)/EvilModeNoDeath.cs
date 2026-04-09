using UnityEngine;

public class EvilModeNoDeath : MonoBehaviour
{
    [Tooltip("If true, disables PlayerHealth.Died events while this object is enabled.")]
    [SerializeField] private bool disableDeathWhileEnabled = true;

    private bool previous;
    private bool applied;

    private void OnEnable()
    {
        if (!disableDeathWhileEnabled) return;

        previous = PlayerHealth.IsDeathAllowed();
        PlayerHealth.SetDeathAllowed(false);
        applied = true;
    }

    private void OnDisable()
    {
        if (!applied) return;
        PlayerHealth.SetDeathAllowed(previous);
        applied = false;
    }
}
