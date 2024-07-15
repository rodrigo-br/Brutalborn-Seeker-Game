using UnityEngine;

public class EnemyPatrolState : EnemyBaseState
{
    private LeftRightPatrol _leftRightPatrol;
    private readonly float _flipCooldown = 0.2f;
    private float _currentFlipCooldown = 0;
    private Health _health;
    private float _currentHealth;

    public EnemyPatrolState(
        EnemyStateMachine stateMachine,
        EnemyInput enemyInput,
        PlayerController playerController,
        EnemyAI enemyAI,
        LeftRightPatrol leftRightPatrol,
        Health health)
        : base(stateMachine, enemyInput, playerController, enemyAI)
    {
        _leftRightPatrol = leftRightPatrol;
        _health = health;
    }

    public override void Enter()
    {
        Debug.Log("Enter Enemy Patrol State");
        enemyInput.SetNewInputFrame(move: Vector2.right);
        _currentHealth = _health.CurrentHealth;
    }

    public override void Tick(float deltaTime)
    {
        if (enemyAI.PathDistance <= 20f || _currentHealth != _health.CurrentHealth)
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
