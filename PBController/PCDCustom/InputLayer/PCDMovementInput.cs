using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PCDMovementInput : MovementInput
{
    public Vector2 rawAxisInput;

    public abstract void UpdateRawAxisInput();

    public override void UpdateInput() {
        GetComponent<PCDMovementMgr>().UpdateInput();
    }

    public override void SetInputActive(bool isActive) {
        if (!isActive) {
            enableMovement = false;
            axisInput = Vector2.zero;
            GetComponent<PCDMovementMgr>().ClearInput();
        } else {
            enableMovement = true;
        }
    }
}
