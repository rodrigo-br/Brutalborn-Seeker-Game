using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour, IPhysicsObject
{

    // References
    private BoxCollider2D _collider;
    private CapsuleCollider2D _airborneCollider;
    private ConstantForce2D _constantForce;
    private Rigidbody2D _rb;
    private PlayerInput _playerInput;

    // Interface
    [field: SerializeField] public PlayerStats Stats { get; private set; }
    [SerializeField] private bool _drawGizmos = true;
    public Vector2 Up { get; private set; }
    public Vector2 Right { get; private set; }
    public Vector2 Input => _frameInput.Move;
    public Vector2 GroundNormal { get; private set; }
    public Vector2 Velocity { get; private set; }

    // Loop
    private float _delta, _time;

    // Setup
    private GeneratedCharacterSize _character;
    private bool _cachedQueryMode, _cachedQueryTriggers;
    private const float GRAVITY_SCALE = 1;

    // Input
    private FrameInput _frameInput;

    // Frame Data
    private Bounds _wallDetectionBounds;
    private Vector2 _framePosition;
    private bool _hasInputThisFrame;
    private Vector2 _trimmedFrameVelocity;

    // Direction
    private Vector2 _frameDirection;

    // Move
    private Vector2 _frameTransientVelocity;
    private Vector2 _immediateMove;
    private Vector2 _decayingTransientVelocity;
    private Vector2 _totalTransientVelocityAppliedLastFrame;
    private Vector2 _frameSpeedModifier, _currentFrameSpeedModifier = Vector2.one;
    private const float SLOPE_ANGLE_FOR_EXACT_MOVEMENT = 0.7f;
    private IPhysicsMover _lastPlatform;
    private float _lastFrameY;

    // Collisions
    private const float SKIN_WIDTH = 0.02f;
    private Vector2 RayPoint => _framePosition + Up * (_character.StepHeight + SKIN_WIDTH);
    private const int RAY_SIDE_COUNT = 5;
    private RaycastHit2D _groundHit;
    private bool _grounded;
    private float _currentStepDownLength;
    private float GrounderLength => _character.StepHeight + SKIN_WIDTH;

    // Jump
    private bool _jumpToConsume;
    private float _timeJumpWasPressed;

    // Dash
    private bool _dashToConsume;

    #region Loop
    private void Awake()
    {
        if (!TryGetComponent(out _playerInput)) _playerInput = gameObject.AddComponent<PlayerInput>();
        if (!TryGetComponent(out _constantForce)) _constantForce = gameObject.AddComponent<ConstantForce2D>();

        PhysicsSimulator.Instance.AddPlayer(this);
    }

    public void TickFixedUpdate(float delta)
    {
        SetFrameData();

        CalculateCollisions();
        CalculateDirection();

        Move();
    }

    public void TickUpdate(float delta, float time)
    {
        _delta = delta;
        _time = time;

        GatherInput();
    }

    public void OnValidate()
    {
        SetupCharacter();
    }

    private void OnDestroy()
    {
        PhysicsSimulator.Instance.RemovePlayer(this);
    }

    #endregion

    #region Interface

    //public void AddFrameForce(Vector2 force, bool resetVelocity = false)
    //{
    //    if (resetVelocity) SetVelocity(Vector2.zero);
    //    _forceToApplyThisFrame += force;
    //}

    #endregion

    #region Setup

    private void SetupCharacter()
    {
        _character = Stats.CharacterSize.GenerateCharacterSize();
        _cachedQueryMode = Physics2D.queriesStartInColliders;

        _wallDetectionBounds = new Bounds(
            new Vector3(0, _character.Height / 2),
        new Vector3(_character.StandingColliderSize.x + CharacterSize.COLLIDER_EDGE_RADIUS * 2 + Stats.WallDetectorRange, _character.Height - 0.1f));

        _rb = GetComponent<Rigidbody2D>();

        // Primary collider
        _collider = GetComponent<BoxCollider2D>();
        _collider.edgeRadius = CharacterSize.COLLIDER_EDGE_RADIUS;
        _collider.sharedMaterial = _rb.sharedMaterial;
        _collider.enabled = true;

        // Airborne collider
        _airborneCollider = GetComponent<CapsuleCollider2D>();
        _airborneCollider.size = new Vector2(_character.Width - SKIN_WIDTH * 2, _character.Height - SKIN_WIDTH * 2);
        _airborneCollider.offset = new Vector2(0, _character.Height / 2);
        _airborneCollider.sharedMaterial = _rb.sharedMaterial;

        SetColliderMode(ColliderMode.Airborne);
    }

    #endregion

    #region Input

    private void GatherInput()
    {
        _frameInput = _playerInput.GatherInput();

        if (_frameInput.JumpDown)
        {
            _jumpToConsume = true;
            _timeJumpWasPressed = _time;
        }

        if (_frameInput.DashDown)
        {
            _dashToConsume = true;
        }
    }

    #endregion

    #region FrameData

    private void SetFrameData()
    {
        var rot = _rb.rotation * Mathf.Deg2Rad;
        Up = new Vector2(-Mathf.Sin(rot), Mathf.Cos(rot));
        Right = new Vector2(Up.y, -Up.x);
        _framePosition = _rb.position;

        _hasInputThisFrame = _frameInput.Move.x != 0;

        Velocity = _rb.velocity;
        _trimmedFrameVelocity = new Vector2(Velocity.x, 0);
    }

    #endregion

    #region Direction

    private void CalculateDirection()
    {
        _frameDirection = new Vector2(_frameInput.Move.x, 0);

        if (_grounded)
        {
            GroundNormal = _groundHit.normal;
            var angle = Vector2.Angle(GroundNormal, Up);
            if (angle < Stats.MaxWalkableSlope) _frameDirection.y = _frameDirection.x * -GroundNormal.x / GroundNormal.y;
        }

        _frameDirection = _frameDirection.normalized;
        Debug.Log(_frameDirection);
    }

    #endregion

    #region Move

    private void Move()
    {
        var targetSpeed = _hasInputThisFrame ? Stats.BaseSpeed : 0;

        var step = _hasInputThisFrame ? Stats.Acceleration : Stats.Friction;

        var xDir = (_hasInputThisFrame ? _frameDirection : Velocity.normalized);

        Vector2 newVelocity;
        step *= _delta;

        if (_grounded)
        {
            var speed = Mathf.MoveTowards(Velocity.magnitude, targetSpeed, step);

            // Blend the two approaches
            var targetVelocity = xDir * speed;

            // Calculate the new speed based on the current and target speeds
            var newSpeed = Mathf.MoveTowards(Velocity.magnitude, targetVelocity.magnitude, step);

            // TODO: Lets actually trace the ground direction automatically instead of direct
            var smoothed = Vector2.MoveTowards(Velocity, targetVelocity, step); // Smooth but potentially inaccurate
            var direct = targetVelocity.normalized * newSpeed; // Accurate but abrupt
            var slopePoint = Mathf.InverseLerp(0, SLOPE_ANGLE_FOR_EXACT_MOVEMENT, Mathf.Abs(_frameDirection.y)); // Blend factor

            // Calculate the blended velocity
            newVelocity = Vector2.Lerp(smoothed, direct, slopePoint);
        }
        else { newVelocity = Vector2.zero; }

        SetVelocity((newVelocity + AdditionalFrameVelocities()) * _currentFrameSpeedModifier);

        
    }

    private Vector2 AdditionalFrameVelocities()
    {
        if (_immediateMove.sqrMagnitude > SKIN_WIDTH)
        {
            _rb.MovePosition(_framePosition + _immediateMove);
        }

        _totalTransientVelocityAppliedLastFrame = _frameTransientVelocity + _decayingTransientVelocity;
        return _totalTransientVelocityAppliedLastFrame;
    }

    private void SetVelocity(Vector2 newVel)
    {
        _rb.velocity = newVel;
        Velocity = newVel;
    }

    #endregion

    #region Collisions

    private void CalculateCollisions()
    {
        Physics2D.queriesStartInColliders = false;

        // Is the middle ray good?
        var isGroundedThisFrame = PerformRay(RayPoint);

        // If not, zigzag rays from the center outward until we find a hit
        if (!isGroundedThisFrame)
        {
            foreach (var offset in GenerateRayOffsets())
            {
                isGroundedThisFrame = PerformRay(RayPoint + Right * offset) || PerformRay(RayPoint - Right * offset);
                if (isGroundedThisFrame) break;
            }
        }

        if (isGroundedThisFrame && !_grounded) ToggleGrounded(true);
        else if (!isGroundedThisFrame && _grounded) ToggleGrounded(false);

        Physics2D.queriesStartInColliders = _cachedQueryMode;

        bool PerformRay(Vector2 point)
        {
            _groundHit = Physics2D.Raycast(point, -Up, GrounderLength + _currentStepDownLength, Stats.CollisionLayers);
            if (!_groundHit) return false;

            if (Vector2.Angle(_groundHit.normal, Up) > Stats.MaxWalkableSlope)
            {
                return false;
            }

            return true;
        }
    }

    private void ToggleGrounded(bool grounded)
    {
        _grounded = grounded;
        if (grounded)
        {
            _rb.gravityScale = 0;
            SetVelocity(_trimmedFrameVelocity);
            _constantForce.force = Vector2.zero;
            _currentStepDownLength = _character.StepHeight;
            SetColliderMode(ColliderMode.Standard);
        }
    }

    private void SetColliderMode(ColliderMode mode)
    {
        _airborneCollider.enabled = mode == ColliderMode.Airborne;

        switch (mode)
        {
            case ColliderMode.Standard:
                _collider.size = _character.StandingColliderSize;
                _collider.offset = _character.StandingColliderCenter;
                break;
            case ColliderMode.Crouching:
                _collider.size = _character.CrouchColliderSize;
                _collider.offset = _character.CrouchingColliderCenter;
                break;
            case ColliderMode.Airborne:
                break;
        }
    }

    private enum ColliderMode
    {
        Standard,
        Crouching,
        Airborne
    }

    private IEnumerable<float> GenerateRayOffsets()
    {
        var extent = _character.StandingColliderSize.x / 2 - _character.RayInset;
        var offsetAmount = extent / RAY_SIDE_COUNT;
        for (var i = 1; i < RAY_SIDE_COUNT + 1; i++)
        {
            yield return offsetAmount * i;
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmos()
    {
        if (!_drawGizmos) return;

        var pos = (Vector2)transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(pos + Vector2.up * _character.Height / 2, new Vector3(_character.Width, _character.Height));
        Gizmos.color = Color.magenta;

        var rayStart = pos + Vector2.up * _character.StepHeight;
        var rayDir = Vector3.down * _character.StepHeight;
        Gizmos.DrawRay(rayStart, rayDir);
        foreach (var offset in GenerateRayOffsets())
        {
            Gizmos.DrawRay(rayStart + Vector2.right * offset, rayDir);
            Gizmos.DrawRay(rayStart + Vector2.left * offset, rayDir);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(pos + (Vector2)_wallDetectionBounds.center, _wallDetectionBounds.size);


        Gizmos.color = Color.black;
        Gizmos.DrawRay(RayPoint, Vector3.right);
    }
    #endregion

}
