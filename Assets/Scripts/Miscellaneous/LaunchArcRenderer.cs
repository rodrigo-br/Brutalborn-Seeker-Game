using System.Drawing;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaunchArcRenderer : MonoBehaviour
{
    public float CurrentVelocity { get; private set; }
    public float CurrentAngle { get; private set; }
    [SerializeField] private int _relosution = 10;
    [SerializeField] private float _angle;
    [SerializeField] private float _maxVelocity = 20;
    [SerializeField] private Transform _parentTransform;
    [SerializeField] private LayerMask _enemyLayerMask;
    private float _explosionRadius = 1f;
    private LineRenderer _lineRenderer;
    private float _gravity;
    private float _radianAngle;
    public bool Fire { get; private set; } = false;
    private bool _isPlayer = false;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _gravity = Mathf.Abs(Physics2D.gravity.y);
        CurrentVelocity = 1;
        CurrentAngle = _angle;
        if (gameObject.CompareTag("Player"))
        {
            _isPlayer = true;
        }
    }

    private void Update()
    {
        RenderArc();
    }

    private void Start()
    {
        RenderArc();
    }

    public void IncrementVelocity(float incrementAmount, float x)
    {
        CurrentVelocity = Mathf.Clamp(CurrentVelocity + incrementAmount, 0, _maxVelocity);
        CurrentAngle = x < 0 ? _angle : -_angle;
    }

    public void ResetVelocity()
    {
        CurrentVelocity = 1;
        gameObject.SetActive(false);
        Fire = false;
    }

    private void RenderArc()
    {
        _lineRenderer.positionCount = _relosution + 1;
        _lineRenderer.SetPositions(CalculateArcArray());
    }

    private Vector3[] CalculateArcArray()
    {
        Vector3[] arcArray = new Vector3[_relosution + 1];
        _radianAngle = Mathf.Deg2Rad * CurrentAngle;
        float maxDistance = (CurrentVelocity * CurrentVelocity * Mathf.Sin(2 * _radianAngle)) / _gravity;

        for (int i = 0; i <= _relosution; i++)
        {
            float t = (float)i / (float)_relosution;
            arcArray[i] = CalculateArcPoint(t, maxDistance);
            if (_isPlayer) { continue; }
            Collider2D[] hits = Physics2D.OverlapCircleAll(arcArray[i], _explosionRadius, _enemyLayerMask);
            if (hits.Length > 0)
            {
                Fire = true;
            }
            else if (i > 0)
            {
                RaycastHit2D hit = Physics2D.Raycast(arcArray[i - 1], arcArray[i] - arcArray[i - 1], Vector3.Distance(arcArray[i - 1], arcArray[i]));
                if (hit.collider != null)
                {
                    arcArray[i] = arcArray[i - 1];
                }
            }
        }
        return arcArray;
    }

    private Vector3 CalculateArcPoint(float t, float maxDistance)
    {
        float x = t * maxDistance;
        float y = x * Mathf.Tan(_radianAngle)
            - ((_gravity * x * x) / (2 * CurrentVelocity * CurrentVelocity * Mathf.Cos(_radianAngle) * Mathf.Cos(_radianAngle)));

        return new Vector2(x * -1, y) + (Vector2)_parentTransform.position;
    }
}
