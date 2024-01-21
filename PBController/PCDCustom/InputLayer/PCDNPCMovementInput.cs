using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDNPCMovementInput : PCDMovementInput
{
    public Vector3 moveDir;

    public override void UpdateRawAxisInput() {
        rawAxisInput = new Vector2(moveDir.x, moveDir.y);
        if (rawAxisInput.magnitude > 1f)
            rawAxisInput = rawAxisInput.normalized;
    }

    public void SetMoveAxis(Vector3 targetDir) {
        moveDir = targetDir;
    }
}