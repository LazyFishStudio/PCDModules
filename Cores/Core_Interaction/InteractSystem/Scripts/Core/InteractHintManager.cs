using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractHintManager : MonoBehaviour
{
	static public InteractHintManager Inst;
	static public InteractHintType curHintType;


	private void Awake() {
		if (Inst != null)
			throw new System.Exception("More than one InteractHintManager exists!");
		Inst = this;
	}

	public GameObject hintObject;

	static public void ShowHintText(string text, InteractHintType hintType) {
		curHintType = hintType;
		Inst.hintObject.SetActive(true);
		var mesh = Inst.hintObject.GetComponentInChildren<TMPro.TextMeshProUGUI>();
		if (mesh != null)
			mesh.text = text;
	}

	static public void HideHintText() {
		Inst.hintObject.SetActive(false);
	}
}

public enum InteractHintType { MouseLeft, MouseRight, KeyF }