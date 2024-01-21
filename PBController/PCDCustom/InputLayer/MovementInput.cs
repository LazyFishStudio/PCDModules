using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MovementInput : MonoBehaviour
{
    [Header("Enable inputs")]
    public bool enableMovement = true;
    public bool enableJump = true;
    public bool enableCrouch = true;
    public bool enableSprint = true;

    /*
    [HideInInspector] public Vector2 axisInput;
    [HideInInspector] public bool jump;
    [HideInInspector] public bool jumpHold;
    [HideInInspector] public bool sprint;
    [HideInInspector] public bool crouch;
    */

    [Header("Realtime Status")]
    public Vector2 axisInput = Vector2.zero;
    public bool jump = false;
    public bool jumpHold = false;
    public bool sprint = false;
    public bool crouch = false;

    public virtual bool IsInputActive => enableMovement;
    public abstract void UpdateInput();
    public abstract void SetInputActive(bool isActive);
}
