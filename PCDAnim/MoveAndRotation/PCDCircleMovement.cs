using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

public class PCDCircleMovement : MonoBehaviour {
    public bool destoryOnFinish;
    public Transform posHandle;
    public float movementDuration = 0.5f;
    [Range(0, 1.0f)]
    public float process = 0;
    public float processWithCurve => processCurve.Evaluate(process);
    public AnimationCurve processCurve;
    public Transform followPos;
    public Transform followRot;
    public Transform movingObj;
    public AnimationCurve targetLengthMultCurve;
    public float targetLength = 1.0f;
    public float targetLengthWithCurve => targetLength * targetLengthMultCurve.Evaluate(process);
    public float pitchAngle;
    public float rollAngle;
    public Vector2 yawAngleRange;
    public bool isStartMovement;
    private float movementDurationCount;
    private UnityAction animFinishCallBack;
    public List<ProcessEvent> processEventList;

    void Update() {

        if (isStartMovement) {
            movementDurationCount += Time.deltaTime;
            process = Mathf.Min(1.0f, movementDurationCount / movementDuration);
            movingObj.transform.position = transform.position + transform.rotation * (Quaternion.AngleAxis(rollAngle, Vector3.forward) 
                * Quaternion.AngleAxis(Mathf.LerpUnclamped(yawAngleRange.x, yawAngleRange.y, processWithCurve), Vector3.down)) * Vector3.forward * targetLengthWithCurve;
            movingObj.transform.rotation = Quaternion.LookRotation(movingObj.transform.position - transform.position, transform.rotation * (Quaternion.AngleAxis(rollAngle, Vector3.forward) * Vector3.up));
            movingObj.transform.rotation = Quaternion.AngleAxis(pitchAngle, transform.right) * movingObj.transform.rotation;
            
            // transform.position = followPos.position;
            if (followRot)
                transform.rotation = followRot.rotation;
            
            if (movementDurationCount >= movementDuration) {
                isStartMovement = false;
                movementDurationCount = 0;
                animFinishCallBack?.Invoke();
                OnMovementExit();
            }

            HandleProcessEvent();
        }

    }
    
    public void StartMovement(Transform followPos, Transform followRot, UnityAction animFinishCallBack = null) {
        this.followPos = null;
        this.followRot = null;
        this.followPos = followPos;
        this.followRot = followRot;
        // transform.position = followPos.position;
        this.animFinishCallBack = animFinishCallBack;
        isStartMovement = true;
        // movingObj.transform.position = transform.position + transform.rotation * (Quaternion.AngleAxis(rollAngle, Vector3.forward) * Quaternion.AngleAxis(Mathf.LerpUnclamped(yawAngleRange.x, yawAngleRange.y, 0), Vector3.down)) * Vector3.forward * targetLengthWithCurve;
        // movingObj.transform.rotation = Quaternion.LookRotation(movingObj.transform.position - transform.position, transform.rotation * (Quaternion.AngleAxis(rollAngle, Vector3.forward) * Vector3.up));

        movingObj.transform.position = transform.position + transform.rotation * (Quaternion.AngleAxis(rollAngle, Vector3.forward) 
            * Quaternion.AngleAxis(Mathf.LerpUnclamped(yawAngleRange.x, yawAngleRange.y, 0), Vector3.down)) * Vector3.forward * targetLengthWithCurve;
        movingObj.transform.rotation = Quaternion.LookRotation(movingObj.transform.position - transform.position, transform.rotation * (Quaternion.AngleAxis(rollAngle, Vector3.forward) * Vector3.up));
        movingObj.transform.rotation = Quaternion.AngleAxis(pitchAngle, transform.right) * movingObj.transform.rotation;
        OnMovementEnter();
        Debug.Log("Start Movement: " + isStartMovement);

    
    }

