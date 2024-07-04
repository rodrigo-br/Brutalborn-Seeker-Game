using UnityEngine;

public class EnemySeekingState : EnemyBaseState
{
    private Vector2 _direction;
    private Gun _gun;

    public EnemySeekingState(
        EnemyStateMachine stateMachine,
        EnemyInput enemyInput,
        PlayerController enemyController,
        EnemyAI enemyAI,
        Gun gun)
        : base(stateMachine, enemyInput, enemyController, enemyAI)
    {
        _gun = gun;
    }

    public override void Enter()
    {
        
    }

    public override void Tick(float deltaTime)
    {
        _direction = enemyAI.Direction;
        bool jump = false;
        if (_direction.y >= 0.9f)
        {
            jump = true;
        }
        enemyInput.SetNewInputFrame(move: _direction, jumpDown: jump, jumpHeld: jump);
    }

    public override void Exit()
    {
        
    }
}
