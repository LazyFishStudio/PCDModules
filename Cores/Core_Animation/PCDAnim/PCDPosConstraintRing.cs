using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDPosConstraintRing : MonoBehaviour {
    public Transform circleCenter; // 圆环中心
    public float radius = 5.0f; // 圆环半径
    public Vector2 angleRange = new Vector2(0, 360); // 角度范围
    public float angularVelocity; // 角速度（度/秒）

    private Rigidbody rb;
    private float lastAngle = 0.0f; // 上一帧的角度

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        // 将刚体当前位置转换为极坐标
        Vector3 offset = rb.position - circleCenter.position;
        float currentAngle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
        float distance = offset.magnitude;

        // 角度标准化
        currentAngle = NormalizeAngle(currentAngle);

        // 计算圆环上的目标点
        if (currentAngle >= angleRange.x && currentAngle <= angleRange.y)
        {
            float targetAngle = Mathf.Deg2Rad * currentAngle;
            Vector3 targetPosition = circleCenter.position + new Vector3(Mathf.Cos(targetAngle), 0, Mathf.Sin(targetAngle)) * radius;

            // 计算需要应用的力
            Vector3 forceDirection = (targetPosition - rb.position).normalized;
            rb.AddForce(forceDirection * (Mathf.Abs(radius - distance) * rb.mass));

            // 校正刚体方向以使其沿圆环切线方向运动
            Vector3 tangentDirection = new Vector3(-Mathf.Sin(targetAngle), 0, Mathf.Cos(targetAngle));
            rb.rotation = Quaternion.LookRotation(tangentDirection);

            // 计算角速度
            angularVelocity = CalculateAngularVelocity(currentAngle, lastAngle);
            lastAngle = currentAngle;
        }
        // 如果超出范围，则不应用力，使刚体停止移动
    }

    private float NormalizeAngle(float angle)
    {
        while (angle < 0)
        {
            angle += 360;
        }
        while (angle > 360)
        {
            angle -= 360;
        }
        return angle;
    }

    private float CalculateAngularVelocity(float currentAngle, float lastAngle)
    {
        float angleDifference = currentAngle - lastAngle;
        // 确保角度差为最小正值
        if (angleDifference < -180)
        {
            angleDifference += 360;
        }
        else if (angleDifference > 180)
        {
            angleDifference -= 360;
        }
        return angleDifference / Time.fixedDeltaTime;
    }
}
