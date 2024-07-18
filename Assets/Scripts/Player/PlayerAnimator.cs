using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerAnimator : MonoBehaviour
{
    public static event Action OnJump1;
    public static event Action OnJump2;
    public static event Action OnDash;
    public static event Action<bool> OnJetpack;
    public static event Action OnFallingGround;
    [Header("References")]
    [SerializeField]
    private Animator _anim;

    [SerializeField] private GameObject _effectsParent;
    [SerializeField] private Transform _trailRenderer;
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField] private TrailRenderer _trail;


    [Header("Particles")][SerializeField] private ParticleSystem _jumpParticles;
    [SerializeField] private ParticleSystem _launchParticles;
    [SerializeField] private ParticleSystem _moveParticles;
    [SerializeField] private ParticleSystem _landParticles;
    [SerializeField] private ParticleSystem _doubleJumpParticles;
    [SerializeField] private ParticleSystem _dashParticles;
    [SerializeField] private ParticleSystem _dashRingParticles;
    [SerializeField] private Transform _dashRingTransform;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] _splats;
    [SerializeField] private AudioClip[] _slideClips;
    [SerializeField] private AudioClip _wallGrabClip;


    private AudioSource _source;
    private IPlayerController _player;
    private Vector2 _defaultSpriteSize;
    private GeneratedCharacterSize _character;
    private Vector3 _trailOffset;
    private Vector2 _trailVel;
    private CinemachineImpulseSource _impulseSource;
    private List<Vector2> _defaultSpriteChildTransforms = new();
    private List<Vector2> _flipedSpriteChildTransforms = new();
    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _player = GetComponentInParent<IPlayerController>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        _character = _player.Stats.CharacterSize.GenerateCharacterSize();
        _defaultSpriteSize = new Vector2(1, _character.Height);

        _trailOffset = _trailRenderer.localPosition;
        _trailRenderer.SetParent(null);
        _originalTrailTime = _trail.time;
        foreach (Transform child in _sprite.transform)
        {
            _defaultSpriteChildTransforms.Add(new Vector2(child.transform.localPosition.x, child.transform.localPosition.y));
            _flipedSpriteChildTransforms.Add(new Vector2(child.transform.localPosition.x * -1, child.transform.localPosition.y));
        }
    }

    private void OnEnable()
    {
        _player.Jumped += OnJumped;
        _player.GroundedChanged += OnGroundedChanged;
        _player.DashChanged += OnDashChanged;
        _player.WallGrabChanged += OnWallGrabChanged;
        _player.Repositioned += PlayerOnRepositioned;
        _player.ToggledPlayer += PlayerOnToggledPlayer;
        _player.OnJetpack += OnJetpacking;

        _moveParticles.Play();
    }

    private void OnDisable()
    {
        _player.Jumped -= OnJumped;
        _player.GroundedChanged -= OnGroundedChanged;
        _player.DashChanged -= OnDashChanged;
        _player.WallGrabChanged -= OnWallGrabChanged;
        _player.Repositioned -= PlayerOnRepositioned;
        _player.ToggledPlayer -= PlayerOnToggledPlayer;
        _player.OnJetpack -= OnJetpacking;

        _moveParticles.Stop();
    }

    private void Update()
    {
        if (_player == null) return;

        var xInput = _player.Input.x;

        SetParticleColor(-_player.Up, _moveParticles);

        HandleSpriteFlip(xInput);

        HandleIdleSpeed(xInput);

        HandleFreeLookSpeed(xInput);

        HandleCharacterTilt(xInput);

        HandleCrouching();

        HandleWallSlideEffects();
    }

    private void HandleFreeLookSpeed(float xInput)
    {
        bool target = false;
        if (_grounded)
        {
            if (xInput != 0)
            {
                target = true;
            }
        }
        _anim.SetBool(RunKey, target);
    }

    private void LateUpdate()
    {
        _trailRenderer.position = Vector2.SmoothDamp(_trailRenderer.position, transform.position + _trailOffset, ref _trailVel, 0.02f);
    }

    #region Squish

    [Header("Squish")][SerializeField] private ParticleSystem.MinMaxCurve _squishMinMaxX;
    [SerializeField] private ParticleSystem.MinMaxCurve _squishMinMaxY;
    [SerializeField] private float _minSquishForce = 6f;
    [SerializeField] private float _maxSquishForce = 30f;
    [SerializeField] private float _minSquishDuration = 0.1f;
    [SerializeField] private float _maxSquishDuration = 0.4f;
    private bool _isSquishing;

    private IEnumerator SquishPlayer(float force)
    {
        force = Mathf.Abs(force);
        if (force < _minSquishForce) yield break;
        _isSquishing = true;

        var elapsedTime = 0f;

        var point = Mathf.InverseLerp(_minSquishForce, _maxSquishForce, force);
        var duration = Mathf.Lerp(_minSquishDuration, _maxSquishDuration, point);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / duration;

            var squishFactorY = Mathf.Lerp(_squishMinMaxY.curveMax.Evaluate(t), _squishMinMaxY.curveMin.Evaluate(t), point);
            var squishFactorX = Mathf.Lerp(_squishMinMaxX.curveMax.Evaluate(t), _squishMinMaxX.curveMin.Evaluate(t), point);
            _sprite.size = new Vector3(_defaultSpriteSize.x * squishFactorX, _defaultSpriteSize.y * squishFactorY);
            yield return null;
        }

        _sprite.size = _defaultSpriteSize;
        _isSquishing = false;
    }

    private void CancelSquish()
    {
        _isSquishing = false;
        if (_squishRoutine != null) StopCoroutine(_squishRoutine);
    }

    #endregion

    #region Walls & Ladders

    [Header("Walls & Ladders")]
    [SerializeField]
    private ParticleSystem _wallSlideParticles;

    [SerializeField] private AudioSource _wallSlideSource;
    [SerializeField] private AudioClip[] _wallClimbClips;
    [SerializeField] private AudioClip[] _ladderClimbClips;
    [SerializeField] private float _maxWallSlideVolume = 0.2f;
    [SerializeField] private float _wallSlideParticleOffset = 0.3f;
    [SerializeField] private float _distancePerClimbSound = 0.2f;

    private bool _isOnWall, _isSliding;
    private float _slideAudioVel;
    private bool _ascendingLadder;
    private float _lastClimbSoundY;
    private int _wallClimbAudioIndex = 0;
    private int _ladderClimbAudioIndex;

    private void OnWallGrabChanged(bool onWall)
    {
        _isOnWall = onWall;
        if (_isOnWall) PlaySound(_wallGrabClip, 0.5f);
    }

    private void HandleWallSlideEffects()
    {
        var slidingThisFrame = _isOnWall && !_grounded && _player.Velocity.y < 0;

        if (!_isSliding && slidingThisFrame)
        {
            _isSliding = true;
            _wallSlideParticles.Play();
        }
        else if (_isSliding && !slidingThisFrame)
        {
            _isSliding = false;
            _wallSlideParticles.Stop();
        }

        SetParticleColor(new Vector2(_player.WallDirection, 0), _wallSlideParticles);
        _wallSlideParticles.transform.localPosition = new Vector3(_wallSlideParticleOffset * _player.WallDirection, 0, 0);

        var requiredAudio = _isSliding || _player.ClimbingLadder && _player.Velocity.y < 0;
        var point = requiredAudio ? Mathf.InverseLerp(0, -_player.Stats.LadderSlideSpeed, _player.Velocity.y) : 0;
        _wallSlideSource.volume = Mathf.SmoothDamp(_wallSlideSource.volume, Mathf.Lerp(0, _maxWallSlideVolume, point), ref _slideAudioVel, 0.2f);

        bool isClimbingWall = false;
        bool isWallSlide = false;

        if ((_player.ClimbingLadder || _isOnWall) && _player.Velocity.y > 0)
        {
            if (!_ascendingLadder && _player.ClimbingLadder)
            {
                _ascendingLadder = true;
                _lastClimbSoundY = transform.position.y;
                Play();
            }
            else if (!_ascendingLadder && _isOnWall)
            {
                isClimbingWall = true;
            }

            if (transform.position.y >= _lastClimbSoundY + _distancePerClimbSound)
            {
                _lastClimbSoundY = transform.position.y;
                Play();
            }
            _anim.speed = 1;
        }
        else if ((_player.ClimbingLadder || _isOnWall) && _player.Velocity.y < 0)
        {
            if (!_ascendingLadder && !_isOnWall)
            {
                _ascendingLadder = true;
            }
            else if (!_ascendingLadder && _isOnWall)
            {
                isWallSlide = true;
            }
            _anim.speed = 1;
        }
        else if (_player.ClimbingLadder && _player.Velocity.y == 0)
        {
            _ascendingLadder = true;
            _anim.speed = 0;
        }
        else
        {
            _ascendingLadder = false;
            isClimbingWall = false;
            _anim.speed = 1;
        }
        _anim.SetBool(LadderClimbKey, _ascendingLadder);
        _anim.SetBool(WallClimbKey, isClimbingWall);
        _anim.SetBool(WallSlideKey, isWallSlide);

        void Play()
        {
            if (_isOnWall) PlayWallClimbSound();
            else PlayLadderClimbSound();
        }
    }

    private void PlayWallClimbSound()
    {
        _wallClimbAudioIndex = (_wallClimbAudioIndex + 1) % _wallClimbClips.Length;
        PlaySound(_wallClimbClips[_wallClimbAudioIndex], 0.1f);
    }

    private void PlayLadderClimbSound()
    {
        _ladderClimbAudioIndex = (_ladderClimbAudioIndex + 1) % _ladderClimbClips.Length;
        PlaySound(_ladderClimbClips[_ladderClimbAudioIndex], 0.07f);
    }

    #endregion

    #region Animation

    [Header("Idle")]
    [SerializeField, Range(1f, 3f)]
    private float _maxIdleSpeed = 2;

    // Speed up idle while running
    private void HandleIdleSpeed(float xInput)
    {
        var inputStrength = Mathf.Abs(xInput);
        //_anim.SetFloat(IdleSpeedKey, Mathf.Lerp(1, _maxIdleSpeed, inputStrength));
        _moveParticles.transform.localScale = Vector3.MoveTowards(_moveParticles.transform.localScale,
            Vector3.one * inputStrength, 2 * Time.deltaTime);
    }

    private void HandleSpriteFlip(float xInput)
    {
        bool flip = xInput < 0;
        int index = 0;
        if (_player.Input.x != 0)
        {
            _sprite.flipX = flip;
            List<Vector2> sprites = flip ? _flipedSpriteChildTransforms : _defaultSpriteChildTransforms;
            foreach (Transform child in _sprite.transform)
            {
                child.transform.localPosition = sprites[index];
                index++;
            }
        }
    }

    #endregion

    #region Tilt

    [Header("Tilt")][SerializeField] private float _runningTilt = 5; // In degrees around the Z axis
    [SerializeField] private float _maxTilt = 10; // In degrees around the Z axis
    [SerializeField] private float _tiltSmoothTime = 0.1f;

    private Vector3 _currentTiltVelocity;

    private void HandleCharacterTilt(float xInput)
    {
        var runningTilt = _grounded ? Quaternion.Euler(0, 0, _runningTilt * xInput) : Quaternion.identity;
        var targetRot = _grounded && _player.GroundNormal != _player.Up ? runningTilt * _player.GroundNormal : runningTilt * _player.Up;

        // Calculate the smooth damp effect
        var smoothRot = Vector3.SmoothDamp(_anim.gameObject.transform.up, targetRot, ref _currentTiltVelocity, _tiltSmoothTime);

        if (Vector3.Angle(_player.Up, smoothRot) > _maxTilt)
        {
            smoothRot = Vector3.RotateTowards(_player.Up, smoothRot, Mathf.Deg2Rad * _maxTilt, 0f);
        }

        // Rotate towards the smoothed target
        _anim.gameObject.transform.up = smoothRot;
    }

    #endregion

    #region Crouch & Slide

    private bool _crouching;
    private Vector2 _currentCrouchSizeVelocity;

    private void HandleCrouching()
    {
        if (!_crouching && _player.Crouching)
        {
            _source.PlayOneShot(_slideClips[Random.Range(0, _slideClips.Length)], Mathf.InverseLerp(0, 5, Mathf.Abs(_player.Velocity.x)));
            _crouching = true;
            CancelSquish();
        }
        else if (_crouching && !_player.Crouching)
        {
            _crouching = false;
        }

        if (!_isSquishing)
        {
            var percentage = _character.CrouchingHeight / _character.Height;
            _sprite.size = Vector2.SmoothDamp(_sprite.size, new Vector2(1, _crouching ? _character.Height * percentage : _character.Height), ref _currentCrouchSizeVelocity, 0.03f);
        }

        _anim.SetBool(CrouchKey, _crouching);
    }

    #endregion

    #region Event Callbacks

    private bool _jumped = false;

    private void OnJumped(JumpType type)
    {
        _jumped = true;
        if (type is JumpType.Jump or JumpType.Coyote or JumpType.WallJump)
        {
            _anim.SetTrigger(JumpKey);
            _anim.SetBool(GroundedKey, false);
            OnJump1?.Invoke();

            // Only play particles when grounded (avoid coyote)
            if (type is JumpType.Jump)
            {
                SetColor(_jumpParticles);
                SetColor(_launchParticles);
                _jumpParticles.Play();
            }
        }
        else if (type is JumpType.AirJump)
        {
            _anim.SetTrigger(AirJumpKey);
            OnJump2?.Invoke();
            _doubleJumpParticles.Play();
        }
    }

    private bool _grounded;
    private Coroutine _squishRoutine;

    private void OnGroundedChanged(bool grounded, float impact)
    {
        _grounded = grounded;

        if (grounded)
        {
            _jumped = false;
            _anim.SetBool(GroundedKey, true);
            CancelSquish();
            _squishRoutine = StartCoroutine(SquishPlayer(Mathf.Abs(impact)));
            _source.PlayOneShot(_splats[Random.Range(0, _splats.Length)], 0.5f);
            _moveParticles.Play();

            //_landParticles.transform.localScale = Vector3.one * Mathf.InverseLerp(0, 40, impact);
            if (impact < -25)
            {
                SetColor(_landParticles);
                _impulseSource.GenerateImpulse();
                OnFallingGround?.Invoke();
                _landParticles.Play();
                _anim.SetTrigger(GroundSlamKey);
                _player.SetClearInputs(0.5f);
            }
        }
        else
        {
            _anim.SetBool(GroundedKey, false);
            _moveParticles.Stop();
            if (!_jumped)
            {
                _anim.SetTrigger(FallKey);
            }
        }
    }

    [SerializeField] private TrailRenderer[] _jetpackTrailRendereres;

    private void OnJetpacking(bool isJetpacking)
    {
        OnJetpack?.Invoke(isJetpacking);
        foreach (TrailRenderer trailRenderer in _jetpackTrailRendereres)
        {
            trailRenderer.emitting = isJetpacking;
        }
    }


    private void OnDashChanged(bool dashing, Vector2 dir)
    {
        if (dashing)
        {
            _dashParticles.Play();
            _dashRingTransform.up = dir;
            _dashRingParticles.Play();
            _anim.SetTrigger(DashKey);
            OnDash?.Invoke();
        }
        else
        {
            _dashParticles.Stop();
        }
    }

    #endregion

    private float _originalTrailTime;
    private void PlayerOnRepositioned(Vector2 newPosition)
    {
        StartCoroutine(ResetTrail());

        IEnumerator ResetTrail()
        {
            _trail.time = 0;
            yield return new WaitForSeconds(0.1f);
            _trail.time = _originalTrailTime;
        }
    }

    private void PlayerOnToggledPlayer(bool on)
    {
        _effectsParent.SetActive(on);
    }

    #region Helpers

    private ParticleSystem.MinMaxGradient _currentGradient;

    private void SetParticleColor(Vector2 detectionDir, ParticleSystem system)
    {
        var ray = Physics2D.Raycast(transform.position, detectionDir, 2);
        if (!ray) return;

        _currentGradient = ray.transform.TryGetComponent(out SpriteRenderer r)
            ? new ParticleSystem.MinMaxGradient(r.color * 0.9f, r.color * 1.2f)
            : new ParticleSystem.MinMaxGradient(Color.white);

        SetColor(system);
    }

    private void SetColor(ParticleSystem ps)
    {
        var main = ps.main;
        main.startColor = _currentGradient;
    }

    private void PlayRandomSound(IReadOnlyList<AudioClip> clips, float volume = 1, float pitch = 1)
    {
        PlaySound(clips[Random.Range(0, clips.Count)], volume, pitch);
    }

    private void PlaySound(AudioClip clip, float volume = 1, float pitch = 1)
    {
        _source.pitch = pitch;
        _source.PlayOneShot(clip, volume);
    }

    #endregion

    #region Animation Keys

    private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
    private static readonly int JumpKey = Animator.StringToHash("Jump");
    private static readonly int RunKey = Animator.StringToHash("Run");
    private static readonly int DashKey = Animator.StringToHash("Dash");
    private static readonly int AirJumpKey = Animator.StringToHash("AirJump");
    private static readonly int FallKey = Animator.StringToHash("Fall");
    private static readonly int CrouchKey = Animator.StringToHash("Crouch");
    private static readonly int GroundSlamKey = Animator.StringToHash("GroundSlam");
    private static readonly int LadderClimbKey = Animator.StringToHash("LadderClimb");
    private static readonly int WallClimbKey = Animator.StringToHash("WallClimb");
    private static readonly int WallSlideKey = Animator.StringToHash("WallSlide");

    #endregion
}