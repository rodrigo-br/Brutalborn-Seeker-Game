using NaughtyAttributes;
using System;
using UnityEngine;

public class Bullet : MonoBehaviour, IPhysicsObject, IPoolable
{
    [MinMaxSlider(0.0f, 100.0f)]
    [SerializeField] private Vector2 _bulletSpeedAcceleration = new Vector2(5f, 50f);
    [SerializeField] private bool _hasAcceleration;
    [ShowIf("_hasAcceleration")]
    [SerializeField] private float _maxAccelerationSpeedAt = 3f;
    [ShowIf("_hasAcceleration")]
    [SerializeField] private bool _isAccelerationReversed = false;
    [SerializeField] private float _maxBulletLifeTime = 5f;
    [SerializeField] private int _damageAmount = 10;
    [SerializeField] private float _knockBackThrust = 10f;
    private Rigidbody2D _rigidbody2D;
    private float _currentBulletLifeTime;
    private float _currentMoveSpeed;
    private Action<IPoolable> _disableCallback;
    private Vector2 _direction = Vector2.right;
    private float _accelerationStartTime;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();

        // REMOVER ME
        _disableCallback = (call) =>
        {
            this.gameObject.SetActive(false);
        };
    }

    private void OnEnable()
    {
        if (_hasAcceleration)
        {
            if (_isAccelerationReversed)
            {
                float tempX = _bulletSpeedAcceleration.x;
                _bulletSpeedAcceleration.x = _bulletSpeedAcceleration.y;
                _bulletSpeedAcceleration.y = tempX;
            }
            _currentMoveSpeed = _bulletSpeedAcceleration.x;
            _accelerationStartTime = Time.time;
        }
        else
        {
            _currentMoveSpeed = _bulletSpeedAcceleration.y;
        }
        _currentBulletLifeTime = 0f;
        PhysicsSimulator.Instance.AddBullet(this);    }

    private void OnDisable()
    {
        PhysicsSimulator.Instance.RemoveBullet(this);
    }

    public void TickFixedUpdate(float delta)
    {
        if (_hasAcceleration)
        {
            UpdateAcceleration(delta);
        }
        _rigidbody2D.velocity = _currentMoveSpeed * _direction;
    }

    public void TickUpdate(float delta, float time)
    {
        _currentBulletLifeTime += delta;
        if (_currentBulletLifeTime >= _maxBulletLifeTime)
        {
            _disableCallback?.Invoke(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other);
        // Instantiate BulletVFX

        IHitable iHitable = other.GetComponent<IHitable>();
        iHitable?.TakeHit(_direction, _knockBackThrust);

        IDamageable iDamageable = other.GetComponent<IDamageable>();
        iDamageable?.TakeDamage(_damageAmount);

        _disableCallback?.Invoke(this);
    }

    public void Init(Vector2 position, Quaternion rotation, Vector2 direction)
    {
        transform.SetPositionAndRotation(position, rotation);
        _direction = direction;
        gameObject.SetActive(true);
    }

    private void UpdateAcceleration(float delta)
    {
        float elapsedTime = _accelerationStartTime + delta;
        if (elapsedTime <= _maxAccelerationSpeedAt)
        {
            float t = elapsedTime / _maxAccelerationSpeedAt;
            _currentMoveSpeed = Mathf.Lerp(_currentMoveSpeed, _bulletSpeedAcceleration.y, t);
        }
        else
        {
            _currentMoveSpeed = _bulletSpeedAcceleration.y;
            _hasAcceleration = false;
        }
    }

    public void SetDisableCallbackAction(Action<IPoolable> callback)
    {
        _disableCallback = callback;
    }
}
