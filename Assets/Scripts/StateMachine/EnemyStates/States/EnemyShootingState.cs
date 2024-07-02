public class EnemyShootingState : EnemyBaseState
{
    public EnemyShootingState(
        EnemyStateMachine stateMachine,
        EnemyInput enemyInput,
        PlayerController enemyController)
        : base(stateMachine,
            enemyInput,
            enemyController)
    {
    }

    public override void Enter()
    {
        enemyInput.SetNewInputFrame(attackDown: true);
    }

    public override void Tick(float deltaTime)
    {

    }

    public override void Exit()
    {
        
    }
}
