using System;
using UnityEngine;

public enum PigArchetype
{
    Neutral,
    Angry,
    Sad,
    Happy,
}

[Serializable]
public struct PigArchetypeTuning
{
    public PigArchetype archetype;
    public Color tintColor;

    [Header("Glow")]
    [Tooltip("If intensity is > 0 and the material supports emission, the pig will glow this color.")]
    public Color emissionColor;
    [Min(0f)] public float emissionIntensity;

    [Header("Scoring")]
    [Min(0f)] public float scoreMultiplier;

    [Min(0f)] public float moveSpeedMultiplier;
    [Min(0f)] public float turnSpeedMultiplier;
    [Min(0f)] public float visionDistanceMultiplier;

    [Tooltip("Additional multiplier applied only after the pig has seen the player.")]
    [Min(0f)] public float seenPlayerSpeedMultiplier;

    public bool latchOnFirstSight;
    public bool rotateEveryFiveSecondsPreArena;

    public static PigArchetypeTuning Create(
        PigArchetype archetype,
        Color tintColor,
        Color emissionColor,
        float emissionIntensity,
        float scoreMultiplier,
        float moveSpeedMultiplier,
        float turnSpeedMultiplier,
        float visionDistanceMultiplier,
        float seenPlayerSpeedMultiplier,
        bool latchOnFirstSight,
        bool rotateEveryFiveSecondsPreArena)
    {
        return new PigArchetypeTuning
        {
            archetype = archetype,
            tintColor = tintColor,
            emissionColor = emissionColor,
            emissionIntensity = Mathf.Max(0f, emissionIntensity),
            scoreMultiplier = Mathf.Max(0f, scoreMultiplier),
            moveSpeedMultiplier = Mathf.Max(0f, moveSpeedMultiplier),
            turnSpeedMultiplier = Mathf.Max(0f, turnSpeedMultiplier),
            visionDistanceMultiplier = Mathf.Max(0f, visionDistanceMultiplier),
            seenPlayerSpeedMultiplier = Mathf.Max(0f, seenPlayerSpeedMultiplier),
            latchOnFirstSight = latchOnFirstSight,
            rotateEveryFiveSecondsPreArena = rotateEveryFiveSecondsPreArena,
        };
    }
}

public static class PigArchetypeDefaults
{
    public static PigArchetypeTuning Get(PigArchetype archetype)
    {
        switch (archetype)
        {
            case PigArchetype.Angry:
                // Faster, sees farther, locks on instantly.
                return PigArchetypeTuning.Create(
                    archetype: PigArchetype.Angry,
                    tintColor: new Color(0.85f, 0.25f, 0.22f, 1f),
                    emissionColor: new Color(0.85f, 0.25f, 0.22f, 1f),
                    emissionIntensity: 2.25f,
                    scoreMultiplier: 2.0f,
                    moveSpeedMultiplier: 1.35f,
                    turnSpeedMultiplier: 1.20f,
                    visionDistanceMultiplier: 1.30f,
                    seenPlayerSpeedMultiplier: 1.15f,
                    latchOnFirstSight: true,
                    rotateEveryFiveSecondsPreArena: false);

            case PigArchetype.Sad:
                // Slower, shorter vision, reluctant to fully lock-on.
                return PigArchetypeTuning.Create(
                    archetype: PigArchetype.Sad,
                    tintColor: new Color(0.25f, 0.45f, 0.85f, 1f),
                    emissionColor: new Color(0.25f, 0.45f, 0.85f, 1f),
                    emissionIntensity: 1.25f,
                    scoreMultiplier: 1.25f,
                    moveSpeedMultiplier: 0.75f,
                    turnSpeedMultiplier: 0.85f,
                    visionDistanceMultiplier: 0.80f,
                    seenPlayerSpeedMultiplier: 0.90f,
                    latchOnFirstSight: false,
                    rotateEveryFiveSecondsPreArena: true);

            case PigArchetype.Happy:
                // Slightly quicker wandering/turning, but less aware.
                return PigArchetypeTuning.Create(
                    archetype: PigArchetype.Happy,
                    tintColor: new Color(0.95f, 0.85f, 0.20f, 1f),
                    emissionColor: new Color(0.95f, 0.85f, 0.20f, 1f),
                    emissionIntensity: 1.75f,
                    scoreMultiplier: 1.5f,
                    moveSpeedMultiplier: 1.05f,
                    turnSpeedMultiplier: 1.05f,
                    visionDistanceMultiplier: 0.90f,
                    seenPlayerSpeedMultiplier: 1.00f,
                    latchOnFirstSight: false,
                    rotateEveryFiveSecondsPreArena: true);

            default:
                return PigArchetypeTuning.Create(
                    archetype: PigArchetype.Neutral,
                    tintColor: new Color(1f, 1f, 1f, 1f),
                    emissionColor: new Color(1f, 1f, 1f, 1f),
                    emissionIntensity: 0f,
                    scoreMultiplier: 1.0f,
                    moveSpeedMultiplier: 1.00f,
                    turnSpeedMultiplier: 1.00f,
                    visionDistanceMultiplier: 1.00f,
                    seenPlayerSpeedMultiplier: 1.00f,
                    latchOnFirstSight: true,
                    rotateEveryFiveSecondsPreArena: true);
        }
    }

    public static PigArchetype PickRandomFrom(PigArchetype[] candidates, PigArchetype fallback = PigArchetype.Neutral)
    {
        if (candidates == null || candidates.Length == 0) return fallback;

        // Avoid returning an invalid enum value if the array was edited badly in the inspector.
        for (int guard = 0; guard < 8; guard++)
        {
            PigArchetype pick = candidates[UnityEngine.Random.Range(0, candidates.Length)];
            if (Enum.IsDefined(typeof(PigArchetype), pick))
            {
                return pick;
            }
        }

        return fallback;
    }
}
