using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintTypeSelector : MonoBehaviour
{
	public GameObject mouseLeftObj;
	public GameObject mouseRightObj;
	public GameObject keyFObj;

	private void Awake() {
		mouseLeftObj.SetActive(true);
		mouseRightObj.SetActive(false);
		keyFObj.SetActive(false);
	}

	public void RefreshHintType(InteractHintType type) {
		mouseLeftObj.SetActive(type == InteractHintType.MouseLeft);
		mouseRightObj.SetActive(type == InteractHintType.MouseRight);
		keyFObj.SetActive(type == InteractHintType.KeyF);
	}
}
