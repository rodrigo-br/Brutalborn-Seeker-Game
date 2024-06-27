using UnityEngine;

public class Parallax : MonoBehaviour
{
    [SerializeField] private Vector2 _movementScale = Vector2.one;

    Transform _camera;

    void Awake()
    {
        _camera = Camera.main.transform;
    }

    void LateUpdate()
    {
        transform.position = Vector2.Scale(_camera.position, _movementScale);
    }
}
