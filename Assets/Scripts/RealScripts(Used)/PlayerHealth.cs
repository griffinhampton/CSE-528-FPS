using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    [Serializable]
    public struct HealthChange
    {
        public int oldHealth;
        public int newHealth;
        public int delta;
        public int maxHealth;
        public GameObject source;

        public HealthChange(int oldHealth, int newHealth, int delta, int maxHealth, GameObject source)
        {
            this.oldHealth = oldHealth;
            this.newHealth = newHealth;
            this.delta = delta;
            this.maxHealth = maxHealth;
            this.source = source;
        }
    }

    public static event Action<HealthChange> HealthChanged;
    public static event Action Died;

    [Tooltip("Maximum health for this player.")]
    [Min(1)]
    [SerializeField] private int maxHealth = 5;

    [Tooltip("Current health. If <= 0 at start, it will be set to Max Health.")]
    [SerializeField] private int currentHealth = 0;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsDead => currentHealth <= 0;

    public static PlayerHealth Instance { get; private set; }

    private static bool deathAllowed = true;

    private static bool fallbackInitialized;
    private static bool fallbackDirty;
    private static int fallbackMaxHealth;
    private static int fallbackHealth;

    public static bool IsDeathAllowed()
    {
        return deathAllowed;
    }

    public static void SetDeathAllowed(bool allowed)
    {
        deathAllowed = allowed;
    }

    private static void EnsureFallbackInitialized()
    {
        if (fallbackInitialized) return;
        fallbackInitialized = true;
        fallbackMaxHealth = 5;
        fallbackHealth = 5;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Keep the first instance.
            return;
        }

        Instance = this;

        EnsureFallbackInitialized();

        maxHealth = Mathf.Max(1, maxHealth);

        if (currentHealth <= 0)
        {
            currentHealth = maxHealth;
        }
        else
        {
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        }

        if (fallbackDirty)
        {
            currentHealth = Mathf.Clamp(fallbackHealth, 0, maxHealth);
            fallbackDirty = false;
        }

        // Emit an initial update so UI can paint immediately on enable.
        Notify(new HealthChange(
            oldHealth: currentHealth,
            newHealth: currentHealth,
            delta: 0,
            maxHealth: maxHealth,
            source: gameObject));
    }

    public void Damage(int amount, GameObject source = null)
    {
        amount = Mathf.Max(0, amount);
        if (amount == 0) return;

        int old = currentHealth;
        bool wasDead = old <= 0;
        int next = old - amount;
        if (next == old) return;

        currentHealth = next;

        Notify(new HealthChange(
            oldHealth: old,
            newHealth: currentHealth,
            delta: currentHealth - old,
            maxHealth: maxHealth,
            source: source));

        if (deathAllowed && !wasDead && currentHealth <= 0)
        {
            try
            {
                Died?.Invoke();
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogException(ex);
#endif
            }
        }
    }

    public void Heal(int amount, GameObject source = null)
    {
        amount = Mathf.Max(0, amount);
        if (amount == 0) return;

        int old = currentHealth;
        int next = Mathf.Min(maxHealth, old + amount);
        if (next == old) return;

        currentHealth = next;

        Notify(new HealthChange(
            oldHealth: old,
            newHealth: currentHealth,
            delta: currentHealth - old,
            maxHealth: maxHealth,
            source: source));
    }

    public void ResetToMax()
    {
        int old = currentHealth;
        currentHealth = Mathf.Clamp(maxHealth, 0, maxHealth);

        Notify(new HealthChange(
            oldHealth: old,
            newHealth: currentHealth,
            delta: currentHealth - old,
            maxHealth: maxHealth,
            source: gameObject));
    }

    public static int GetHealth()
    {
        EnsureFallbackInitialized();
        return Instance != null ? Instance.CurrentHealth : fallbackHealth;
    }

    public static int GetMaxHealth()
    {
        EnsureFallbackInitialized();
        return Instance != null ? Instance.MaxHealth : fallbackMaxHealth;
    }

    public static void DamagePlayer(int amount, GameObject source = null)
    {
        amount = Mathf.Max(0, amount);
        if (amount == 0) return;

        if (Instance != null)
        {
            Instance.Damage(amount, source);
            return;
        }

        EnsureFallbackInitialized();
        fallbackDirty = true;

        int old = fallbackHealth;
        bool wasDead = old <= 0;
        fallbackHealth = old - amount;

        Notify(new HealthChange(
            oldHealth: old,
            newHealth: fallbackHealth,
            delta: fallbackHealth - old,
            maxHealth: fallbackMaxHealth,
            source: source));

        if (deathAllowed && !wasDead && fallbackHealth <= 0)
        {
            try
            {
                Died?.Invoke();
            }
            catch (Exception ex)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogException(ex);
#endif
            }
        }
    }

    public static void HealPlayer(int amount, GameObject source = null)
    {
        amount = Mathf.Max(0, amount);
        if (amount == 0) return;

        if (Instance != null)
        {
            Instance.Heal(amount, source);
            return;
        }

        EnsureFallbackInitialized();
        fallbackDirty = true;

        int old = fallbackHealth;
        fallbackHealth = Mathf.Min(fallbackMaxHealth, fallbackHealth + amount);

        Notify(new HealthChange(
            oldHealth: old,
            newHealth: fallbackHealth,
            delta: fallbackHealth - old,
            maxHealth: fallbackMaxHealth,
            source: source));
    }

    public static void ResetHealthToMax(GameObject source = null)
    {
        if (Instance != null)
        {
            Instance.ResetToMax();
            return;
        }

        EnsureFallbackInitialized();
        fallbackDirty = true;

        int old = fallbackHealth;
        fallbackHealth = Mathf.Max(0, fallbackMaxHealth);

        Notify(new HealthChange(
            oldHealth: old,
            newHealth: fallbackHealth,
            delta: fallbackHealth - old,
            maxHealth: fallbackMaxHealth,
            source: source));
    }

    private static void Notify(HealthChange change)
    {
        try
        {
            HealthChanged?.Invoke(change);
        }
        catch (Exception ex)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogException(ex);
#endif
        }
    }
}
