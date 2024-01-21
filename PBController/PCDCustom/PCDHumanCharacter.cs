#if false

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDHumanCharacter : MonoBehaviour {
    public string playerName = "P1";

    [SerializeField]
    protected PCDHumanMovementSM moveSM;
    protected bool isCanRun => IsCanRun();
    public Vector2 inputAxis;

    protected virtual void Awake() { }

    protected float forward = 0f;
    protected float right = 0f;
    void Update() {
        Vector3 moveAxis = PCDPlayerActionManager.GetInstance().GetMoveAxis(playerName);
        var locker = GetComponent<PCDActLocker>();
        if (locker != null && locker.movementLocked)
            moveAxis = Vector3.zero;

        forward = moveAxis.y;
        right = moveAxis.x;

        MovementInput();
    }
    
    protected virtual void MovementInput() {
        inputAxis = new Vector2(right, forward);
        moveSM.moveInput.moveAxis = inputAxis;
        if (moveSM.moveInput.moveAxis.magnitude > 1f)
            moveSM.moveInput.moveAxis = moveSM.moveInput.moveAxis.normalized;

        HandleRunInput();

    }

    protected virtual void HandleRunInput() {
         // 跑步切换判断
        if (InputManager.GetKeyDown(KeyCode.LeftShift) && isCanRun) {
            moveSM.moveInput.run = !moveSM.moveInput.run;
        }

        if (!isCanRun || moveSM.moveInput.moveAxis == Vector3.zero) {
            moveSM.moveInput.run = false;
        }
    }

    public virtual void ClearInput() {
        moveSM.moveInput.moveAxis = Vector3.zero;
    }

    public virtual bool IsCanRun() {
        return true;
    }

}

#endif