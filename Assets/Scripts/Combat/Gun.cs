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
    private ObjectPooling<Bullet> _bulletPool;
    private IPlayerController _player;
    private Vector2 _direction = Vector2.right;

    private void Awake()
    {
        _bulletPool = new("Prefabs/Combat/" + _bulletPrefab.ToString(), _initialPoolSize);
        _player = GetComponentInParent<IPlayerController>();
    }

    private void Update()
    {
        if (_player.Input != Vector2.zero)
        {
            _direction = _player.Input;
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

    [Button]
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
}
