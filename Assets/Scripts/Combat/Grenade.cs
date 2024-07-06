using Cinemachine;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public static event Action<SoundSO> OnGrenadeBeep;
    public static event Action<SoundSO> OnGrenadeExplode;
    public static event Action<SoundSO> OnGrenadeLaunch;

    [SerializeField] private GameObject _grenadeVFX;
    [SerializeField] private GameObject _grenadeLight;
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private int _damageAmount = 1;
    [SerializeField] private float _knockBackThrust = 20f;
    [SerializeField] private float _toqueAmount = 10f;
    [SerializeField] private float _explosionRadius = 3.5f;
    [SerializeField] private LayerMask _enemyLayerMask;
    [Layer]
    [SerializeField] private int _enemyLayerCollider;
    [SerializeField] private float _explosionTime = 4f;
    [Range(0, 3.5f)]
    [SerializeField] private float[] _beepTimes = { 1f, 0.5f, 0.5f };
    [SerializeField] private SoundSO _beepSound;
    [SerializeField] private SoundSO _explodeSound;
    [SerializeField] private SoundSO _launchSound;

    private Vector2 _fireDirection;

    private CinemachineImpulseSource _impulseSource;
    private Rigidbody2D _rigidBody;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_enemyLayerCollider == other.gameObject.layer)
        {
            ExplodeGranade();
        }
    }

    public void Init(Vector2 grenadeSpawnPos, Vector2 direction, Gun gun)
    {
        transform.position = grenadeSpawnPos;
        OnGrenadeLaunch?.Invoke(_launchSound);
        _fireDirection = direction;
        _rigidBody.AddForce(_fireDirection * _moveSpeed, ForceMode2D.Impulse);
        _rigidBody.AddTorque(_toqueAmount);
        StartCoroutine(BeepRoutine());
    }

    private void ExplodeGranade()
    {
        Instantiate(_grenadeVFX, transform.position, Quaternion.identity);
        OnGrenadeExplode?.Invoke(_explodeSound);
        _impulseSource.GenerateImpulse();
        DamageNearby();
        Destroy(gameObject);
    }

    private IEnumerator BeepRoutine()
    {
        foreach (float beepTime in _beepTimes)
        {
            yield return new WaitForSeconds(beepTime);
            OnGrenadeBeep?.Invoke(_beepSound);
            FlashGrenadeLight();
        }
        yield return new WaitForSeconds(_explosionTime - _beepTimes.Sum());
        ExplodeGranade();
    }

    private void FlashGrenadeLight()
    {
        StartCoroutine(FlashGrenadeLightRoutine());
    }

    private IEnumerator FlashGrenadeLightRoutine()
    {
        _grenadeLight.SetActive(true);
        yield return new WaitForSeconds(0.2f);
        _grenadeLight.SetActive(false);
    }

    private void DamageNearby()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _explosionRadius, _enemyLayerMask);
        
        foreach (Collider2D hit in hits)
        {
            IHitable iHitable = hit.gameObject.GetComponentInChildren<IHitable>();
            iHitable?.TakeHit(_fireDirection, _knockBackThrust);

            IDamageable iDamageable = hit.gameObject.GetComponentInChildren<IDamageable>();
            iDamageable?.TakeDamage(_damageAmount);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _explosionRadius);
    }
}
