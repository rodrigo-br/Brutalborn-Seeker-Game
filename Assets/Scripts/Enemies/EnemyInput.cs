using UnityEngine;

public class EnemyInput : MonoBehaviour, IInput
{
    private FrameInput _frameInput;

    private void Awake()
    {
    }

    public FrameInput GatherInput()
    {
        return _frameInput;
    }

    public void SetNewInputFrame(
        Vector2 move = new Vector2(),
        bool jumpDown = false,
        bool jumpHeld = false,
        bool attackDown = false,
        bool attackHeld = false,
        bool dashDown = false,
        bool interactDown = false,
        bool interactHeld = false,
        bool grenadeHeld = false,
        bool grenadeRelease = false)
    {
        _frameInput.Move = move;
        _frameInput.JumpDown = jumpDown;
        _frameInput.JumpHeld = jumpHeld;
        _frameInput.AttackDown = attackDown;
        _frameInput.AttackHeld = attackHeld;
        _frameInput.DashDown = dashDown;
        _frameInput.InteractDown = interactDown;
        _frameInput.InteractHeld = interactHeld;
        _frameInput.GrenadeHeld = grenadeHeld;
        _frameInput.GrenadeRelease = grenadeRelease;
    }
}
