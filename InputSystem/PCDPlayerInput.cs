using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
public class PCDPlayerInput : MonoBehaviour
{
    public string playerName = "P1";

    private PCDPlayerActionManager actionManager;

    private UnityEngine.InputSystem.PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction firstInteract;
    private InputAction secondInteract;
    private InputAction respawnInteract;

    private bool firstInteractPressing = false;
    private bool secondInteractPressing = false;

    private void Awake() {
        actionManager = PCDPlayerActionManager.GetInstance();
        playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
        moveAction = playerInput.actions["Move"];
        firstInteract = playerInput.actions["FirstInteract"];
        secondInteract = playerInput.actions["SecondInteract"];
        respawnInteract = playerInput.actions["Respawn"];
    }

	private void OnEnable() {
        moveAction.performed += OnMove;
        moveAction.canceled += OnMove;
        firstInteract.performed += FirstInteractPerformed;
        firstInteract.canceled += FirstInteractCanceled;
        secondInteract.performed += SecondInteractPerformed;
        secondInteract.canceled += SecondInteractCanceled;
        respawnInteract.performed += RespawnInteractPerformed;
    }

	private void OnDisable() {
        moveAction.performed -= OnMove;
        moveAction.canceled -= OnMove;
        firstInteract.performed -= FirstInteractPerformed;
        firstInteract.canceled -= FirstInteractCanceled;
        secondInteract.performed -= SecondInteractPerformed;
        secondInteract.canceled -= SecondInteractCanceled;
        respawnInteract.performed -= RespawnInteractPerformed;
    }

	private void Update() {
        if (firstInteractPressing)
            actionManager.TriggerAction(string.Format("[{0}, {1}, {2}]", playerName, "FirstInteract", "GetKey"));
        if (secondInteractPressing)
            actionManager.TriggerAction(string.Format("[{0}, {1}, {2}]", playerName, "SecondInteract", "GetKey"));
    }

	private void OnMove(InputAction.CallbackContext ctx) {
        Vector2 moveAxis = ctx.ReadValue<Vector2>();
        actionManager.SetMoveAxis(playerName, moveAxis);
    }
    private void FirstInteractPerformed(InputAction.CallbackContext ctx) {
        firstInteractPressing = true;
        actionManager.TriggerAction(string.Format("[{0}, {1}, {2}]", playerName, "FirstInteract", "GetKeyDown"));
    }
    private void FirstInteractCanceled(InputAction.CallbackContext ctx) {
        firstInteractPressing = false;
        actionManager.TriggerAction(string.Format("[{0}, {1}, {2}]", playerName, "FirstInteract", "GetKeyUp"));
    }
    private void SecondInteractPerformed(InputAction.CallbackContext ctx) {
        secondInteractPressing = true;
        actionManager.TriggerAction(string.Format("[{0}, {1}, {2}]", playerName, "SecondInteract", "GetKeyDown"));
    }
    private void SecondInteractCanceled(InputAction.CallbackContext ctx) {
        secondInteractPressing = false;
        actionManager.TriggerAction(string.Format("[{0}, {1}, {2}]", playerName, "SecondInteract", "GetKeyUp"));
    }
    private void RespawnInteractPerformed(InputAction.CallbackContext ctx) {
        Debug.Log("Respawn");
        actionManager.TriggerAction(string.Format("[{0}, {1}, {2}]", playerName, "RespawnInteract", "GetKeyDown"));
    }
}

