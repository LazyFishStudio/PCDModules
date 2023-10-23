using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using System.Linq;

namespace PCD.EventSystem
{
	public interface PCDSimpleListener { }

	public class SimpleListenerManager : MonoBehaviour, MMEventListener<PCDGenericEvent>, MMEventListener<MoveAreaEvent>, MMEventListener<InteractEvent>
	{
		static System.Type[] eventTypes = { typeof(PCDGenericEvent), typeof(MoveAreaEvent), typeof(InteractEvent) };

		static public SimpleListenerManager Inst;
		private void Awake() {
			if (Inst != null)
				throw new System.Exception("More than one SimpleListenerManager exits!");
			Inst = this;
		}

		public Dictionary<System.Type, List<Object>> listenerDict;
		private void Start() {
			listenerDict = new Dictionary<System.Type, List<Object>>();
			foreach (var type in eventTypes)
				listenerDict[type] = new List<Object>();

			List<PCDSimpleListener> allListeners = new List<PCDSimpleListener>();
			foreach (MonoBehaviour comp in FindObjectsOfType<MonoBehaviour>(true)) {
				if (comp is PCDSimpleListener listener)
					allListeners.Add(listener);
			}
			foreach (var listener in allListeners) {
				RegisterSimpleListener(listener);
			}
		}

		static public void RegisterSimpleListener(PCDSimpleListener listener) {
			foreach (var type in eventTypes) {
				System.Type listenerType = typeof(MMEventListener<>).MakeGenericType(type);
				if (listenerType.IsInstanceOfType(listener)) {
					Inst.listenerDict[type].Add(listener as Object);
				}
			}
		}

		static private bool IsListenerActive(Object obj) {
			return obj != null && !obj.Equals(null) && (obj as MonoBehaviour).enabled;
		}

		public void OnMMEvent(PCDGenericEvent genericEvent) {
			foreach (var listener in listenerDict[typeof(PCDGenericEvent)]) {
				if (IsListenerActive(listener)) {
					(listener as MMEventListener<PCDGenericEvent>).OnMMEvent(genericEvent);
				}
			}
		}

		public void OnMMEvent(MoveAreaEvent moveAreaEvent) {
			foreach (var listener in listenerDict[typeof(MoveAreaEvent)]) {
				if (IsListenerActive(listener)) {
					(listener as MMEventListener<MoveAreaEvent>).OnMMEvent(moveAreaEvent);
				}
			}
		}

		public void OnMMEvent(InteractEvent interactEvent) {
			foreach (var listener in listenerDict[typeof(InteractEvent)]) {
				if (IsListenerActive(listener)) {
					(listener as MMEventListener<InteractEvent>).OnMMEvent(interactEvent);
				}
			}
		}

		public void OnEnable() {
			this.MMEventStartListening<MoveAreaEvent>();
			this.MMEventStartListening<InteractEvent>();
			this.MMEventStartListening<PCDGenericEvent>();
		}

		public void OnDisable() {
			this.MMEventStopListening<MoveAreaEvent>();
			this.MMEventStopListening<InteractEvent>();
			this.MMEventStopListening<PCDGenericEvent>();
		}
	}
}

