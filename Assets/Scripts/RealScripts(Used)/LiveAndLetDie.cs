using UnityEngine;
using System.Collections.Generic;

public class LiveAndLetDie : MonoBehaviour
{
    [Header("Ragdoll")]
    [Tooltip("Optional: assign the root transform of the ragdoll (Armature). If null, uses this GameObject.")]
    [SerializeField] private Transform ragdollRoot;

    [Tooltip("Optional: Rigidbody to exclude from ragdoll freezing (usually the enemy root Rigidbody used for movement/gravity). If null, auto-detects on this GameObject.")]
    [SerializeField] private Rigidbody excludeRigidbody;

    [Header("Colliders")]
    [Tooltip("If true, ragdoll colliders stay enabled while alive (bones remain kinematic). Useful if you want accurate hitboxes instead of a big parent collider.")]
    [SerializeField] private bool enableRagdollCollidersWhileAlive = false;

    [Tooltip("Colliders used while alive (typically the big parent capsule/box). These will be disabled on Death(). If empty, will be auto-detected.")]
    [SerializeField] private Collider[] aliveColliders;

    [Header("Joints")]
    [Tooltip("Disable ragdoll joints while alive so kinematic bones cannot constrain/hold up the main body. Joints are re-enabled on Death().")]
    [SerializeField] private bool disableRagdollJointsWhileAlive = true;

    private Rigidbody[] bones;
    private Collider[] ragdollColliders;
    private Joint[] ragdollJoints;
    private HashSet<Rigidbody> boneSet;

    private struct JointConnectionState
    {
        public Rigidbody ConnectedBody;
        public Vector3 ConnectedAnchor;
        public bool AutoConfigureConnectedAnchor;
    }

    private Dictionary<Joint, JointConnectionState> cachedJointConnections;

    public bool IsDead { get; private set; }

    private void Awake()
    {
        if (excludeRigidbody == null)
        {
            excludeRigidbody = GetComponent<Rigidbody>();
        }

        InitializeRagdoll();

        if (aliveColliders == null || aliveColliders.Length == 0)
        {
            CacheAliveColliders();
        }

        SetAliveState();
        IsDead = false;
    }

    private void InitializeRagdoll()
    {
        if (ragdollRoot == null)
        {
            ragdollRoot = transform;
        }

        Rigidbody[] allBones = ragdollRoot.GetComponentsInChildren<Rigidbody>(true);
        List<Rigidbody> filteredBones = new List<Rigidbody>(allBones.Length);
        for (int i = 0; i < allBones.Length; i++)
        {
            Rigidbody bone = allBones[i];
            if (bone == null) continue;
            if (excludeRigidbody != null && bone == excludeRigidbody) continue;
            filteredBones.Add(bone);
        }

        bones = filteredBones.ToArray();
        boneSet = new HashSet<Rigidbody>(bones);

        List<Collider> boneColliders = new List<Collider>();
        Collider[] allChildColliders = ragdollRoot.GetComponentsInChildren<Collider>(true);
        foreach (Collider col in allChildColliders)
        {
            Rigidbody attached = col.attachedRigidbody;
            if (attached != null && boneSet.Contains(attached) && (excludeRigidbody == null || attached != excludeRigidbody))
            {
                boneColliders.Add(col);
            }
        }
        ragdollColliders = boneColliders.ToArray();

        // Cache joints on ragdoll bones (these can constrain a root Rigidbody even if bones are kinematic).
        Joint[] allJoints = ragdollRoot.GetComponentsInChildren<Joint>(true);
        List<Joint> boneJoints = new List<Joint>(allJoints.Length);
        foreach (Joint joint in allJoints)
        {
            if (joint == null) continue;
            Rigidbody attached = joint.GetComponent<Rigidbody>();
            if (attached == null) continue;
            if (excludeRigidbody != null && attached == excludeRigidbody) continue;
            if (boneSet != null && boneSet.Contains(attached))
            {
                boneJoints.Add(joint);
            }
        }
        ragdollJoints = boneJoints.ToArray();

        CacheJointConnections();
    }

