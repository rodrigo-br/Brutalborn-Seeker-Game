using UnityEngine;

public class EnemyPatrolState : EnemyBaseState
{
    private LeftRightPatrol _leftRightPatrol;
    private readonly float _flipCooldown = 0.2f;
    private float _currentFlipCooldown = 0;

    public EnemyPatrolState(
        EnemyStateMachine stateMachine,
        EnemyInput enemyInput,
        PlayerController playerController,
        EnemyAI enemyAI,
        LeftRightPatrol leftRightPatrol)
        : base(stateMachine, enemyInput, playerController, enemyAI)
    {
        _leftRightPatrol = leftRightPatrol;
    }

    public override void Enter()
    {
        Debug.Log("Enter Enemy Patrol State");
        enemyInput.SetNewInputFrame(move: Vector2.right);
    }

    public override void Tick(float deltaTime)
    {
        if (enemyAI.PathDistance <= 30f)
        {
            stateMachine.EnterSeekingState();
        }
        _currentFlipCooldown += deltaTime;
        if (_currentFlipCooldown < _flipCooldown) { return; }
        if (_leftRightPatrol.FoundEdge)
        {
            Vector2 currentMovementDirection = enemyInput.GatherInput().Move;
            enemyInput.SetNewInputFrame(move: new Vector2(currentMovementDirection.x * -1, currentMovementDirection.y));
            _leftRightPatrol.ClearFoundEdge();
            _currentFlipCooldown = 0;
        }
    }

    public override void Exit()
    {
    }
}
