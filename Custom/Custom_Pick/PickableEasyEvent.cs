using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
    public class PickableEasyEvent : PickableObject
    {
		public bool withInteractorName = false;
		public string pickEventName;
		public string dropEventName;
		public string placeEventName;
		public string removeEventName;

		public override void OnPickedBy(InteractComp interactor) {
			base.OnPickedBy(interactor);
			if (pickEventName != null && pickEventName != "")
				EasyEvent.TriggerEvent(pickEventName);
		}

		public override void OnDroppedBy(InteractComp interactor) {
			base.OnDroppedBy(interactor);
			if (dropEventName != null && dropEventName != "")
				EasyEvent.TriggerEvent(dropEventName);
		}

		public override void OnPlaceTo(IPlaceSlot slot) {
			base.OnPlaceTo(slot);
			if (placeEventName != null && placeEventName != "")
				EasyEvent.TriggerEvent(placeEventName);
		}

		public override void OnRemoveFrom(InteractComp interactor) {
			base.OnRemoveFrom(interactor);
			if (removeEventName != null && removeEventName != "")
				EasyEvent.TriggerEvent(removeEventName);
		}
	}
}
