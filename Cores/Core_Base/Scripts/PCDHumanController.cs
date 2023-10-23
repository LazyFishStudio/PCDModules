using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDHumanController : MonoBehaviour
{
    public PCDHuman humanAnim;
    public Rigidbody rb;
    // Start is called before the first frame update
    void Awake() {
        humanAnim = GetComponent<PCDHuman>();
        rb = GetComponentInParent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        humanAnim.poseInfo.velocity = rb.velocity;
    }
}
