using UnityEngine;
using Pathfinding;
using System;

public class EnemyAI : MonoBehaviour
{
    public Transform Target;
    public float Speed = 200f;
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
        _seeker.StartPath(transform.position, Target.position, OnPathComplete);
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            _path = p;
            _currentWaypoint = 0;
        }
    }
}
