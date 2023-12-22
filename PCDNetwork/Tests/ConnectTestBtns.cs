using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ConnectTestBtns : MonoBehaviour
{
    public Button createHostBtn;
    public Button createServerBtn;
    public Button createClientBtn;

	private void Awake() {
		createHostBtn.onClick.AddListener(() => { NetworkManager.Singleton.StartHost(); });
		createServerBtn.onClick.AddListener(() => { NetworkManager.Singleton.StartServer(); });
		createClientBtn.onClick.AddListener(() => { NetworkManager.Singleton.StartClient(); });
	}
}
