using UnityEngine;

public class EnemyShootingState : EnemyBaseState
{
    private Vector2 _direction;
    private Gun _gun;

    public EnemyShootingState(
        EnemyStateMachine stateMachine,
        EnemyInput enemyInput,
        PlayerController enemyController,
        EnemyAI enemyAI,
        Gun gun)
        : base(stateMachine,
            enemyInput,
            enemyController,
            enemyAI)
    {
        _gun = gun;
    }

    public override void Enter()
    {
        enemyInput.SetNewInputFrame(attackDown: true);
    }

    public override void Tick(float deltaTime)
    {
        _direction = enemyAI.Target.position - enemyController.transform.position;
        _direction.Normalize();
        _gun.SetDirection(_direction);
    }

    public override void Exit()
    {
        
    }
}
