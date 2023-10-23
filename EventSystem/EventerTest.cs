using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace PCD.EventSystem
{
    public class EventerTest : MonoBehaviour, PCDSimpleListener, MMEventListener<PCDGenericEvent>, MMEventListener<MoveAreaEvent>, MMEventListener<InteractEvent>
	{
		public void OnMMEvent(MoveAreaEvent moveAreaEvent) {
			Debug.Log(moveAreaEvent.eventName + ": " + moveAreaEvent.character.gameObject.name + " " + moveAreaEvent.area.gameObject.name);
		}

		public void OnMMEvent(InteractEvent interactEvent) {
			Debug.Log(interactEvent.eventName + ": " + interactEvent.interactor.name + " " + interactEvent.interactType.ToString() + " " + interactEvent.target.name);
		}

		public void OnMMEvent(PCDGenericEvent genericEvent) {
			Debug.Log(genericEvent.eventName);
		}
	}
}
