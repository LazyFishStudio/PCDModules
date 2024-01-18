
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InteractSystem
{
    public class InteractLogger
    {
        /* 仅作为参考，实际使用时 Log 跟 Listener 能对得上就行 */
        static public string[] basicEventTypes = {
            "Pick", "Drop", "Throw", "Attack", "Attacked", "FocusEnter", "FocusExit",
            "PlaceItem", "RemoveItem"
        };

        static public Action<InteractEvent> interactLogListeners;

        static public void LogInteractEvent(string eventType, GameObject obj1, GameObject obj2) {
            interactLogListeners?.Invoke(new InteractEvent(eventType, obj1, obj2));
        }
    }

    public struct InteractEvent
    {
        public string eventType;
        public GameObject obj1;
        public GameObject obj2;

        public InteractEvent(string eventType, GameObject obj1, GameObject obj2) {
            this.eventType = eventType;
            this.obj1 = obj1;
            this.obj2 = obj2;
        }
    }
}
