using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
    public interface IPlaceSlot {
        public Action<IPlaceSlot, IPlaceable> acceptItemCallbacks { get; set; }
        public Action<IPlaceSlot, IPlaceable> removeItemCallbacks { get; set; }

        public List<IPlaceable> GetAllAttachedItems();
        public bool CheckAcceptItem(InteractComp interactor, IPlaceable item);
        public void OnAcceptItemCallback(InteractComp interactor, IPlaceable item);
        public void OnRemoveItemCallback(IPlaceable item);
    }
}
