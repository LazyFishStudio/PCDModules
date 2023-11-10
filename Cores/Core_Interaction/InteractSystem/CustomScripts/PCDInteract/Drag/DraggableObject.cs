using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DraggableObject : MonoBehaviour {
    public Transform dragger;
    public Transform dragPoint;
    public float dragDistance = 3f; // 拖动的最大距离
    public float dragStrength = 800f; // 弹力的强度
    public bool rotateToDragger = true;
    public float rotateSpeed = 5.0f;
    private Rigidbody rb;
    private Quaternion targetRot;

    void Awake() {
        rb = GetComponent<Rigidbody>();
    }
    
    void Update() {

        if (!dragger) {
            return;
        }

        Vector3 playerPosition = dragger.transform.position;

        // 计算物体到玩家的距离
        float distanceToPlayer = Vector3.Distance(transform.position, playerPosition);

        // 如果距离大于最大拖动距离，启用弹力效果
        if (distanceToPlayer > dragDistance) {
            // 计算拖动方向
            Vector3 dragDirection = (playerPosition - transform.position).ClearY().normalized;
            // 计算弹力的大小
            float dragForce = (distanceToPlayer - dragDistance) * dragStrength;
            // 应用弹力
            rb?.AddForceAtPosition(dragDirection * dragForce * Mathf.Min(Time.deltaTime, 0.0125f), dragPoint.position, ForceMode.Acceleration);

            if (rotateToDragger) {
                targetRot = Quaternion.LookRotation((dragger.transform.position - transform.position).ClearY(), Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
            }
        }

    }

    public void StartDragBy(Transform dragger) {
        this.dragger = dragger;
    }

    public void RestDrag() {
        dragger = null;
    }

    
}
