using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Bros.Utils;

namespace InteractSystem
{
    [RequireComponent(typeof(InteractComp))]
    public class BaseInteractionManager : InteractionManager
	{
        public float focusDist = 3f;
        public float focusDegree = 90f;
        public float thorwForce = 20f;

		protected virtual void Update() {
            FreshFocus();
        }

        private void FreshFocus() {
            var nearestFocus = UtilClass.FindBest<Focusable>(FindObjectsOfType<Focusable>(), (candidate) => {
                if (!candidate.FreshAndCheckFocusComps(this))
                    return float.NegativeInfinity;
                return -candidate.GetDistClearY(this);
            });

            if (focusing == nearestFocus) {
                if (focusing != null)
                    focusing.OnFocusStay(this);
            } else {
                if (focusing != null)
                    focusing.OnFocusExit(this);
                focusing = nearestFocus;
                if (focusing != null)
                    focusing?.OnFocusEnter(this);
            }
        }

        public override bool CheckCommonFocusCond(Focusable focusable) {
            Vector3 diff = (focusable.transform.position - transform.position);
            if (Mathf.Abs(diff.y) > 4.5f)
                return false;
            diff = diff.ClearY();
            Vector3 forward = transform.forward.ClearY();

            CapsuleCollider collider = GetComponent<CapsuleCollider>();
            if (collider != null)
                diff += forward.normalized * collider.radius;
            float degree = Mathf.Abs(Vector3.Angle(diff, forward));
            return diff.magnitude <= focusDist && degree <= focusDegree;
        }
    }
}
