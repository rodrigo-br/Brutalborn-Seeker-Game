using System.Collections;
using UnityEngine;

public class EnemyShootingState : EnemyBaseState
{
    private Vector2 _direction;
    private Gun _gun;
    private float _minStateTime = 1.5f;

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
        Debug.Log("Enter Shooting State");
        enemyInput.SetNewInputFrame(attackDown: true);
        
    }

    public override void Tick(float deltaTime)
    {
        Vector2 direction = enemyAI.Target.position - enemyController.transform.position;
        _direction = direction.normalized;
        _gun.SetDirection(_direction);
        _minStateTime -= deltaTime;
        if (!stateMachine.CanShootTarget(direction) && _minStateTime < 0)
        {
            stateMachine.EnterSeekingState();
        }
    }

    public override void Exit()
    {
        
    }
}
