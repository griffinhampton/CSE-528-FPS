using UnityEngine;
using System.Collections.Generic;

public class LiveAndLetDie : MonoBehaviour
{
    [Header("Ragdoll")]
    [Tooltip("Optional: assign the root transform of the ragdoll (Armature). If null, uses this GameObject.")]
    [SerializeField] private Transform ragdollRoot;

    private Rigidbody[] bones;
    private Collider[] ragdollColliders;
    private HashSet<Rigidbody> boneSet;

    private void Awake()
    {
        InitializeRagdoll();
        SetAliveState();
    }

    private void InitializeRagdoll()
    {
        if (ragdollRoot == null)
        {
            ragdollRoot = transform;
        }

        bones = ragdollRoot.GetComponentsInChildren<Rigidbody>(true);
        boneSet = new HashSet<Rigidbody>(bones);

        List<Collider> boneColliders = new List<Collider>();
        Collider[] allChildColliders = ragdollRoot.GetComponentsInChildren<Collider>(true);
        foreach (Collider col in allChildColliders)
        {
            Rigidbody attached = col.attachedRigidbody;
            if (attached != null && boneSet.Contains(attached))
            {
                boneColliders.Add(col);
            }
        }
        ragdollColliders = boneColliders.ToArray();
    }

    private void SetAliveState()
    {
        if (ragdollColliders != null)
        {
            foreach (Collider col in ragdollColliders)
            {
                if (col != null) col.enabled = false;
            }
        }

        if (bones != null)
        {
            foreach (Rigidbody bone in bones)
            {
                if (bone == null) continue;
                bone.isKinematic = true;
                bone.detectCollisions = false;
            }
        }
    }

    public void Death()
    {
        if (bones == null || bones.Length == 0)
        {
            InitializeRagdoll();
        }

        if (ragdollColliders != null)
        {
            foreach (Collider col in ragdollColliders)
            {
                if (col != null) col.enabled = true;
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
}
