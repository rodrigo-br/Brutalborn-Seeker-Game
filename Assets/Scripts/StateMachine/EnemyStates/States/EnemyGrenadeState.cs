public class EnemyGrenadeState : EnemyBaseState
{
    private Gun _gun;
    private LaunchArcRenderer _launcher;
    private float _giveUpTime = 2f;
    private float _elapsedTime = 0f;
    private bool _launchDecided = false;

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
        _elapsedTime += deltaTime;
        if (_elapsedTime < _giveUpTime) { return; }
        if (_launchDecided)
        {
            stateMachine.EnterSeekingState();
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
        _elapsedTime = _giveUpTime / 2;
    }

    public override void Exit()
    {
    }
}
