using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDHumanCharacter : MonoBehaviour {
    [SerializeField]
    private PCDHumanMovementSM moveSM;

    void Update() {
        TestMovementInput();
        moveSM?.update();
    }
    
    private void TestMovementInput() {
        float forward = 0, right = 0;
        if (Input.GetKey(KeyCode.W))
            forward += 1f;
        if (Input.GetKey(KeyCode.S))
            forward -= 1f;
        if (Input.GetKey(KeyCode.D))
            right += 1f;
        if (Input.GetKey(KeyCode.A))
            right -= 1f;
        moveSM.moveInput.moveAxis = new Vector2(right, forward);
        if (moveSM.moveInput.moveAxis.magnitude > 1f)
            moveSM.moveInput.moveAxis = moveSM.moveInput.moveAxis.normalized;

        moveSM.moveInput.run = Input.GetKey(KeyCode.LeftShift);
    }

}
