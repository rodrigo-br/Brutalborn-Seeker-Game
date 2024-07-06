using UnityEngine;

public abstract class EnemyBaseState : State
{
    protected EnemyStateMachine stateMachine;
    protected EnemyInput enemyInput;
    protected PlayerController enemyController;
    protected EnemyAI enemyAI;

    public EnemyBaseState(
        EnemyStateMachine stateMachine,
        EnemyInput enemyInput,
        PlayerController enemyController,
        EnemyAI enemyAI)
    {
        this.stateMachine = stateMachine;
        this.enemyInput = enemyInput;
        this.enemyController = enemyController;
        this.enemyAI = enemyAI;
    }
}