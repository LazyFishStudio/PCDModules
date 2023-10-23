using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
	public abstract class Focusable : MonoBehaviour
	{
		[TextArea]
		public string focusHintContent;
		public bool CheckFocusCond(InteractionManager manager) {
			if (!manager.CheckCommonFocusCond(this))
				return false;
			return GetComponent<IFocusable>().CheckFocusCond(manager);
		}

		public abstract void OnFocusEnter(InteractionManager manager);
		public abstract void OnFocusStay(InteractionManager manager);
		public abstract void OnFocusExit(InteractionManager manager);
	}
}
