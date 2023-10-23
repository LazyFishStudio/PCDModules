using System.Collections;
using System.Collections.Generic;
using PhysicsBasedCharacterController;
using Unity.VisualScripting;
using UnityEditor.Rendering.Universal;
using UnityEngine;

[RequireComponent(typeof(CharacterManager))]
[RequireComponent(typeof(InputReader))]
public class PCDHumanMovementSM : MonoBehaviour {
    public enum State {
        Idle, Walk, Run,
    }

    public float walkSpeed = 6.0f;
    public float runSpeed = 7.0f;
    public MoveInput moveInput;
    [SerializeField]
    private StateMachine<State> sm;
    private CharacterManager charaM;
    private InputReader inputReader;
    private PCDHuman human;

    void Awake() {
        charaM = GetComponent<CharacterManager>();
        inputReader = GetComponent<InputReader>();
        human = GetComponentInChildren<PCDHuman>();

        InitMoveSM();
    }
    
    void InitMoveSM() {

        sm = new StateMachine<State>(State.Idle);

        sm.GetState(State.Idle).Bind(
            () => {
                human.SetPoseLayerIndex(0);
            },
            () => {
                SetControllerInput_Idle();
            },
            () => {}
        );

        sm.GetState(State.Walk).Bind(
            () => {
                charaM.movementSpeed = walkSpeed;
                human.SetPoseLayerIndex(0);
            },
            () => {
                SetControllerInput_Move();
            },
            () => {}
        );

        sm.GetState(State.Run).Bind(
            () => {
                human.SetPoseLayerIndex(1);
                charaM.sprintSpeed = runSpeed;
                inputReader.sprint = true;
            },
            () => {
                SetControllerInput_Move();
            },
            () => {
                inputReader.sprint = false;
            }
        );

        sm.Init();

    }

    public void update() {
        HandleMovementInput();
        sm.UpdateStateAction();
    }

    void LateUpdate() {
        // moveInput.Init();
    }

    [System.Serializable]
    public class MoveInput {
        public Vector3 moveAxis;
        public bool run;
        public bool haveMoveInput => moveAxis.magnitude > 0.001f;
        
        public void Init() {
            moveAxis = Vector2.zero;
            run = false;
        }
    }

    private void HandleMovementInput() {

        if (!moveInput.haveMoveInput) {
            sm.GotoState(State.Idle);
            return;
        }

        if (moveInput.run) {
            sm.GotoState(State.Run);
            return;
        }
        
        sm.GotoState(State.Walk);

    }

    private void SetControllerInput_Idle() {
        inputReader.axisInput = Vector2.zero;
    }

    private void SetControllerInput_Move() {
        inputReader.axisInput = moveInput.moveAxis;
    }

}
