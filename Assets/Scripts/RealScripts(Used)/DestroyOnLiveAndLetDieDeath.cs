using UnityEngine;

public class DestroyOnLiveAndLetDieDeath : MonoBehaviour
{
    [SerializeField] private LiveAndLetDie target;
    [SerializeField] private float destroyDelaySeconds = 0f;

    private bool queued;

    public void SetTarget(LiveAndLetDie liveAndLetDie)
    {
        target = liveAndLetDie;
    }

    private void Awake()
    {
        if (target == null)
        {
            target = GetComponentInParent<LiveAndLetDie>();
        }
    }

    private void Update()
    {
        if (queued) return;
        if (target == null) return;

        if (target.IsDead)
        {
            queued = true;
            if (destroyDelaySeconds <= 0f)
            {
                Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject, destroyDelaySeconds);
            }
        }
    }
}
