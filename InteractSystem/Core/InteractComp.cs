using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
    public abstract class InteractComp : MonoBehaviour
    {
        public GameObject holdingItem;
        /// <summary>
        /// It's not necessary, just recommended to use.
        /// </summary>
        public virtual bool Pick(IPickable pickable) {
            return pickable.PickedBy(this);
        }
        public virtual bool Drop() {
            return holdingItem.GetComponent<IPickable>().DroppedBy(this);
		}
    }
}
