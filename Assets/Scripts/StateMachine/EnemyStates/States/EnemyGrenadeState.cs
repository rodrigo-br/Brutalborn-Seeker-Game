using UnityEngine;

public class EnemyGrenadeState : EnemyBaseState
{
    private readonly Gun _gun;
    private readonly LaunchArcRenderer _launcher;
    private readonly float _giveUpTime = 1f;
    private readonly float _chargeTime = 2f;
    private float _elapsedTime = 0f;
    private bool _launchDecided = false;
    private bool _changingState = false;

    public EnemyGrenadeState(
        EnemyStateMachine stateMachine,
        EnemyInput enemyInput,
        PlayerController enemyController,
        EnemyAI enemyAI,
        Gun gun)
        : base(stateMachine, enemyInput, enemyController, enemyAI)
    {
        _gun = gun;
        _launcher = _gun.gameObject.GetComponentInChildren<LaunchArcRenderer>(true);
    }

    public override void Enter()
    {
        enemyInput.SetNewInputFrame(grenadeHeld:true);
    }

    public override void Tick(float deltaTime)
    {
        if (_changingState) { return; }
        _elapsedTime += deltaTime;
        if (_elapsedTime < _giveUpTime) { return; }
        if (_launchDecided)
        {
            Vector3 targetDirection = enemyAI.Target.position - enemyController.transform.position;
            if (stateMachine.CanShootTarget(targetDirection))
            {
                stateMachine.EnterShootingState();
            }
            else
            {
                stateMachine.EnterSeekingState();
            }
            _changingState = true;
        }
        else if (_launcher.IsColliding())
        {
            enemyInput.SetNewInputFrame(grenadeHeld: false);
            _gun.GiveUpGrenade();
        }
        else if (_gun.LastLobGrenadeTime <= 0)
        {
            enemyInput.SetNewInputFrame(grenadeHeld: false, grenadeRelease: true);
        }
        _launchDecided = true;
        _elapsedTime = _chargeTime;
    }

    public override void Exit()
    {
    }
}
