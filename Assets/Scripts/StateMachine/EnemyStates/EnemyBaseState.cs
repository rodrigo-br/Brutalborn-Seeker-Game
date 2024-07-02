public abstract class EnemyBaseState : State
{
    protected EnemyStateMachine stateMachine;
    protected EnemyInput enemyInput;
    protected PlayerController enemyController;

    public EnemyBaseState(
        EnemyStateMachine stateMachine,
        EnemyInput enemyInput,
        PlayerController enemyController)
    {
        this.stateMachine = stateMachine;
        this.enemyInput = enemyInput;
        this.enemyController = enemyController;
    }
}