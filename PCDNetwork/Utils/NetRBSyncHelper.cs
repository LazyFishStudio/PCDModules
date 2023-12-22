using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetRBSyncHelper : MonoBehaviour, IVelocitySyncer
{
    public NetRBSyncer rbSyncer;

    public Vector3 GetCharacterVelocity() {
		return rbSyncer.GetCharacterVelocity();
	}
}
