using UnityEngine;

public class EnemyIdleState : EnemyBaseState
{
    public EnemyIdleState(
        EnemyStateMachine stateMachine,
        EnemyInput enemyInput,
        PlayerController playerController)
        : base(stateMachine, enemyInput, playerController)
    {
    }

    public override void Enter()
    {
        Debug.Log("Enter Enemy Idle State");
        enemyInput.SetNewInputFrame();
    }

    public override void Tick(float deltaTime)
    {
        
    }

    public override void Exit()
    {
        
    }
}
