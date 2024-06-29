using Cinemachine;
using System;
using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public event Action OnShoot;
    public static Action OnLobGrenade;
    private enum PrefabPath
    {
        Bullet,
        PlayerBullet,
        EnemyBullet
    }
    [SerializeField] private Transform _bulletSpawnPosition;
    [SerializeField] private PrefabPath _bulletPrefab;
    [SerializeField] private int _initialPoolSize = 10;
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private float _shootCooldown = 0.3f;
    [SerializeField] private Animator _animator;
    [SerializeField] private Vector2 _impulseVelocity;
    [SerializeField] private GameObject _muzzleFlash;
    [SerializeField] private float _muzzleFlashTime = 0.05f;
    [SerializeField] private Grenade _grenadePrefab;
    [SerializeField] private float _lobGranadeCooldown = 1.5f;
    private CinemachineImpulseSource _impulseSource;
    private ObjectPooling<Bullet> _bulletPool;
    private PlayerController _player;
    private Coroutine _muzzleFlashRoutine;
    private Vector2 _direction = Vector2.right;
    private float _lastLobGrenadeTime = 0f;
    private float _currentShootCooldown;
    private static readonly int FIRE_HASH = Animator.StringToHash("Fire");

    private void Awake()
    {
        _bulletPool = new("Prefabs/Combat/" + _bulletPrefab.ToString(), _initialPoolSize);
        _player = GetComponentInParent<PlayerController>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    private void Update()
    {
        _currentShootCooldown -= Time.deltaTime;
        _lastLobGrenadeTime -= Time.deltaTime;
        if (_player.Input != Vector2.zero)
        {
            _direction = _player.Input;
            HandleSpriteFlip();
        }

        if (_player.LobGrenade && _lastLobGrenadeTime <= 0)
        {
            OnLobGrenade?.Invoke();
        }
    }

    private void OnEnable()
    {
        _player.Attack += Shoot;
        _player.AttackHeld += Shoot;
        OnShoot += BulletRent;
        OnShoot += FireAnimation;
        OnShoot += GunScreenShake;
        OnShoot += MuzzleFlash;
        OnLobGrenade += LobGrenade;
        OnLobGrenade += FireAnimation;
    }

    private void OnDisable()
    {
        _player.Attack -= Shoot;
        _player.AttackHeld -= Shoot;
        OnShoot -= BulletRent;
        OnShoot -= FireAnimation;
        OnShoot -= GunScreenShake;
        OnShoot -= MuzzleFlash;
        OnLobGrenade -= LobGrenade;
        OnLobGrenade -= FireAnimation;
    }

    public void Shoot()
    {
        if (_currentShootCooldown <= 0)
        {
            OnShoot?.Invoke();
        }
    }

    private void LobGrenade()
    {
        _lastLobGrenadeTime = _lobGranadeCooldown;
        Grenade newGrenade = Instantiate(_grenadePrefab);
        newGrenade.Init(_bulletSpawnPosition.position, _direction, this);
    }

    private void BulletRent()
    {
        _currentShootCooldown = _shootCooldown;
        Bullet bullet = _bulletPool.Rent();
        bullet.Init(_bulletSpawnPosition.position, _bulletSpawnPosition.rotation, _direction);
    }

    public void SetObjectPooling(ObjectPooling<Bullet> objectPooling)
    {
        _bulletPool = objectPooling;
    }

    private void HandleSpriteFlip()
    {
        float y = 0;
        float z = 0;
        if (_direction.y > 0) z = 45;
        else if (_direction.y < 0) z = -45;
        if (_direction.x != 0)
        {
            y = _direction.x > 0 ? 0: 180;
        }
        else
        {
            z *= 2;
        }
        transform.localRotation = Quaternion.Euler(0, y, z);
    }

    private void FireAnimation()
    {
        _animator.Play(FIRE_HASH, 0, 0f);
    }

    private void GunScreenShake()
    {
        Vector2 impulseVelocity = _impulseVelocity;
        if (_direction.x < 0)
        {
            impulseVelocity *= -1;
        }
        else if (_direction.x == 0)
        {
            impulseVelocity = new Vector2(impulseVelocity.y, impulseVelocity.x);
            if (_direction.y < 0)
            {
                impulseVelocity *= -1;
            }
        }
        _impulseSource.GenerateImpulse(impulseVelocity);
    }

    private void MuzzleFlash()
    {
        if (_muzzleFlashRoutine != null)
        {
            StopCoroutine(_muzzleFlashRoutine);
        }
        
        _muzzleFlashRoutine = StartCoroutine(MuzzleFlashRoutine());
    }

    private IEnumerator MuzzleFlashRoutine()
    {
        _muzzleFlash.SetActive(true);
        yield return new WaitForSeconds(_muzzleFlashTime);
        _muzzleFlash.SetActive(false);
    }
}
