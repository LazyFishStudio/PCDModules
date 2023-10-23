using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
	[RequireComponent(typeof(PCDFocusable))]
	public class ItemSlot : MonoBehaviour, IFocusable, IPlaceSlot
	{
		public IPlaceable holdItem;

		public Action<IPlaceSlot, IPlaceable> acceptItemCallbacks { get; set; }
		public Action<IPlaceSlot, IPlaceable> removeItemCallbacks { get; set; }

		public List<IPlaceable> GetAllAttachedItems() {
			var res = new List<IPlaceable>();
			if (holdItem != null)
				res.Add(holdItem);
			return res;
		}

		public virtual bool CheckAcceptItem(InteractComp interactor, IPlaceable item) {
			if (item == null)
				return false;

			if (holdItem != null)
				return false;

			if (item.attachedPlace != null)
				return false;

			return true;
		}

		public virtual void OnAcceptItemCallback(InteractComp interactor, IPlaceable item) {
			holdItem = item;
			
			/* Deal with different type of items */
			if (item is Component component) {
				if (component.GetComponent<Collider>()) {
					component.GetComponent<Collider>().enabled = false;
				}
				if (component.GetComponent<Rigidbody>()) {
					component.GetComponent<Rigidbody>().isKinematic = true;
				}
				component.transform.position = transform.position;
				component.transform.SetParent(transform);
			}

			acceptItemCallbacks?.Invoke(this, item);
		}

		public virtual void OnRemoveItemCallback(IPlaceable item) {
			holdItem = null;

			/* Deal with different type of items */
			if (item is Component component) {
				if (component.GetComponent<Collider>()) {
					component.GetComponent<Collider>().enabled = true;
				}
				if (component.GetComponent<Rigidbody>()) {
					component.GetComponent<Rigidbody>().isKinematic = false;
				}
				component.transform.SetParent(null);
			}

			removeItemCallbacks?.Invoke(this, item);
		}

		public virtual bool CheckFocusCond(InteractionManager manager) {
			var interactor = manager.interactComp;
			if (interactor.holdingItem == null)
				return false;
			IPlaceable placeable = interactor.holdingItem.GetComponent<IPlaceable>();
			return placeable != null && CheckAcceptItem(interactor, placeable);
		}
	}
}
