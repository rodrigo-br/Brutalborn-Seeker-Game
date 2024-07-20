using UnityEngine;

public class Turrent : MonoBehaviour
{
    [SerializeField] private Transform _lowLevelAnimations;
    [SerializeField] private float animationSpeedVariation = 0.2f;
    private Health _health;
    private Animator[] _lowLevelAnimators;

    private void Awake()
    {
        _health = GetComponentInChildren<Health>();
        _lowLevelAnimators = _lowLevelAnimations.GetComponentsInChildren<Animator>();
    }

    private void OnEnable()
    {
        Health.OnHealthChange += CheckChangeAnimation;
    }

    private void OnDisable()
    {
        Health.OnHealthChange -= CheckChangeAnimation;
    }

    private void CheckChangeAnimation(Health health)
    {
        if (health != _health || _lowLevelAnimations.gameObject.activeSelf) { return; }

        Debug.Log(_health.CurrentHealth);
        Debug.Log(_health.MaxHealth / 3f);

        if (_health.CurrentHealth <= _health.MaxHealth / 3f)
        {
            _lowLevelAnimations.gameObject.SetActive(true);
            foreach (Animator animator in _lowLevelAnimators)
            {
                animator.speed = Random.Range(animator.speed - animationSpeedVariation, animator.speed + animationSpeedVariation);
            }
        }
    }
}
