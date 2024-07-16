using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour, IInput
{
    private PlayerInputActions _inputActions;
    private InputAction _move, _jump, _dash, _attack, _interact, _jetpack, _grenade;

    private void Awake()
    {
        Debug.Log("Player Input");
        _inputActions = new PlayerInputActions();
        _move = _inputActions.Player.Move;
        _jump = _inputActions.Player.Jump;
        _dash = _inputActions.Player.Dash;
        _attack = _inputActions.Player.Attack;
        _interact = _inputActions.Player.Interact;
        _jetpack = _inputActions.Player.Jetpack;
        _grenade = _inputActions.Player.Grenade;
    }

    private void OnEnable()
    {
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions.Disable();
    }

    public FrameInput GatherInput()
    {
        return new FrameInput
        {
            Move = _move.ReadValue<Vector2>(),
            JumpDown = _jump.WasPerformedThisFrame(),
            JumpHeld = _jump.IsPressed(),
            AttackDown = _attack.WasPerformedThisFrame(),
            AttackHeld = _attack.IsPressed(),
            DashDown = _dash.WasPerformedThisFrame(),
            InteractDown = _interact.WasPerformedThisFrame(),
            InteractHeld = _interact.IsPressed(),
            JetpackDown = _jetpack.WasPerformedThisFrame(),
            GrenadeRelease = _grenade.WasReleasedThisFrame(),
            GrenadeHeld = _grenade.IsPressed(),
        };
    }
}

public struct FrameInput
{
    public Vector2 Move;
    public bool JumpDown;
    public bool JumpHeld;
    public bool AttackDown;
    public bool AttackHeld;
    public bool DashDown;
    public bool InteractDown;
    public bool InteractHeld;
    public bool JetpackDown;
    public bool GrenadeRelease;
    public bool GrenadeHeld;
}

public interface IInput
{
    FrameInput GatherInput();
}