    public void StopMovement() {
        
        isStartMovement = false;

    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        for (int i = 0; i < 10; i++) {
            Gizmos.DrawLine(transform.position, transform.position + transform.rotation * (Quaternion.AngleAxis(rollAngle, Vector3.forward) 
            * Quaternion.AngleAxis(Mathf.Lerp(yawAngleRange.x, yawAngleRange.y, processCurve.Evaluate(i / 10.0f)), Vector3.down)) * Vector3.forward * targetLength * targetLengthMultCurve.Evaluate(i / 10.0f));
        }
        for (int i = 200; i < 300; i++) {
            Gizmos.DrawLine(transform.position, transform.position + transform.rotation * (Quaternion.AngleAxis(rollAngle, Vector3.forward) 
            * Quaternion.AngleAxis(Mathf.Lerp(yawAngleRange.x, yawAngleRange.y, processCurve.Evaluate(i / 500.0f)), Vector3.down)) * Vector3.forward * targetLength * targetLengthMultCurve.Evaluate(i / 500.0f));
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.rotation * (Quaternion.AngleAxis(rollAngle, Vector3.forward) 
            * Quaternion.AngleAxis(yawAngleRange.y, Vector3.down)) * Vector3.forward * targetLength * targetLengthMultCurve.Evaluate(1.0f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + transform.rotation * (Quaternion.AngleAxis(rollAngle, Vector3.forward) 
            * Quaternion.AngleAxis(Mathf.Lerp(yawAngleRange.x, yawAngleRange.y, processWithCurve), Vector3.down)) * Vector3.forward * targetLengthWithCurve);

            // target.transform.position = transform.position + transform.rotation * (Quaternion.AngleAxis(rollAngle, Vector3.forward) 
            //     * Quaternion.AngleAxis(Mathf.LerpUnclamped(yawAngleRange.x, yawAngleRange.y, processWithCurve), Vector3.down)) * Vector3.forward * targetLengthWithCurve;
    }

    private void OnMovementEnter() {
        InitAllProcessEvent();
    }

    private void OnMovementExit() {
        if (destoryOnFinish) {
            if (posHandle) {
                GameObject.Destroy(posHandle.gameObject);
            } else {
                GameObject.Destroy(gameObject);
            }
        }
    }

    private void InitAllProcessEvent() {
        if (processEventList != null)
            for (int i = 0; i < processEventList.Count; i++) {
                ProcessEvent processEvent = processEventList[i];
                    processEvent.isTriggerThisMovement = false;
            }
    }

    private void HandleProcessEvent() {
        if (processEventList != null)
            for (int i = 0; i < processEventList.Count; i++) {
                ProcessEvent processEvent = processEventList[i];
                if (process >= processEvent.triggerProcess && !processEvent.isTriggerThisMovement) {
                    // LogTriggerEvent();
                    processEvent.eventAction?.Invoke();
                    processEvent.isTriggerThisMovement = true;
                }
            }
    }

    public void AddProcessEvent(float triggerProcess, UnityEvent eventAction) {
        if (processEventList == null) {
            processEventList = new();
        }
        processEventList.Add(new ProcessEvent(triggerProcess, eventAction));
    }

    public void ClearAllProcessEvent() {
        if (processEventList != null) {
            processEventList.Clear();
        }
    }

    public void LogTriggerEvent() {
        Debug.Log("Trigger Event!");
    }

    public void SendAttackStartMessageUpwards() {
        SendMessageUpwards("TriggerAttackingWeaponStartEvent", SendMessageOptions.DontRequireReceiver);
    }

    public void SendAttackEndMessageUpwards() {
        SendMessageUpwards("TriggerAttackingWeaponEndEvent", SendMessageOptions.DontRequireReceiver);
    }
    

    [System.Serializable]
    public class ProcessEvent {
        public bool isTriggerThisMovement;
        public float triggerProcess;
        public UnityEvent eventAction;

        public ProcessEvent(float triggerProcess, UnityEvent eventAction) {
            this.triggerProcess = triggerProcess;
            this.eventAction = eventAction;
        }
    }

}
