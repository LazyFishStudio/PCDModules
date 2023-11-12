using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDHumanCharacter : MonoBehaviour {
    [SerializeField]
    protected PCDHumanMovementSM moveSM;
    protected Condition condition;
    protected bool isCanRun => condition ? condition.data.saitity > 0 : true;

    void Awake() {
        condition = GetComponent<Condition>();
    }

    void Update() {
        MovementInput();
    }
    
    protected virtual void MovementInput() {
        float forward = 0, right = 0;
        if (InputManager.GetKey(KeyCode.W))
            forward += 1f;
        if (InputManager.GetKey(KeyCode.S))
            forward -= 1f;
        if (InputManager.GetKey(KeyCode.D))
            right += 1f;
        if (InputManager.GetKey(KeyCode.A))
            right -= 1f;
        moveSM.moveInput.moveAxis = new Vector2(right, forward);
        if (moveSM.moveInput.moveAxis.magnitude > 1f)
            moveSM.moveInput.moveAxis = moveSM.moveInput.moveAxis.normalized;

        // 跑步切换判断
        if (InputManager.GetKeyDown(KeyCode.LeftShift) && isCanRun) {
            moveSM.moveInput.run = !moveSM.moveInput.run;
        }

        if (moveSM.moveInput.run) {
            condition?.LoseSaitity(Time.deltaTime / 60.0f);
        }

        if (!isCanRun || moveSM.moveInput.moveAxis == Vector3.zero) {
            moveSM.moveInput.run = false;
        }

    }

    public virtual void ClearInput() {
        moveSM.moveInput.moveAxis = Vector3.zero;
    }

}
