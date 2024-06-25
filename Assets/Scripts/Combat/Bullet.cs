using System;
using UnityEngine;

public class Bullet : MonoBehaviour, IPhysicsObject, IPoolable
{
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _maxBulletLifeTime = 5f;
    private Rigidbody2D _rigidbody2D;
    private float _currentBulletLifeTime;
    private Action<IPoolable> _disableCallback;
    private Vector2 _direction = Vector2.right;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        PhysicsSimulator.Instance.AddBullet(this);
    }

    private void OnDisable()
    {
        PhysicsSimulator.Instance.RemoveBullet(this);
    }

    public void TickFixedUpdate(float delta)
    {
        if (!this.enabled) return;

        _rigidbody2D.velocity = _moveSpeed * _direction;
    }

    public void TickUpdate(float delta, float time)
    {
        _currentBulletLifeTime += delta;
        if (_currentBulletLifeTime >= _maxBulletLifeTime)
        {
            _disableCallback?.Invoke(this);
        }
    }

    public void SetDisableCallbackAction(Action<IPoolable> callback)
    {
        _disableCallback = callback;
    }
}
