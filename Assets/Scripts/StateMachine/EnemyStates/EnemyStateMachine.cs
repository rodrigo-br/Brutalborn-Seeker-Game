using NaughtyAttributes;
using UnityEngine;

public class EnemyStateMachine : StateMachine
{
    [SerializeField] private LayerMask _targetLayerMask;
    [SerializeField] private LayerMask _ignoreLayerMask;
    private EnemyInput _enemyInput;
    private PlayerController _enemyController;
    private EnemyAI _enemyAI;
    private Gun _gun;
    private RaycastHit2D _lastHit;

    private void Awake()
    {
        _enemyInput = GetComponent<EnemyInput>();
        _enemyController = GetComponent<PlayerController>();
        _enemyAI = GetComponent<EnemyAI>();
        _gun = GetComponentInChildren<Gun>();
        SwitchState(new EnemyIdleState(this, _enemyInput, _enemyController, _enemyAI));
    }

    [Button]
    public void EnterIdleState()
    {
        SwitchState(new EnemyIdleState(this, _enemyInput, _enemyController, _enemyAI));
    }

    [Button]
    public void EnterPatrolState()
    {
        SwitchState(new EnemyPatrolState(this, _enemyInput, _enemyController, _enemyAI));
    }

    [Button]
    public void EnterShootingState()
    {
        SwitchState(new EnemyShootingState(this, _enemyInput, _enemyController, _enemyAI, _gun));
    }

    [Button]
    public void EnterSeekingState()
    {
        SwitchState(new EnemySeekingState(this, _enemyInput, _enemyController, _enemyAI, _gun));
    }

    public bool CanShootTarget(Vector2 targetDirection)
    {
        Vector2 origin = _gun.BulletSpawnPosition.position;
        float distance = targetDirection.magnitude;

        int layerMask = ~_ignoreLayerMask;

        RaycastHit2D _lastHit = Physics2D.Raycast(origin, targetDirection, distance, layerMask);
        _rayOrigin = origin;
        _rayDirection = targetDirection;
        _rayDistance = distance;
        if (_lastHit.collider != null && (_lastHit.collider.gameObject.layer & _targetLayerMask) != 0)
        {
            if (_lastHit.collider.gameObject.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }

    private Vector2 _rayOrigin;
    private Vector2 _rayDirection;
    private float _rayDistance;
    private void OnDrawGizmos()
    {
        if (_lastHit.collider != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(_rayOrigin, _lastHit.point);
            Gizmos.DrawSphere(_lastHit.point, 0.1f);
        }
        else
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_rayOrigin, _rayOrigin + _rayDirection.normalized * _rayDistance);
        }
    }
}
