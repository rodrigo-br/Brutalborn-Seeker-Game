using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyInput : MonoBehaviour, IInput
{
    public FrameInput GatherInput()
    {
        return new FrameInput
        {
            Move = Vector2.zero,
            JumpDown = false,
            JumpHeld = false,
            AttackDown = false,
            AttackHeld = false,
            DashDown = false,
            InteractDown = false,
            InteractHeld = false,
        };
    }
}
