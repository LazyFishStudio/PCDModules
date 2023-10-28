using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class EasyEvent : SingletonMono<EasyEvent>
{
	/* Trigger methods */
	static public partial void TriggerEvent(string eventName);

	/* Easy form */
	static public partial EasyCallbackItem RegisterCallback(string eventName, Action callback);
	static public partial void RegisterOnceCallback(string eventName, Action callback);
	static public partial void RegisterAutoCallback(string eventName, Func<bool> checker);

	/* Async form */
	static public partial Task WaitForEvent(string eventName);
}

#if false
public partial class EasyEvent : SingletonMono<EasyEvent>
{
	/* Trigger methods */
	static public partial void TriggerEvent<T>(T easyEvent);

	/* Normal form */
	static public partial void AddCallback<T>(T easyEvent, Action<T> callback);
	static public partial void AddOnceCallback<T>(T easyEvent, Action<T> callback);
	static public partial void AddAutoCallback<T>(T easyEvent, Func<T, bool> checker);

	/* Async form */
	static public partial Task WaitForEvent<T>(Func<T, bool> checker);
}
#endif

#region Implement
public partial class EasyEvent : SingletonMono<EasyEvent>
{
	private Dictionary<string, List<EasyCallbackItem>> callbackDict;
	private void Awake() {
		callbackDict = new Dictionary<string, List<EasyCallbackItem>>();
	}
	private void AddCallbackItem(string eventName, EasyCallbackItem item) {
		if (callbackDict.TryGetValue(eventName, out List<EasyCallbackItem> list)) {
			list.Add(item);
		} else {
			callbackDict[eventName] = new List<EasyCallbackItem> { item };
		}
	}

	static public partial void TriggerEvent(string eventName) {
		// Debug.Log("EasyEvent triggered: " + eventName);

		if (GetInstance().callbackDict.TryGetValue(eventName, out List<EasyCallbackItem> list)) {
			List<EasyCallbackItem> pendingRemove = new List<EasyCallbackItem>();
			foreach (var item in list) {
				if (item.active) {
					item.callback?.Invoke();
					bool finished = (item.checker != null && item.checker.Invoke());
					if (finished || item.once)
						item.active = false;
				}
				if (!item.active)
					pendingRemove.Add(item);
			}
			foreach (var item in pendingRemove)
				list.Remove(item);
		}
	}

	static public partial EasyCallbackItem RegisterCallback(string eventName, Action callback) {
		EasyCallbackItem callbackItem = new EasyCallbackItem(callback, null, false);
		GetInstance().AddCallbackItem(eventName, callbackItem);
		return callbackItem;
	}
	static public partial void RegisterOnceCallback(string eventName, Action callback) {
		GetInstance().AddCallbackItem(eventName, new EasyCallbackItem(callback, null, true));
	}
	static public partial void RegisterAutoCallback(string eventName, Func<bool> checker) {
		GetInstance().AddCallbackItem(eventName, new EasyCallbackItem(null, checker, false));
	}

	static public partial async Task WaitForEvent(string eventName) {
		var eventTriggered = new TaskCompletionSource<bool>();
		RegisterOnceCallback(eventName, () => eventTriggered.SetResult(true));
		await eventTriggered.Task;
	}
}

#endregion

#region Utils
public interface IEasyCallbackItem
{
	public void RemoveSelf();
}

public class EasyCallbackItem : IEasyCallbackItem
{
	public bool active;
	public bool once;
	public Action callback;
	public Func<bool> checker;

	public EasyCallbackItem(Action callback, Func<bool> checker, bool once) {
		this.callback = callback;
		this.checker = checker;
		this.once = once;
		active = true;
	}

	public void RemoveSelf() {
		active = false;
	}
}
#endregion