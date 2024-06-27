using NaughtyAttributes;
using System;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable, IHealable
{
    public Action<Health> OnHealthChange;
    public Action<Health> OnDeath;
    [field: SerializeField] public int MaxHealth { get; private set; } = 100;
    [SerializeField] private GameObject _splatterPrefab;
    [SerializeField] private GameObject _deathVFX;
    public int CurrentHealth { get; private set; }
    private readonly int minMaxHealth = 10;
    private Knockback _knockback;
    private Flash _flash;

    private void Awake()
    {
        _knockback = GetComponent<Knockback>();
        _flash = GetComponent<Flash>();
    }

    private void Update()
    {
        Debug.Log(transform.position);
    }

    private void Start()
    {
        ResetHealth();
    }

    private void OnEnable()
    {
        OnDeath += SpawnDeathVFX;
        OnDeath += SpawnDeathSplatterPrefab;
    }

    private void OnDisable()
    {
        OnDeath -= SpawnDeathVFX;
        OnDeath -= SpawnDeathSplatterPrefab;
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

    private void SpawnDeathSplatterPrefab(Health health)
    {
        if (health != this) { return; }
        GameObject newSplatterPrefab = Instantiate(_splatterPrefab, transform.position, transform.rotation);
        SpriteRenderer spriteRenderer = newSplatterPrefab.GetComponent<SpriteRenderer>();
        spriteRenderer.color = RandomizeColor(spriteRenderer.color);
    }

    private Color RandomizeColor(Color color)
    {
        float randomR = UnityEngine.Random.Range(color.r / 2, color.r);
        float randomG = UnityEngine.Random.Range(color.g / 2, color.g);
        float randomB = UnityEngine.Random.Range(color.b / 2, color.b);
        return new Color(randomR, randomG, randomB);
    }

    private void SpawnDeathVFX(Health health)
    {
        if (health != this) { return; }
        GameObject deathVFX = Instantiate(_deathVFX, transform.position, transform.rotation);
        ParticleSystem.MainModule ps = deathVFX.GetComponent<ParticleSystem>().main;
        ps.startColor = RandomizeColor(ps.startColor.color);
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
