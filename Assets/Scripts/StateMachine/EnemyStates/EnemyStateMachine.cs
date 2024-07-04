using NaughtyAttributes;

public class EnemyStateMachine : StateMachine
{
    private EnemyInput _enemyInput;
    private PlayerController _enemyController;
    private EnemyAI _enemyAI;
    private Gun _gun;

    private void Awake()
    {
        _enemyInput = GetComponent<EnemyInput>();
        _enemyController = GetComponent<PlayerController>();
        _enemyAI = GetComponent<EnemyAI>();
        _gun = GetComponentInChildren<Gun>();
        SwitchState(new EnemyIdleState(this, _enemyInput, _enemyController, _enemyAI));
    }

    [Button]
    private void EnterIdleState()
    {
        SwitchState(new EnemyIdleState(this, _enemyInput, _enemyController, _enemyAI));
    }

    [Button]
    private void EnterPatrolState()
    {
        SwitchState(new EnemyPatrolState(this, _enemyInput, _enemyController, _enemyAI));
    }

    [Button]
    private void EnterShootingState()
    {
        SwitchState(new EnemyShootingState(this, _enemyInput, _enemyController, _enemyAI, _gun));
    }

    [Button]
    private void EnterSeekingState()
    {
        SwitchState(new EnemySeekingState(this, _enemyInput, _enemyController, _enemyAI, _gun));
    }
}
