using System.Collections;
using UnityEngine;

public class OneWayPlatform : MonoBehaviour
{
    [SerializeField] private float _disableColliderTime = 0.8f;
    private BoxCollider2D _boxCollider;
    private PlayerController _playerController;
    private bool _playerOnPlatform = false;

    private void Awake()
    {
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        _playerController = FindFirstObjectByType<PlayerInput>().GetComponent<PlayerController>();
    }

    private void Update()
    {
        DetectPlayerInput();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerHitableColliders"))
        {
            _playerOnPlatform = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerHitableColliders"))
        {
            _playerOnPlatform = false;
        }
    }

    private void DetectPlayerInput()
    {
        if (!_playerOnPlatform) { return; }

        if (_playerController.Input.y < 0f)
        {
            StartCoroutine(DisablePlatformColliderRoutine());
        }
    }

    private IEnumerator DisablePlatformColliderRoutine()
    {
        Collider2D[] playerColliders = _playerController.GetComponents<Collider2D>();

        _playerController.SetBlockGrounded(true);
        foreach (Collider2D collider in playerColliders)
        {
            Physics2D.IgnoreCollision(collider, _boxCollider, true);
        }

        yield return new WaitForSeconds(_disableColliderTime);

        foreach (Collider2D collider in playerColliders)
        {
            Physics2D.IgnoreCollision(collider, _boxCollider, false);
        }
        _playerController.SetBlockGrounded(false);
    }
}
