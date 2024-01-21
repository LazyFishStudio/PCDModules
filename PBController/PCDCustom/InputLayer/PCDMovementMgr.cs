using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PhysicsBasedCharacterController;

public class PCDMovementMgr : MonoBehaviour
{
    public float walkSpeed = 6.0f;
    public string walkPoseLayerName = "Walk";

    protected PCDMovementInput movementInput;
    protected CharacterManager characterMgr;
    protected PCDWalkMgr[] walkMgrs;

    protected void Awake() {
        movementInput = GetComponent<PCDMovementInput>();
        characterMgr = GetComponent<CharacterManager>();
        walkMgrs = GetComponentsInChildren<PCDWalkMgr>();
    }

    public virtual void UpdateInput() {
        if (!movementInput.IsInputActive)
            return;

        characterMgr.movementSpeed = walkSpeed;
        foreach (PCDWalkMgr human in walkMgrs)
            human.SetAnim(walkPoseLayerName);

        movementInput.UpdateRawAxisInput();
        movementInput.axisInput = movementInput.rawAxisInput;
    }

    public virtual void ClearInput() {}
}
