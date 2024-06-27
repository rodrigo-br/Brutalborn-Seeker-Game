using NaughtyAttributes;
using UnityEngine;

public class Gun : MonoBehaviour
{
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
    private ObjectPooling<Bullet> _bulletPool;
    private PlayerController _player;
    private Vector2 _direction = Vector2.right;

    private void Awake()
    {
        _bulletPool = new("Prefabs/Combat/" + _bulletPrefab.ToString(), _initialPoolSize);
        _player = GetComponentInParent<PlayerController>();
    }

    private void Update()
    {
        if (_player.Input != Vector2.zero)
        {
            _direction = _player.Input;
            HandleSpriteFlip();
        }
    }

    private void OnEnable()
    {
        _player.Attack += Shoot;
    }

    private void OnDisable()
    {
        _player.Attack -= Shoot;
    }

    public void Shoot()
    {
        BulletRent();
    }

    private void BulletRent()
    {
        Bullet bullet = _bulletPool.Rent();
        bullet.Init(_bulletSpawnPosition.position, _bulletSpawnPosition.rotation, _direction);
    }

    public void SetObjectPooling(ObjectPooling<Bullet> objectPooling)
    {
        _bulletPool = objectPooling;
    }

    private void HandleSpriteFlip()
    {
        Debug.Log(_direction);
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
}
