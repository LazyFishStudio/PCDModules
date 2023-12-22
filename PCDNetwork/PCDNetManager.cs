using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Cinemachine;

public class PCDNetManager : NetworkBehaviour
{
    public ulong clientId;
    public string playerName;
    public GameObject playerPrefab;
    public GameObject player;

    public override void OnNetworkSpawn() {
        if (!IsOwner) return;

        clientId = OwnerClientId;
        playerName = "Player" + clientId;

        Debug.Log(transform.name + "Inst Player: " + playerName);
        player = Instantiate(playerPrefab);
        player.GetComponent<NetworkObject>().Spawn();

        FindObjectOfType<CinemachineVirtualCamera>().Follow = player.transform;
        FindObjectOfType<PCDPlayerInput>().playerName = playerName;
    }
}
