using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    public Transform Target;
    public Vector2 Direction { get; private set; }
    public float NextWayPointDistance = 3f;
    private Path _path;
    private int _currentWaypoint = 0;
    private bool _reachedEndOfPath = false;
    private Seeker _seeker;

    private void Awake()
    {
        _seeker = GetComponent<Seeker>();
    }

    private void Start()
    {
        InvokeRepeating(nameof(UpdatePath), 0, 1f);
    }

    private void FixedUpdate()
    {
        if (_path == null) { return; }

        if (_currentWaypoint >= _path.vectorPath.Count)
        {
            _reachedEndOfPath = true;
            return;
        }
        else
        {
            _reachedEndOfPath = false;
        }

        Direction = (Vector2)(_path.vectorPath[_currentWaypoint] - transform.position).normalized;

        float distance = Vector2.Distance(transform.position, _path.vectorPath[_currentWaypoint]);

        if (distance < NextWayPointDistance)
        {
            _currentWaypoint++;
        }
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            _path = p;
            _currentWaypoint = 0;
        }
    }

    private void UpdatePath()
    {
        if (!Target)
        {
            CancelInvoke(nameof(UpdatePath));
            return;
        }
        if (_seeker.IsDone())
        {
            _seeker.StartPath(transform.position, Target.position, OnPathComplete);
        }
    }


}
