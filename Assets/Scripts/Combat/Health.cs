using NaughtyAttributes;
using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable, IHealable
{
    public static Action<Health> OnHealthChange;
    public static Action<Health> OnDeath;
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;
    [field: SerializeField] public GameObject SplatterPrefab { get; private set; }
    [field: SerializeField] public GameObject DeathVFX { get; private set; }
    public int CurrentHealth { get; private set; }
    private readonly int minMaxHealth = 10;
    private Knockback _knockback;
    private Flash _flash;

    private void Awake()
    {
        _knockback = GetComponent<Knockback>();
        _flash = GetComponent<Flash>();
    }

    private void Start()
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        ChangeHealthByAmount(MaxHealth);
    }

    private void ChangeHealthByAmount(int amount)
    {
        CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, MaxHealth);
        OnHealthChange?.Invoke(this);
    }

    private void ChangeMaxHealthByAmount(int amount)
    {
        MaxHealth = Mathf.Clamp(MaxHealth + amount, minMaxHealth, int.MaxValue);
    }

    public void IncreaseMaxHealth(int amount)
    {
        ChangeMaxHealthByAmount(amount);
        ChangeHealthByAmount(amount);
    }

    public void DecreaseMaxHealth(int amount)
    {
        ChangeMaxHealthByAmount(-amount);
        if (this.CurrentHealth > minMaxHealth)
        {
            ChangeHealthByAmount(-amount);
        }
        else
        {
            OnHealthChange?.Invoke(this);
        }
    }

    public void TakeDamage(int amount)
    {
        ChangeHealthByAmount(-amount);
        _flash.StartFlash();
        if (CurrentHealth <= 0)
        {
            OnDeath?.Invoke(this);
            Destroy(gameObject.transform.parent.parent.gameObject);
        }
    }

    public void TakeHit(Vector2 damageSourceDirection, float knockBackThrust)
    {
        _knockback.GetKnockedBack(damageSourceDirection, knockBackThrust);
    }

    public void Heal(int amount)
    {
        ChangeHealthByAmount(amount);
    }

    [Button]
    private void DebugTakeDamage()
    {
        TakeDamage(10);
    }

    [Button]
    private void DebugHeal()
    {
        Heal(10);
    }

    [Button]
    private void DebugResetHealth()
    {
        ResetHealth();
    }

    [Button]
    private void DebugIncreaseMaxHealth()
    {
        IncreaseMaxHealth(10);
    }

    [Button]
    private void DebugDecreaseMaxHealth()
    {
        DecreaseMaxHealth(10);
    }
}

public interface IDamageable : IHitable
{
    void TakeDamage(int amount);
}

public interface IHealable
{
    void Heal(int amount);
}

public interface IHitable
{
    void TakeHit(Vector2 damageSourceDirection, float knockBackThrust);
}
