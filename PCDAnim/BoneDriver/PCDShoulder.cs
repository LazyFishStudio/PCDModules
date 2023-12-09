using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDShoulder : PCDBoneDriver
{
	private Transform shoulder;
	private Transform hand;

	public PCDShoulder(PCDBone shoulder, PCDBone hand, bool autoTryGetOwnship = false) : base(shoulder, autoTryGetOwnship) {
		this.shoulder = shoulder.transform;
		this.hand = hand.transform;
	}

	public void UpdateShoulder() {
		var rotation = Quaternion.LookRotation((hand.position - shoulder.position).normalized, hand.up);
		SetGlobalRotation(rotation);
	}
}
