using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasyAreaEvent : SingletonMono<EasyAreaEvent>
{
	static public void RegisterAreaEvent(string eventName, GameObject target, GameObject area, bool checkStay = false) {
		RegisterAreaEvent(eventName, target.GetComponent<Collider>(), area.GetComponent<Collider>(), checkStay);
	}
	static public void RegisterAreaEvent(string eventName, Collider target, Collider area, bool checkStay = false) {
		if (!target)
			Debug.LogError("Target of AreaEvent is Invalid");
		RegisterAreaEvent(eventName, area, (collider) => collider == target, checkStay);
	}
	static public void RegisterAreaEvent(string eventName, Collider area, Func<Collider, bool> checker, bool checkStay = false) {
		if (!area)
			Debug.LogError("Area of AreaEvent is Invalid");
		if (!area.isTrigger)
			Debug.LogError("Area of AreaEvent is not trigger");
		if (checker == null)
			Debug.LogError("Checker of AreaEvent is Invalid");
		AreaEventHelper helper = area.gameObject.AddComponent<AreaEventHelper>();
		helper.SetupHelper(eventName, checker, checkStay);
	}
	static public async Task WaitForAreaEvent(string eventName, string type) {
		await EasyEvent.WaitForEvent(string.Format("AreaEvent[{0}]{1}", eventName, type));
	}

	private class AreaEventHelper : MonoBehaviour
	{
		private string eventName;
		private bool checkStay;
		private Func<Collider, bool> checker;
		public void SetupHelper(string eventName, Func<Collider, bool> checker, bool checkStay) {
			this.eventName = eventName;
			this.checker = checker;
			this.checkStay = checkStay;
		}
		private void OnTriggerEnter(Collider other) {
			if (checker != null && checker.Invoke(other))
				EasyEvent.TriggerEvent(string.Format("AreaEvent[{0}]Enter", eventName));
		}
		private void OnTriggerExit(Collider other) {
			if (checker != null && checker.Invoke(other))
				EasyEvent.TriggerEvent(string.Format("AreaEvent[{0}]Exit", eventName));
		}
		private void OnTriggerStay(Collider other) {
			if (checkStay && checker != null && checker.Invoke(other))
				EasyEvent.TriggerEvent(string.Format("AreaEvent[{0}]Exit", eventName));
		}
	}
}
