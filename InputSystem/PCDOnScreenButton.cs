using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.OnScreen;

public class PCDOnScreenButton : OnScreenButton
{
	private bool initialed = false;
	protected override void OnEnable() {
		if (!initialed) {
			initialed = true;
			base.OnEnable();
		}
	}

	protected override void OnDisable() {
		if (!control .CheckStateIsAtDefault())
			SentDefaultValueToControl();
	}
}
