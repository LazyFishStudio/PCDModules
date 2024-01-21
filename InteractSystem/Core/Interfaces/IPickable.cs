using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InteractSystem
{
    public interface IPickable
    {
        public bool CheckPickCond(InteractComp interactor);

        /// <summary>
        /// All things about pick item **MUST** be resolved inside.
        /// </summary>
        public bool PickedBy(InteractComp interactor);
        /// <summary>
        /// All things about drop item **MUST** be resolved inside.
        /// </summary>
        public bool DroppedBy(InteractComp interactor);
        public bool ThrownBy(InteractComp interactor);
    }
}
