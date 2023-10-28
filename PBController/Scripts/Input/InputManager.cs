using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class InputManager
{
    static private bool _forbid;

    static public void SetInputForbid(bool forbid) {
        _forbid = forbid;
    }

    static public bool GetKey(KeyCode keyCode) {
        return !_forbid && Input.GetKey(keyCode);
	}

    static public bool GetKeyDown(KeyCode keyCode) {  
        return !_forbid && Input.GetKeyDown(keyCode);
    }

    static public bool GetKeyUp(KeyCode keyCode) {
        return !_forbid && Input.GetKeyUp(keyCode);
    }

    static public float GetAxis(string axisName) {
        return _forbid ? 0f : Input.GetAxis(axisName);
    }

    static public float GetAxisRaw(string axisName) {
        return _forbid ? 0f : Input.GetAxisRaw(axisName);
    }
}
