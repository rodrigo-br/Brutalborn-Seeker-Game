using UnityEngine;

public class EnemySeekingState : EnemyBaseState
{
    private Vector2 _pathDirection;
    private readonly Gun _gun;
    private float _lastSearchForAimTime;
    private readonly float _searchForAimCooldown = 2f;
    private Vector2 _targetDirection;
    private bool _jump;
    private bool _dash;
    private bool _canDash = false;
    private readonly float _dashDelay = 1f;
    private float _timeSinceAirJump;
    private bool _changingState;

    public EnemySeekingState(
        EnemyStateMachine stateMachine,
        EnemyInput enemyInput,
        PlayerController enemyController,
        EnemyAI enemyAI,
        Gun gun)
        : base(stateMachine, enemyInput, enemyController, enemyAI)
    {
        _gun = gun;
    }

    public override void Enter()
    {
        Debug.Log("Enter Seeking State");
        enemyInput.SetNewInputFrame();
        _lastSearchForAimTime = 0;
        enemyController.Jumped += SetCanDash;
        _timeSinceAirJump = 0;
    }

    public override void Tick(float deltaTime)
    {
        if (_changingState) { return; }
        _pathDirection = enemyAI.Direction;
        
        if (_pathDirection.y >= 0.9f)
        {
            _jump = true;
        }
        if (_canDash)
        {
            _timeSinceAirJump += deltaTime;
            if (_timeSinceAirJump >= _dashDelay)
            {
                _dash = true;
                _timeSinceAirJump = 0;
                _canDash = false;
            }
        }
        enemyInput.SetNewInputFrame(move: _pathDirection, jumpDown: _jump, jumpHeld: _jump, dashDown: _dash);
        _jump = false;
        _dash = false;
        _lastSearchForAimTime += deltaTime;
        if (_lastSearchForAimTime < _searchForAimCooldown) { return; }
        _lastSearchForAimTime = 0;
        _targetDirection = enemyAI.Target.position - enemyController.transform.position;
        if (stateMachine.CanShootTarget(_targetDirection))
        {
            stateMachine.EnterShootingState();
            _changingState = true;
        }
        float distanceFromTarget = Vector2.Distance(enemyAI.Target.position, enemyController.transform.position);
        if (distanceFromTarget <= 12f && _gun.LastLobGrenadeTime <= 0)
        {
            stateMachine.EnterGrenadeState();
            _changingState = true;
        }
        else if (distanceFromTarget < 9f)
        {
            _lastSearchForAimTime = _searchForAimCooldown;
        }
    }

    public override void Exit()
    {
        enemyController.Jumped -= SetCanDash;
    }

    private void SetCanDash(JumpType type)
    {
        if (type == JumpType.AirJump)
        {
            _timeSinceAirJump = 0;
            _canDash = true;
        }
    }
}
