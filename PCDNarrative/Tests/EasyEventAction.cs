using UnityEngine;
using NodeCanvas.Framework;

public class EasyEventAction : ActionTask
{
	public string eventName;

	protected override void OnExecute() {
		Debug.Log("My agent is " + agent.name + " Event is " + eventName);
		EasyEvent.TriggerEvent(eventName);
		EndAction(true);
	}
}

public class EasyEventWaitAction : ActionTask
{
	public string eventName;

	protected override void OnExecute() {
		EasyEvent.RegisterOnceCallback(eventName, () => {
			EndAction(true);
		});
	}
}