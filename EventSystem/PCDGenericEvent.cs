using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCD.EventSystem
{
    public struct PCDGenericEvent
    {
        public string eventName;
        public PCDGenericEvent(string eventName) {
            this.eventName = eventName;
        }
    }
}