    private void CacheJointConnections()
    {
        if (cachedJointConnections == null)
        {
            cachedJointConnections = new Dictionary<Joint, JointConnectionState>();
        }

        if (ragdollJoints == null) return;

        foreach (Joint joint in ragdollJoints)
        {
            if (joint == null) continue;
            if (cachedJointConnections.ContainsKey(joint)) continue;

            cachedJointConnections[joint] = new JointConnectionState
            {
                ConnectedBody = joint.connectedBody,
                ConnectedAnchor = joint.connectedAnchor,
                AutoConfigureConnectedAnchor = joint.autoConfigureConnectedAnchor
            };
        }
    }

    private void SetJointDisconnected(Joint joint)
    {
        if (joint == null) return;

        CacheJointConnections();

        if (cachedJointConnections != null && !cachedJointConnections.ContainsKey(joint))
        {
            cachedJointConnections[joint] = new JointConnectionState
            {
                ConnectedBody = joint.connectedBody,
                ConnectedAnchor = joint.connectedAnchor,
                AutoConfigureConnectedAnchor = joint.autoConfigureConnectedAnchor
            };
        }

        joint.connectedBody = null;
    }

    private void RestoreJointConnection(Joint joint)
    {
        if (joint == null) return;
        if (cachedJointConnections == null) return;
        if (!cachedJointConnections.TryGetValue(joint, out JointConnectionState state)) return;

        joint.autoConfigureConnectedAnchor = state.AutoConfigureConnectedAnchor;
        joint.connectedBody = state.ConnectedBody;

        if (!state.AutoConfigureConnectedAnchor)
        {
            joint.connectedAnchor = state.ConnectedAnchor;
        }
    }

    private void SetAliveState()
    {
        if (aliveColliders != null)
        {
            foreach (Collider col in aliveColliders)
            {
                if (col != null) col.enabled = true;
            }
        }

        if (ragdollColliders != null)
        {
            foreach (Collider col in ragdollColliders)
            {
                if (col != null) col.enabled = enableRagdollCollidersWhileAlive;
            }
        }

        if (disableRagdollJointsWhileAlive && ragdollJoints != null)
        {
            foreach (Joint joint in ragdollJoints)
            {
                if (joint != null) SetJointDisconnected(joint);
            }
        }

        if (bones != null)
        {
            foreach (Rigidbody bone in bones)
            {
                if (bone == null) continue;
                bone.isKinematic = true;
                bone.detectCollisions = enableRagdollCollidersWhileAlive;
            }
        }
    }

    public void Death()
    {
        if (IsDead) return;
        IsDead = true;

        if (bones == null || bones.Length == 0)
        {
            InitializeRagdoll();
        }

        if (aliveColliders != null)
        {
            foreach (Collider col in aliveColliders)
            {
                if (col != null) col.enabled = false;
            }
        }

        if (ragdollColliders != null)
        {
            foreach (Collider col in ragdollColliders)
            {
                if (col != null) col.enabled = true;
            }
        }

        if (ragdollJoints != null)
        {
            foreach (Joint joint in ragdollJoints)
            {
                if (joint != null) RestoreJointConnection(joint);
            }
        }

        if (bones != null)
        {
            foreach (Rigidbody bone in bones)
            {
                if (bone == null) continue;
                bone.isKinematic = false;
                bone.detectCollisions = true;
            }
        }
    }

    private void CacheAliveColliders()
    {
        // Alive colliders are any colliders in the ragdollRoot hierarchy that are NOT part of the ragdoll bones.
        // Typically this is the single big capsule/box collider on the enemy root.
        if (ragdollRoot == null)
        {
            ragdollRoot = transform;
        }

        Collider[] all = ragdollRoot.GetComponentsInChildren<Collider>(true);
        List<Collider> result = new List<Collider>(all.Length);

        foreach (Collider col in all)
        {
            if (col == null) continue;

            Rigidbody attached = col.attachedRigidbody;
            bool isRagdollCollider = attached != null && boneSet != null && boneSet.Contains(attached);
            if (isRagdollCollider) continue;

            result.Add(col);
        }

        aliveColliders = result.ToArray();
    }
}
