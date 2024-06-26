using NaughtyAttributes;
using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable, IHealable
{
    public Action<Health> OnHealthChange;
    public Action<Health> OnDeath;
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;
    public int CurrentHealth { get; private set; }
    private readonly int minMaxHealth = 10;
    private Knockback _knockback;

    private void Awake()
    {
        _knockback = GetComponent<Knockback>();
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
        if (CurrentHealth <= 0)
        {
            OnDeath?.Invoke(this);
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
