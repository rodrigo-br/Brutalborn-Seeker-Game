using UnityEngine;

public class EnemyPatrolState : EnemyBaseState
{
    public EnemyPatrolState(
        EnemyStateMachine stateMachine,
        EnemyInput enemyInput,
        PlayerController playerController,
        EnemyAI enemyAI)
        : base(stateMachine, enemyInput, playerController, enemyAI)
    {
    }

    public override void Enter()
    {
        Debug.Log("Enter Enemy Patrol State");
        enemyInput.SetNewInputFrame(move: Vector2.left);
    }

    public override void Tick(float deltaTime)
    {
        Vector2 currentMovementDirection = enemyInput.GatherInput().Move;
        if (enemyController.WallDirection == currentMovementDirection.x)
        {
            enemyInput.SetNewInputFrame(move: new Vector2(currentMovementDirection.x * -1, currentMovementDirection.y));
        }
    }

    public override void Exit()
    {
    }
}
