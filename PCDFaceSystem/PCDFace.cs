using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Drawing;
using Unity.Mathematics;

[ExecuteInEditMode]
public class PCDFace : MonoBehaviour {
    public LayerMask faceRaycastLayer;
    public Rigidbody faceRb;
    public MeshCollider faceMeshColl;
    public bool facePosEditMode = true;
    [Range(0, 0.03f)]
    public float normalOffset = 0.02f;
    public Transform lEye;  // 目标物体的Transform
    public Transform rEye;  // 目标物体的Transform
    public EyeSetting eyeSetting;
    public Transform nose;  // 目标物体的Transform
    public EyeSetting noseSetting;
    public Transform mouth;  // 目标物体的Transform
    public EyeSetting mouthSetting;
    public Transform lEyebrow;
    public Transform rEyebrow;
    public EyeSetting eyebrowSetting;
    public Transform lBlusher;
    public Transform rBlusher;
    public EyeSetting blusherSetting;

    void Awake() {
        facePosEditMode = false;
        if (faceRb) {
            faceRb.isKinematic = false;
        }
        if (faceMeshColl) {
            faceMeshColl.enabled = false;
        }
    }

    void Update() {

        if (facePosEditMode) {
            UpdateFaceTransform();
        }

        SetEyeTexAndColorBySetting(lEye, eyeSetting);
        SetEyeTexAndColorBySetting(rEye, eyeSetting);
        SetEyeTexAndColorBySetting(nose, noseSetting);
        SetEyeTexAndColorBySetting(mouth, mouthSetting);
        SetEyeTexAndColorBySetting(lEyebrow, eyebrowSetting);
        SetEyeTexAndColorBySetting(rEyebrow, eyebrowSetting);

    }

    void OnValidate() {
        if (facePosEditMode) {
            if (faceRb) {
                faceRb.isKinematic = true;
            }
            if (faceMeshColl) {
                faceMeshColl.enabled = true;
            }
        } else {
            if (faceRb) {
                faceRb.isKinematic = false;
            }
            if (faceMeshColl) {
                faceMeshColl.enabled = false;
            }
        }
    }

    void UpdateFaceTransform() {

        UpdateEyeTransform(lEye, eyeSetting);
        UpdateEyeTransform(rEye, eyeSetting, false);
        UpdateEyeTransform(nose, noseSetting);
        UpdateEyeTransform(mouth, mouthSetting);
        UpdateEyeTransform(lEyebrow, eyebrowSetting);
        UpdateEyeTransform(rEyebrow, eyebrowSetting, false);

    }

    private void UpdateEyeTransform(Transform eye, EyeSetting eyeSetting, bool isLeft = true) {
        Vector3 eyeRayStartPoint = transform.position + (isLeft ? -1.0f : 1.0f) * transform.right * eyeSetting.spacing + transform.up * eyeSetting.height;
        Draw.SphereOutline(eyeRayStartPoint, 0.05f);
        Draw.Line(eyeRayStartPoint, eyeRayStartPoint + transform.forward);
        if (eye) {

            RaycastHit rayHit;
            if (Physics.Raycast(eyeRayStartPoint, transform.forward, out rayHit, 1.0f, faceRaycastLayer)) {
                // 设置左眼的位置为碰撞点
                eye.position = rayHit.point + rayHit.normal * normalOffset;
                // 设置左眼的朝向为碰撞点的法线
                eye.rotation = Quaternion.AngleAxis(isLeft ? eyeSetting.rollAngle.x : eyeSetting.rollAngle.y, rayHit.normal) * Quaternion.LookRotation(rayHit.normal, transform.up);
                Draw.SphereOutline(eye.position, 0.05f);
                Draw.Line(eye.position, eye.position + eye.forward * 0.1f);
            }
            eye.localScale = new Vector3(eyeSetting.scale.x, eyeSetting.scale.y, 1.0f);

        }
    }

    private void SetEyeTexAndColorBySetting(Transform eye, EyeSetting eyeSetting) {
        Renderer render = eye.GetComponentInChildren<Renderer>();
        if (PCDFaceTextureManager.GetInstance().faceMatNum != 0) {
            eyeSetting.textureIndex %= PCDFaceTextureManager.GetInstance().faceMatNum;
        }
        if (render) {
            if (PCDFaceTextureManager.GetInstance().faceMatNum > 0) {
                render.material = PCDFaceTextureManager.GetInstance().GetMatByIndex(eyeSetting.textureIndex);
            }
            #if UNITY_EDITOR
            if (Application.isPlaying) {
            #endif
                if (eyeSetting.color != Color.black) {
                    render.material.color = eyeSetting.color;
                }
            #if UNITY_EDITOR
            }
            #endif
        }
    }

    [System.Serializable]
    public class EyeSetting {
        public Color color = Color.black;
        public int textureIndex = 0;
        public float spacing = 0.05f;
        public float height = 0.1f;
        public Vector2 rollAngle;
        public Vector2 scale = Vector2.one;

    }
    

}
