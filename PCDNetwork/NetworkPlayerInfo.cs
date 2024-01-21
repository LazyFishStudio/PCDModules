using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class NetworkPlayerInfo : NetworkBehaviour
{
	public override void OnNetworkSpawn() {
		string playerName = "Player" + OwnerClientId;
		GetComponent<PCDPlayerMovementInput>().playerName = playerName;
		GetComponent<PCDPlayerInteractionManager>().playerName = playerName;

		if (IsOwner) {
			FindObjectOfType<CinemachineVirtualCamera>().Follow = transform;
			FindObjectOfType<PCDPlayerInput>().playerName = playerName;
		}
	}
}
