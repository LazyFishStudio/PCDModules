using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;

public class PCDPlayerActionManager : SingletonMono<PCDPlayerActionManager>
{
	public List<InputDesc> actionDescList;
	private Dictionary<string, Action> playerActionDict;

    public List<IPCDActionHandler> actionHandlers;
    public List<IPCDActionHandler> lateActionHandlers;

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

    private void Awake() {
        actionDescList = new List<InputDesc>();
        playerActionDict = new Dictionary<string, Action>();
        actionHandlers = new List<IPCDActionHandler>();
        lateActionHandlers = new List<IPCDActionHandler>();
    }

	private void Update() {
        foreach (var handler in actionHandlers) {
            handler.RegisterActionOnUpdate();
		}
        foreach (var handler in lateActionHandlers) {
            handler.RegisterActionOnUpdate();
		}

        HandlePlayerInput();
    }

	private void LateUpdate() {
        ResetInputActions();
    }

	private void ResetInputActions() {
        actionDescList.Clear();
        playerActionDict.Clear();
    }

    private void HandlePlayerInput() {
        KeyCode[] keyList = { KeyCode.Mouse0, KeyCode.Mouse1, KeyCode.F };

        foreach (var key in keyList) {
            if (Input.GetKeyDown(key)) {
                string keyName = string.Format("[{0}, {1}, {2}]", "P1", key.ToString(), "GetKeyDown");
                if (playerActionDict.TryGetValue(keyName, out Action action)) {
                    action?.Invoke();
				}
			}
            if (Input.GetKey(key)) {
                string keyName = string.Format("[{0}, {1}, {2}]", "P1", key.ToString(), "GetKey");
                if (playerActionDict.TryGetValue(keyName, out Action action)) {
                    action?.Invoke();
                }
            }
            if (Input.GetKeyUp(key)) {
                string keyName = string.Format("[{0}, {1}, {2}]", "P1", key.ToString(), "GetKeyUp");
                if (playerActionDict.TryGetValue(keyName, out Action action)) {
                    action?.Invoke();
                }
            }
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
