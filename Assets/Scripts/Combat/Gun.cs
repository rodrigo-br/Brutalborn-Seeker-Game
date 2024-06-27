using Cinemachine;
using NaughtyAttributes;
using System;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public event Action OnShoot;
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
    private CinemachineImpulseSource _impulseSource;
    private ObjectPooling<Bullet> _bulletPool;
    private PlayerController _player;
    private Vector2 _direction = Vector2.right;
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
        if (_player.Input != Vector2.zero)
        {
            _direction = _player.Input;
            HandleSpriteFlip();
        }
    }

    private void OnEnable()
    {
        _player.Attack += Shoot;
        _player.AttackHeld += Shoot;
        OnShoot += BulletRent;
        OnShoot += FireAnimation;
        OnShoot += GunScreenShake;
    }

    private void OnDisable()
    {
        _player.Attack -= Shoot;
        _player.AttackHeld += Shoot;
        OnShoot -= BulletRent;
        OnShoot -= FireAnimation;
        OnShoot = GunScreenShake;
    }

    public void Shoot()
    {
        if (_currentShootCooldown <= 0)
        {
            OnShoot?.Invoke();
        }
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
}
