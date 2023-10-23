using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pullable_SnailShell : PullableObject {
    public Transform shell;
    public override void OnPulledOut() {
        if (pulledOutEffect)
            GameObject.Instantiate(pulledOutEffect, transform.position, Quaternion.identity);
        GetComponentInParent<PullablePCDIKController>(true)?.SetFollowTargetOverride(null);
        if (shell) {
            GameObject.Destroy(shell.gameObject);
        }
    }
}
