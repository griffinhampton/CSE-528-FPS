using UnityEngine;

/// <summary>
/// Defines a simple waypoint path for enemies spawned at this location.
/// Attach this to a spawn point (cubby) and assign waypoints in order.
/// </summary>
public class EnemyPath : MonoBehaviour
{
    [Tooltip("Waypoints the enemy should follow in order after spawning.")]
    [SerializeField] private Transform[] waypoints;

    public Transform[] Waypoints => waypoints;
}
