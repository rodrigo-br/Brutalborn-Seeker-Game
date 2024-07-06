using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaunchArcRenderer : MonoBehaviour
{
    [SerializeField] private int _relosution = 10;
    [SerializeField] private float _angle;
    [SerializeField] private float _velocity;
    [SerializeField] private Transform _parentTransform;
    private LineRenderer _lineRenderer;
    private float _gravity;
    private float _radianAngle;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _gravity = Mathf.Abs(Physics2D.gravity.y);
    }

    private void Update()
    {
        RenderArc();
    }

    private void Start()
    {
        RenderArc();
    }

    private void RenderArc()
    {
        _lineRenderer.positionCount = _relosution + 1;
        _lineRenderer.SetPositions(CalculateArcArray());
    }

    private Vector3[] CalculateArcArray()
    {
        Vector3[] arcArray = new Vector3[_relosution + 1];
        _radianAngle = Mathf.Deg2Rad * _angle;
        float maxDistance = (_velocity * _velocity * Mathf.Sin(2 * _radianAngle)) / _gravity;

        for (int i = 0; i <= _relosution; i++)
        {
            float t = (float)i / (float)_relosution;
            arcArray[i] = CalculateArcPoint(t, maxDistance);
        }

        return arcArray;
    }

    private Vector3 CalculateArcPoint(float t, float maxDistance)
    {
        float x = t * maxDistance;
        float y = x * Mathf.Tan(_radianAngle)
            - ((_gravity * x * x) / (2 * _velocity * _velocity * Mathf.Cos(_radianAngle) * Mathf.Cos(_radianAngle)));
        return new Vector2(x * -1, y) + (Vector2)_parentTransform.position;
    }
}
