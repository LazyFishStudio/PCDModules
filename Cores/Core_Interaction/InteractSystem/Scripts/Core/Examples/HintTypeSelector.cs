using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintTypeSelector : MonoBehaviour
{
	public GameObject mouseLeftObj;
	public GameObject mouseRightObj;
	public GameObject keyFObj;

	private void Awake() {
		mouseLeftObj.SetActive(false);
		mouseRightObj.SetActive(false);
		keyFObj.SetActive(false);
		switch (InteractHintManager.curHintType) {
			case InteractHintType.MouseLeft:
				mouseLeftObj.SetActive(true);
				break;
			case InteractHintType.MouseRight:
				mouseRightObj.SetActive(true);
				break;
			case InteractHintType.KeyF:
				keyFObj.SetActive(true);
				break;
		}
	}
}
