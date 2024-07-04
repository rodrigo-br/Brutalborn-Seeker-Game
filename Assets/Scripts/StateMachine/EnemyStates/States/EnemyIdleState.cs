using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public EnemyIdleState(
        EnemyStateMachine stateMachine,
        EnemyInput enemyInput,
        PlayerController playerController,
        EnemyAI enemyAI)
        : base(stateMachine, enemyInput, playerController, enemyAI)
    {
    }

    public override void Enter()
    {
        Debug.Log("Enter Enemy Idle State");
        enemyInput.SetNewInputFrame();
    }

    public override void Tick(float deltaTime)
    {
        stateMachine.EnterSeekingState();
    }

    public override void Exit()
    {
        
    }
}
