
using NaughtyAttributes;

public class EnemyStateMachine : StateMachine
{
    private EnemyInput _enemyInput;
    private PlayerController _playerController;

    private void Awake()
    {
        _enemyInput = GetComponent<EnemyInput>();
        _playerController = GetComponent<PlayerController>();
        SwitchState(new EnemyIdleState(this, _enemyInput, _playerController));
    }

    [Button]
    private void EnterIdleState()
    {
        SwitchState(new EnemyIdleState(this, _enemyInput, _playerController));
    }

    [Button]
    private void EnterPatrolState()
    {
        SwitchState(new EnemyPatrolState(this, _enemyInput, _playerController));
    }

    [Button]
    private void EnterShootingState()
    {
        SwitchState(new EnemyShootingState(this, _enemyInput, _playerController));
    }
}
