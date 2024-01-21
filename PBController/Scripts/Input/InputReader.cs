#if false

using UnityEngine;
using UnityEngine.Events;

namespace PhysicsBasedCharacterController
{
    public class InputReader : MonoBehaviour
    {
        [Header("Input specs")]
        public UnityEvent changedInputToMouseAndKeyboard;
        public UnityEvent changedInputToGamepad;

        [Header("Enable inputs")]
        public bool enableJump = true;
        public bool enableCrouch = true;
        public bool enableSprint = true;

        // [HideInInspector]
        public Vector2 axisInput;
        [HideInInspector]
        public Vector2 cameraInput = Vector2.zero;
        [HideInInspector]
        public bool jump;
        [HideInInspector]
        public bool jumpHold;
        [HideInInspector]
        public float zoom;
        [HideInInspector]
        public bool sprint;
        [HideInInspector]
        public bool crouch;


        private bool hasJumped = false;
        private bool skippedFrame = false;
        private bool isMouseAndKeyboard = true;
        private bool oldInput = true;


        //ENABLE if using old input system
        private void Update()
        {

            if (enableJump)
            {
                if (Input.GetButtonDown("Jump")) OnJump();
                if (Input.GetButtonUp("Jump")) JumpEnded();
            }

            if (enableCrouch) crouch = Input.GetButton("Fire1");

            GetDeviceOld();
        }


        


        //ENABLE if using old input system
        private void GetDeviceOld()
        {
            oldInput = isMouseAndKeyboard;

            if (Input.GetJoystickNames().Length > 0) isMouseAndKeyboard = false;
            else isMouseAndKeyboard = true;

            if (oldInput != isMouseAndKeyboard && isMouseAndKeyboard) changedInputToMouseAndKeyboard.Invoke();
            else if (oldInput != isMouseAndKeyboard && !isMouseAndKeyboard) changedInputToGamepad.Invoke();
        }


        #region Actions


        public void OnJump()
        {
            if (enableJump)
            {
                jump = true;
                jumpHold = true;

                hasJumped = true;
                skippedFrame = false;
            }
        }


        public void JumpEnded()
        {
            jump = false;
            jumpHold = false;
        }



        private void FixedUpdate()
        {
            if (hasJumped && skippedFrame)
            {
                jump = false;
                hasJumped = false;
            }
            if (!skippedFrame && enableJump) skippedFrame = true;
        }


        

        #endregion
    }
}

#endif