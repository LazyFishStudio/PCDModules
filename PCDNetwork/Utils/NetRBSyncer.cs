using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class NetRBSyncer : NetworkBehaviour
{
    public NetworkVariable<Vector3> velocity = new NetworkVariable<Vector3>(Vector3.zero, writePerm: NetworkVariableWritePermission.Owner);
	private Rigidbody rb;

	public Vector3 GetCharacterVelocity() {
		if (!IsSpawned) return Vector3.zero;
		if (IsOwner) {
			if (rb == null) rb = GetComponent<Rigidbody>();
			velocity.Value = rb.velocity;
		}
		return velocity.Value;
	}

	public void Update() {
		GetCharacterVelocity();
	}
}
