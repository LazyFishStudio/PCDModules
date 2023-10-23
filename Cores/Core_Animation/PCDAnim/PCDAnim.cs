using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PCDAnim : MonoBehaviour {
    public PCDHuman human;
    void Awake() {
        human = GetComponentInParent<PCDHuman>();
    }

    public virtual void StartAnim() {

    }

    public virtual void StopAnim() {

    }

}
