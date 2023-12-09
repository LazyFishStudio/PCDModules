using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDBone : MonoBehaviour
{
    public string boneName;
    public PCDBoneDriver owner;
    public bool forceOwner;

    public void SetOwnership(PCDBoneDriver driver, bool force = false) {
        owner = driver;
        forceOwner = force;
    }
    public void ResetOwnership() {
        owner = null;
        forceOwner = false;
    }
}
