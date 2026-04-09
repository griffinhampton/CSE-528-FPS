using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Archetype")]
    [Tooltip("If true, this pig will pick a random archetype on spawn (Awake).")]
    [SerializeField] private bool randomizeArchetypeOnSpawn = true;

    [Tooltip("Chance that a pig becomes an emotive archetype (Angry/Sad/Happy). 0.2 = 1/5.")]
    [Range(0f, 1f)]
    [SerializeField] private float emotiveArchetypeChance = 0.2f;

    [Tooltip("Used when Randomize Archetype On Spawn is disabled.")]
    [SerializeField] private PigArchetype archetype = PigArchetype.Neutral;

    [Tooltip("Archetypes that can be selected when randomizing.")]
    [SerializeField] private PigArchetype[] randomArchetypes = new PigArchetype[]
    {
        PigArchetype.Angry,
        PigArchetype.Sad,
        PigArchetype.Happy,
    };

    [Tooltip("Tints the pig by setting renderer color via MaterialPropertyBlock (does not modify shared materials).")]
    [SerializeField] private bool tintRenderersByArchetype = true;

    [Tooltip("If enabled, applies emission color/intensity per archetype. For a visible 'glow halo', enable Bloom in your URP post-processing.")]
    [SerializeField] private bool glowRenderersByArchetype = true;

    [Tooltip("Global multiplier for archetype emission intensity.")]
    [Min(0f)]
    [SerializeField] private float glowIntensityMultiplier = 1f;

    [Tooltip("Optional explicit renderer list. If empty and Tint Renderers By Archetype is enabled, this auto-finds child renderers.")]
    [SerializeField] private Renderer[] renderersToTint;

    [Header("Scoring")]
    [Tooltip("Base points awarded when this pig dies.")]
    [Min(0)]
    [SerializeField] private int baseKillPoints = 10;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float turnSpeedDegreesPerSecond = 540f;

    [Header("Stability")]
    [Tooltip("Keeps the enemy upright by freezing pitch/roll (X/Z rotation) while still allowing yaw (Y rotation) for lock-on turning.")]
    [SerializeField] private bool keepUpright = true;

    [Header("Vision")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float visionDistance = 15f;
    [SerializeField] private float visionSphereRadius = 0.5f;
    [SerializeField] private float visionOriginHeight = 1.0f;
    [SerializeField] private LayerMask visionMask = ~0;

    [Header("Chase")]
    [SerializeField] private bool latchOnFirstSight = true;

    [Header("Spawn Path")]
    [Tooltip("If assigned (via EnemyManager/EnemyPath), the enemy will follow these waypoints in order until it enters the arena.")]
    [SerializeField] private Transform[] spawnPathWaypoints;
    [SerializeField] private float waypointReachDistance = 0.5f;

    [Header("Pre-Arena Search")]
    [Tooltip("If true, rotates the enemy by 90 degrees around local Y every 5 seconds while not in the arena.")]
    [SerializeField] private bool rotateEveryFiveSecondsPreArena = true;

    [Header("Arena")]
    [Tooltip("When the enemy enters a trigger with this tag, it switches to locked-on arena mode.")]
    [SerializeField] private string arenaTag = "Arena";

    [Tooltip("Optional: center of the arena. If assigned, enemies will steer toward it while leaving cubbies before entering the arena.")]
    [SerializeField] private Transform arenaCenter;

    private Transform target;
    private bool hasSeenPlayer;
    private Rigidbody rb;
    private LiveAndLetDie liveAndLetDie;

    private bool warnedMissingPlayer;

    private bool awardedKillScore;

	private int currentWaypointIndex;
	private bool isInArena;
    private float nextPreArenaRotateTime;

	private Quaternion desiredRotation;
	private bool hasDesiredRotation;

    private float baseMoveSpeed;
    private float baseTurnSpeedDegreesPerSecond;
    private float baseVisionDistance;
    private bool baseLatchOnFirstSight;
    private bool baseRotateEveryFiveSecondsPreArena;

    private PigArchetypeTuning activeArchetypeTuning;

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");
    private MaterialPropertyBlock propertyBlock;

    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");
    private static readonly int EmissiveColorId = Shader.PropertyToID("_EmissiveColor");
    private static readonly string EmissionKeyword = "_EMISSION";

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Unity does not allow creating some engine objects in static initializers.
        // This must be created during instance lifetime (Awake/Start).
        propertyBlock = new MaterialPropertyBlock();

        CacheBaseTuningValues();

        if (randomizeArchetypeOnSpawn)
        {
            // 1/5 pigs are emotive; the rest are Neutral.
            if (Random.value < emotiveArchetypeChance)
            {
                archetype = PigArchetypeDefaults.PickRandomFrom(randomArchetypes, fallback: PigArchetype.Neutral);
                if (archetype == PigArchetype.Neutral)
                {
                    // If a neutral value sneaks into the array, treat it as non-emotive.
                    archetype = PigArchetype.Neutral;
                }
            }
            else
            {
                archetype = PigArchetype.Neutral;
            }
        }

        ApplyArchetype(archetype);

        if (keepUpright && rb != null)
        {
            // Allow yaw for turning, but prevent pitch/roll which can cause nosedives.
            RigidbodyConstraints c = rb.constraints;
            c &= ~RigidbodyConstraints.FreezeRotationY;
            c |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.constraints = c;
        }

		nextPreArenaRotateTime = Time.time + 5f;

        liveAndLetDie = GetComponent<LiveAndLetDie>();
        if (liveAndLetDie == null) liveAndLetDie = GetComponentInChildren<LiveAndLetDie>(true);
        if (liveAndLetDie == null) liveAndLetDie = GetComponentInParent<LiveAndLetDie>();

        awardedKillScore = false;
    }

    private void CacheBaseTuningValues()
    {
        // These are used as the baseline so designers can still tune the prefab per-enemy.
        baseMoveSpeed = moveSpeed;
        baseTurnSpeedDegreesPerSecond = turnSpeedDegreesPerSecond;
        baseVisionDistance = visionDistance;
        baseLatchOnFirstSight = latchOnFirstSight;
        baseRotateEveryFiveSecondsPreArena = rotateEveryFiveSecondsPreArena;
    }

    public void SetArchetype(PigArchetype newArchetype)
    {
        archetype = newArchetype;
        ApplyArchetype(archetype);
    }

    private void ApplyArchetype(PigArchetype newArchetype)
    {
        activeArchetypeTuning = PigArchetypeDefaults.Get(newArchetype);

        moveSpeed = baseMoveSpeed * activeArchetypeTuning.moveSpeedMultiplier;
        turnSpeedDegreesPerSecond = baseTurnSpeedDegreesPerSecond * activeArchetypeTuning.turnSpeedMultiplier;
        visionDistance = baseVisionDistance * activeArchetypeTuning.visionDistanceMultiplier;

        // Archetypes can also flip behavior flags.
        latchOnFirstSight = activeArchetypeTuning.latchOnFirstSight;
        rotateEveryFiveSecondsPreArena = activeArchetypeTuning.rotateEveryFiveSecondsPreArena;

        if (tintRenderersByArchetype)
        {
            EnsureRenderersToTint();
            TintAllRenderers(activeArchetypeTuning.tintColor);
        }

        if (glowRenderersByArchetype)
        {
            EnsureRenderersToTint();
            ApplyGlowAllRenderers(activeArchetypeTuning.emissionColor, activeArchetypeTuning.emissionIntensity * glowIntensityMultiplier);
        }
    }

    private void EnsureRenderersToTint()
    {
        if (renderersToTint != null && renderersToTint.Length > 0) return;
        renderersToTint = GetComponentsInChildren<Renderer>(true);
    }

    private void TintAllRenderers(Color tint)
    {
        if (renderersToTint == null) return;

        for (int i = 0; i < renderersToTint.Length; i++)
        {
            Renderer r = renderersToTint[i];
            if (r == null) continue;
            ApplyTintToRenderer(r, tint);
        }
    }

    private void ApplyTintToRenderer(Renderer renderer, Color tint)
    {
        // Use property blocks so we do not mutate shared materials or create material instances.
        Material[] mats = renderer.sharedMaterials;
        int matCount = mats != null ? mats.Length : 0;

        if (matCount <= 1)
        {
            renderer.GetPropertyBlock(propertyBlock);
            SetColorOnBlockForMaterial(mats != null && mats.Length > 0 ? mats[0] : null, propertyBlock, tint);
            renderer.SetPropertyBlock(propertyBlock);
            return;
        }

        for (int i = 0; i < matCount; i++)
        {
            renderer.GetPropertyBlock(propertyBlock, i);
            SetColorOnBlockForMaterial(mats[i], propertyBlock, tint);
            renderer.SetPropertyBlock(propertyBlock, i);
        }
    }

    private void ApplyGlowAllRenderers(Color emissionColor, float intensity)
    {
        if (renderersToTint == null) return;
        if (intensity <= 0f) return;

        // Emission requires material keywords; to avoid affecting all pigs, we intentionally
        // work on instance materials (Renderer.materials) rather than shared materials.
        Color emission = emissionColor * Mathf.Max(0f, intensity);

        for (int i = 0; i < renderersToTint.Length; i++)
        {
            Renderer r = renderersToTint[i];
            if (r == null) continue;
            ApplyGlowToRendererInstanceMaterials(r, emission);
        }
    }

    private void ApplyGlowToRendererInstanceMaterials(Renderer renderer, Color emission)
    {
        Material[] mats = renderer.materials;
        if (mats == null || mats.Length == 0) return;

        for (int i = 0; i < mats.Length; i++)
        {
            Material mat = mats[i];
            if (mat == null) continue;

            bool setAny = false;

            if (mat.HasProperty(EmissionColorId))
            {
                mat.SetColor(EmissionColorId, emission);
                setAny = true;
            }

            if (mat.HasProperty(EmissiveColorId))
            {
                mat.SetColor(EmissiveColorId, emission);
                setAny = true;
            }

            if (setAny)
            {
                mat.EnableKeyword(EmissionKeyword);
            }
        }

        renderer.materials = mats;
    }

    private void SetColorOnBlockForMaterial(Material mat, MaterialPropertyBlock block, Color tint)
    {
        if (mat != null)
        {
            if (mat.HasProperty(BaseColorId))
            {
                block.SetColor(BaseColorId, tint);
                return;
            }

            if (mat.HasProperty(ColorId))
            {
                block.SetColor(ColorId, tint);
                return;
            }
        }

        // Fallback: set both common IDs.
        block.SetColor(BaseColorId, tint);
        block.SetColor(ColorId, tint);
    }

    public void SetSpawnPath(Transform[] waypoints)
    {
        spawnPathWaypoints = waypoints;
        currentWaypointIndex = 0;
    }

    public void SetArenaCenter(Transform center)
    {
        arenaCenter = center;
    }

    private void Update()
    {
        TryAwardKillScoreIfDead();

        if (liveAndLetDie != null && liveAndLetDie.IsDead)
        {
            return;
        }

        // Only acquire/lock-on once in the arena.
        if (isInArena)
        {
            if (!hasSeenPlayer || !latchOnFirstSight)
            {
                TryAcquirePlayerByVision();
            }
        }

        // Movement is applied in FixedUpdate (physics-friendly). If there's no Rigidbody, FixedUpdate
        // still runs and we will fall back to Transform movement.
    }

    private void FixedUpdate()
    {
        TryAwardKillScoreIfDead();

        if (liveAndLetDie != null && liveAndLetDie.IsDead)
        {
            SetStoppedMotion();
            return;
        }

        if (isInArena)
        {
            // Keep trying to acquire the player once we're in the arena.
            if (!hasSeenPlayer || target == null)
            {
                TryAcquirePlayerByVision();
            }

            // Arena mode: rotate continuously to face the player.
            if (hasSeenPlayer)
            {
                if (target != null)
                {
                    TurnTowards(target.position, Time.fixedDeltaTime);
                }
            }
        }
        else
        {
            // Pre-arena mode: follow the spawn path (leave cubby), then keep walking forward.
            FollowSpawnPathIfAny(Time.fixedDeltaTime);

            // Angry pigs will start hunting as soon as they can see the player, even before the arena.
            if (archetype == PigArchetype.Angry)
            {
                if (!hasSeenPlayer || target == null)
                {
                    TryAcquirePlayerByVision();
                }
                if (hasSeenPlayer && target != null)
                {
                    TurnTowards(target.position, Time.fixedDeltaTime);
                }
            }

            // If there is no path (or it is finished), steer toward the arena center to reduce "stuck in cubby" spawns.
            if ((spawnPathWaypoints == null || currentWaypointIndex >= (spawnPathWaypoints?.Length ?? 0)) && arenaCenter != null)
            {
                TurnTowards(arenaCenter.position, Time.fixedDeltaTime);
            }

            if (rotateEveryFiveSecondsPreArena && Time.time >= nextPreArenaRotateTime)
            {
                desiredRotation = transform.rotation * Quaternion.Euler(0f, 90f, 0f);
                hasDesiredRotation = true;
                nextPreArenaRotateTime = Time.time + 5f;
            }
        }

        ApplyDesiredRotationIfAny();
        MoveForward();
    }

    private void FollowSpawnPathIfAny(float deltaTime)
    {
        if (spawnPathWaypoints == null || spawnPathWaypoints.Length == 0) return;

        // Advance past null or reached waypoints.
        int n = spawnPathWaypoints.Length;
        while (currentWaypointIndex < n && spawnPathWaypoints[currentWaypointIndex] == null)
        {
            currentWaypointIndex++;
        }

        if (currentWaypointIndex >= n) return;

        Transform wp = spawnPathWaypoints[currentWaypointIndex];
        Vector3 toWp = wp.position - transform.position;
        toWp.y = 0f;
        float reach = Mathf.Max(0.01f, waypointReachDistance);
        if (toWp.sqrMagnitude <= reach * reach)
        {
            currentWaypointIndex++;
            return;
        }

        // Rotate towards waypoint; movement stays "forward".
        TurnTowards(wp.position, deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (!string.IsNullOrWhiteSpace(arenaTag) && other.CompareTag(arenaTag))
        {
            isInArena = true;
			nextPreArenaRotateTime = float.PositiveInfinity;
        }
    }

    private void TryAcquirePlayerByVision()
    {
        // First, try a direct acquire against the player by tag.
        // The original forward-cast only works if the enemy is already facing the player.
        if (target == null)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(playerTag))
                {
                    GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
                    if (playerObj != null) target = playerObj.transform;
                }
            }
            catch (UnityException)
            {
                // Tag not defined in the project.
            }

            // In arena mode, a direct tag match is sufficient to "lock on".
            if (isInArena && target != null)
            {
                hasSeenPlayer = true;
                return;
            }

            if (target == null && !warnedMissingPlayer)
            {
                warnedMissingPlayer = true;
                Debug.LogWarning($"EnemyAI: Could not find player with tag '{playerTag}'. The enemy will not lock on until a tagged object exists.", this);
            }
        }

        Vector3 origin = transform.position + Vector3.up * Mathf.Max(0f, visionOriginHeight);
        Vector3 direction = transform.forward;

        if (target != null)
        {
            Vector3 toTarget = target.position - origin;
            float distanceToTarget = toTarget.magnitude;

            if (distanceToTarget > 0.001f && distanceToTarget <= Mathf.Max(0f, visionDistance))
            {
                Vector3 dir = toTarget / distanceToTarget;

                // RaycastAll so we can ignore our own colliders and pick the first real obstruction.
                RaycastHit[] hits = Physics.RaycastAll(
                    origin,
                    dir,
                    distanceToTarget,
                    visionMask,
                    QueryTriggerInteraction.Ignore);

                float closest = float.PositiveInfinity;
                RaycastHit closestHit = default;
                bool found = false;

                for (int i = 0; i < hits.Length; i++)
                {
                    RaycastHit h = hits[i];
                    if (h.collider == null) continue;

                    // Ignore self.
                    if (h.collider.transform == transform || h.collider.transform.IsChildOf(transform))
                    {
                        continue;
                    }

                    if (h.distance < closest)
                    {
                        closest = h.distance;
                        closestHit = h;
                        found = true;
                    }
                }

                if (!found || (closestHit.transform != null && (closestHit.transform == target || closestHit.transform.root == target)))
                {
                    hasSeenPlayer = true;
                    return;
                }
            }
        }

        RaycastHit hit;
        bool hitSomething;

        if (visionSphereRadius > 0f)
        {
            hitSomething = Physics.SphereCast(
                origin,
                visionSphereRadius,
                direction,
                out hit,
                Mathf.Max(0f, visionDistance),
                visionMask,
                QueryTriggerInteraction.Ignore);
        }
        else
        {
            hitSomething = Physics.Raycast(
                origin,
                direction,
                out hit,
                Mathf.Max(0f, visionDistance),
                visionMask,
                QueryTriggerInteraction.Ignore);
        }

        if (!hitSomething) return;

        Transform hitTransform = hit.transform;
        if (hitTransform == null) return;

        if (hitTransform.CompareTag(playerTag) || hitTransform.root.CompareTag(playerTag))
        {
            target = hitTransform.CompareTag(playerTag) ? hitTransform : hitTransform.root;
            hasSeenPlayer = true;
        }
    }

    private void TurnTowards(Vector3 worldPosition, float deltaTime)
    {
        Vector3 toTarget = worldPosition - transform.position;
        toTarget.y = 0f;
        if (toTarget.sqrMagnitude < 0.0001f) return;

        Quaternion desired = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        desiredRotation = Quaternion.RotateTowards(
            transform.rotation,
            desired,
            turnSpeedDegreesPerSecond * Mathf.Max(0f, deltaTime));
        hasDesiredRotation = true;
    }

    private void MoveForward()
    {
        float speed = moveSpeed;
        if (archetype != PigArchetype.Neutral && hasSeenPlayer)
        {
            speed *= activeArchetypeTuning.seenPlayerSpeedMultiplier;
        }
        if (speed == 0f) return;

        // If we have a dynamic Rigidbody, drive horizontal velocity so movement works
        // even when collisions/physics are involved.
        if (rb != null && rb.isKinematic == false)
        {
            bool freezeX = (rb.constraints & RigidbodyConstraints.FreezePositionX) != 0;
            bool freezeZ = (rb.constraints & RigidbodyConstraints.FreezePositionZ) != 0;
            if (!freezeX || !freezeZ)
            {
                Vector3 current = rb.linearVelocity;
                Vector3 desiredHoriz = transform.forward * speed;
                if (!freezeX) current.x = desiredHoriz.x;
                if (!freezeZ) current.z = desiredHoriz.z;
                rb.linearVelocity = current;
                return;
            }
        }

        // Fallback for kinematic Rigidbody / fully constrained bodies / no Rigidbody.
        transform.position += transform.forward * (speed * Time.fixedDeltaTime);
    }

    private void ApplyDesiredRotationIfAny()
    {
        if (!hasDesiredRotation) return;

        if (rb != null && rb.isKinematic == false)
        {
            rb.MoveRotation(desiredRotation);
        }
        else
        {
            transform.rotation = desiredRotation;
        }

        hasDesiredRotation = false;
    }

    private void SetStoppedMotion()
    {
        if (rb != null && rb.isKinematic == false)
        {
            Vector3 v = rb.linearVelocity;
            v.x = 0f;
            v.z = 0f;
            rb.linearVelocity = v;
        }
    }

    private void TryAwardKillScoreIfDead()
    {
        if (awardedKillScore) return;
        if (liveAndLetDie == null) return;
        if (!liveAndLetDie.IsDead) return;

        awardedKillScore = true;

        float mult = activeArchetypeTuning.scoreMultiplier;
        if (mult <= 0f) mult = 1f;

        PlayerScore.AddScoreWithMultiplier(Mathf.Max(0, baseKillPoints), mult);
    }
}
