using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IVelocitySyncer
{
    public Vector3 GetCharacterVelocity();
}

public class RBVelocitySyncer : MonoBehaviour, IVelocitySyncer
{
    public Rigidbody rb;

    public Vector3 GetCharacterVelocity() {
        return rb.velocity;
	}
}