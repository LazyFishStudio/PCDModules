using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;

public class PCDPlayerActionManager : SingletonMono<PCDPlayerActionManager>
{
	public List<InputDesc> actionDescList;
	private Dictionary<string, Action> playerActionDict;
    private Dictionary<string, Vector2> playerMoveAxisDict;

    public List<IPCDActionHandler> actionHandlers;
    public List<IPCDActionHandler> lateActionHandlers;

    private bool registerFinished = false;
    private System.Object registerLock = new System.Object();

    public void RegisterActionHandler(IPCDActionHandler handler) {
        actionHandlers.Add(handler);
    }

    public void RegisterLateActionHandler(IPCDActionHandler handler) {
        lateActionHandlers.Add(handler);
    }

    public void RegisterAction(string playerName, string keyName, string keyState, string textDesc, Action action) {
        var desc = new InputDesc(playerName, keyName, keyState, textDesc);
        if (textDesc != null)
            actionDescList.Add(desc);
        if (playerActionDict.ContainsKey(desc.ToString()))
            Debug.LogError("Player action map has conflict!");
        playerActionDict[desc.ToString()] = action;
    }

    public void TriggerAction(string actionName) {
        if (!registerFinished)
            RegisterAllActions();
        if (playerActionDict.TryGetValue(actionName, out Action action)) {
            action?.Invoke();
        }
    }

    public void SetMoveAxis(string playerName, Vector2 axis) {
        playerMoveAxisDict[playerName] = axis;
    }

    public Vector2 GetMoveAxis(string playerName) {
        if (playerMoveAxisDict.TryGetValue(playerName, out Vector2 moveAxis))
            return moveAxis;
        return new Vector2(0f, 0f);
    }

    private void Awake() {
        actionDescList = new List<InputDesc>();
        playerActionDict = new Dictionary<string, Action>();
        actionHandlers = new List<IPCDActionHandler>();
        lateActionHandlers = new List<IPCDActionHandler>();
        playerMoveAxisDict = new Dictionary<string, Vector2>();
    }

	private void Update() {
        RegisterAllActions();
    }

	private void LateUpdate() {
        ResetInputActions();
    }

    private void RegisterAllActions() {
        lock (registerLock) {
            if (!registerFinished) {
                foreach (var handler in actionHandlers) {
                    handler.RegisterActionOnUpdate();
                }
                foreach (var handler in lateActionHandlers) {
                    handler.RegisterActionOnUpdate();
                }
                registerFinished = true;
            }
        }
    }

	private void ResetInputActions() {
        lock (registerLock) {
            registerFinished = false;
            actionDescList.Clear();
            playerActionDict.Clear();
        }
    }
}

public struct InputDesc
{
    public string playerName; /* P1, P2, etc */
    public string keyName; /* Pick, Drop, etc */
    public string keyState; /* GetKeyDown, GetKey, GetKeyUp */
    public string keyDesc; /* text description */

    public InputDesc(string playerName, string keyName, string keyState, string desc) {
        this.playerName = playerName;
        this.keyName = keyName;
        this.keyState = keyState;
        this.keyDesc = desc;
    }

    public override string ToString() {
        return string.Format("[{0}, {1}, {2}]", playerName, keyName, keyState);
    }
}
