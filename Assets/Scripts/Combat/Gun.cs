using Cinemachine;
using System;
using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public event Action OnShoot;
    public event Action OnLobGrenade;
    private enum PrefabPath
    {
        Bullet,
        PlayerBullet,
        EnemyBullet,
        TurrentBullet
    }
    [field: SerializeField] public Transform BulletSpawnPosition { get; private set; }
    [SerializeField] private PrefabPath _bulletPrefab;
    [SerializeField] private int _initialPoolSize = 10;
    [field: SerializeField] public float ShootCooldown { get; private set; } = 0.3f;
    [SerializeField] private Animator _animator;
    [SerializeField] private Vector2 _impulseVelocity;
    [SerializeField] private GameObject _muzzleFlash;
    [SerializeField] private float _muzzleFlashTime = 0.05f;
    [SerializeField] private Grenade _grenadePrefab;
    [SerializeField] private float _lobGranadeCooldown = 1.5f;
    [SerializeField] private float _grenadeArcVelocity = 0.05f;
    [SerializeField] private LaunchArcRenderer _launchArcRenderer;
    [SerializeField] private bool _autoSetDirection = false;
    private IShootStrategy _shootStrategy;
    private IShootStrategy[] _shootStrategies;
    private CinemachineImpulseSource _impulseSource;
    private ObjectPooling<Bullet> _bulletPool;
    private PlayerController _player;
    private Coroutine _muzzleFlashRoutine;
    private Vector2 _direction = Vector2.right;
    private float _lastXDirection = 0;
    public float LastLobGrenadeTime { get; private set; } = 0f;
    private float _currentShootCooldown;
    private static readonly int FIRE_HASH = Animator.StringToHash("Fire");

    private void Awake()
    {
        _bulletPool = new("Prefabs/Combat/" + _bulletPrefab.ToString(), _initialPoolSize);
        _player = GetComponentInParent<PlayerController>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _shootStrategies = new IShootStrategy[]
        {
            new SingleConeShoot(this)
        };
        _shootStrategy = _shootStrategies[0];
    }

    private void Update()
    {
        _currentShootCooldown -= Time.deltaTime;
        LastLobGrenadeTime -= Time.deltaTime;
        if (_player == null) { return; }
        if (_player.Input != Vector2.zero)
        {
            SetDirection(_player.Input);
        }

        if (_player.HeldGrenade && LastLobGrenadeTime <= 0)
        {
            if (!_launchArcRenderer.gameObject.activeSelf)
            {
                _launchArcRenderer.gameObject.SetActive(true);
            }
            _launchArcRenderer.IncrementVelocity(_grenadeArcVelocity, _lastXDirection);
            if (_launchArcRenderer.Fire)
            {
                OnLobGrenade?.Invoke();
                _launchArcRenderer.ResetVelocity();
            }
        }

        if (_player.LobGrenade && LastLobGrenadeTime <= 0)
        {
            OnLobGrenade?.Invoke();
            _launchArcRenderer.ResetVelocity();
        }
    }

    private void OnEnable()
    {
        if (_player != null)
        {
            _player.Attack += _shootStrategy.Use;
            _player.AttackHeld += _shootStrategy.Use;
        }
        OnShoot += BulletRent;
        OnShoot += FireAnimation;
        OnShoot += GunScreenShake;
        OnShoot += MuzzleFlash;
        OnLobGrenade += LobGrenade;
        OnLobGrenade += FireAnimation;
    }

    private void OnDisable()
    {
        if (_player != null)
        {
            _player.Attack -= _shootStrategy.Use;
            _player.AttackHeld -= _shootStrategy.Use;
        }
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

    public void LobGrenade()
    {
        LastLobGrenadeTime = _lobGranadeCooldown;
        Grenade newGrenade = Instantiate(_grenadePrefab);
        newGrenade.Init(
            BulletSpawnPosition.position,
            _lastXDirection,
            this,
            _launchArcRenderer.CurrentAngle,
            _launchArcRenderer.CurrentVelocity);
    }

    public void GiveUpGrenade()
    {
        LastLobGrenadeTime = _lobGranadeCooldown;
        _launchArcRenderer.ResetVelocity();
    }

    private void BulletRent()
    {
        _currentShootCooldown = ShootCooldown;
        if (_autoSetDirection)
        {
            _direction = BulletSpawnPosition.right;
            _direction.x *= BulletSpawnPosition.localScale.x;
        }
        Bullet bullet = _bulletPool.Rent();
        bullet.Init(BulletSpawnPosition.position, BulletSpawnPosition.rotation, _direction);
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
            _lastXDirection = _direction.x;
            y = _direction.x > 0 ? 0: 180;
        }
        else
        {
            z *= 2;
            y = _lastXDirection > 0 ? 0 : 180;
        }
        transform.localRotation = Quaternion.Euler(0, y, z);
    }

    private void FireAnimation()
    {
        if (_animator == null) { return; }
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
        _impulseSource?.GenerateImpulse(impulseVelocity);
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

    public void SetDirection(Vector2 direction)
    {
        _direction = direction;
        HandleSpriteFlip();
    }

    public void SetShootCooldown(float shootCooldown)
    {
        ShootCooldown = shootCooldown;
    }
}
