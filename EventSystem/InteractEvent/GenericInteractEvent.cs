using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCD.EventSystem
{
	public class GenericInteractEvent : MonoBehaviour
	{
		public string eventName;
		public InteractType interactType;
		public GameObject interactor;
		public GameObject target;

		private void Start() {
			InteractEventConfig config = new InteractEventConfig(eventName, interactType, interactor, target);
			config.AddCondition(EventCondition);
			InteractEventer.RegisterEvent(config);
		}

		protected virtual bool EventCondition(InteractSystem.InteractEvent interactEvent) {
			return true;
		}
	}
}
