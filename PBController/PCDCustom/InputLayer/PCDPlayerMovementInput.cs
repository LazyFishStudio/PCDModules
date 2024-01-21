using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDPlayerMovementInput : PCDMovementInput
{
    public string playerName = "P1";

    public override void UpdateRawAxisInput() {
        Vector3 moveAxis = PCDPlayerActionManager.Instance.GetMoveAxis(playerName);
        var locker = GetComponent<PCDActLocker>();
        if (locker != null && locker.movementLocked)
            moveAxis = Vector3.zero;

        rawAxisInput = new Vector2(moveAxis.x, moveAxis.y);
        if (rawAxisInput.magnitude > 1f)
            rawAxisInput = rawAxisInput.normalized;
    }
}
