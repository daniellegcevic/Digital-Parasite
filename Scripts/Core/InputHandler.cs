using UnityEngine;

public class InputHandler : MonoBehaviour
{
    #region Variables

        #region Singleton

            public static InputHandler instance = null;

        #endregion

        #region Main Input Data

            [HideInInspector] public bool leftMouseClicked;
            [HideInInspector] public bool rightMouseClicked;
            [HideInInspector] public bool controllerInteractClicked;
            [HideInInspector] public bool interactClicked;
            [HideInInspector] public bool escapeClicked;
            [HideInInspector] public Vector2 controllerInputVector;

        #endregion

        #region Camera Input Data

            [HideInInspector] public Vector2 mouseInputVector;
            [HideInInspector] public bool zoomClicked;
            [HideInInspector] public bool zoomReleased;
            [HideInInspector] public bool isZoomedIn;

        #endregion

        #region Movement Input Data

            [HideInInspector] public Vector2 movementInputVector;
            [HideInInspector] public bool isRunning;
            [HideInInspector] public bool runClicked;
            [HideInInspector] public bool runReleased;
            [HideInInspector] public bool isCrouching;
            [HideInInspector] public bool crouchClicked;
            [HideInInspector] public bool jumpClicked;

            [HideInInspector] public bool hasMovementInput;

        #endregion

        #region DEBUG

            [HideInInspector] public bool enableRunning = true;
            [HideInInspector] public bool enableCrouching = false;
            [HideInInspector] public bool enableJumping = false;
            [HideInInspector] public bool enableInput = true;

            private bool rightTriggerCheck = true;
            private bool rightTriggerPressed = false;
            private bool leftTriggerCheck = true;
            private bool leftTriggerPressed = false;

        #endregion

    #endregion

    #region Built-in Methods

        private void Awake()
        {
            if(instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            ResetInputData();
        }

        private void Update()
        {
            if(enableInput)
            {
                MainInputData();
                CameraInputData();
                MovementInputData();
            }
        }

    #endregion

    #region Custom Methods

        private void MainInputData()
        {
            if(rightTriggerCheck)
            {
                rightTriggerPressed = Input.GetAxis("Right Trigger") != 0f ? true : false;
                rightTriggerCheck = rightTriggerPressed ? false : true;
            }
            else
            {
                rightTriggerPressed = false;

                if(Input.GetAxis("Right Trigger") == 0f)
                {
                    rightTriggerCheck = true;
                }
            }
            
            leftMouseClicked = Input.GetButtonDown("Left Click") ? true : false;

            if(leftTriggerCheck)
            {
                leftTriggerPressed = Input.GetAxis("Left Trigger") != 0f ? true : false;
                leftTriggerCheck = leftTriggerPressed ? false : true;
            }
            else
            {
                leftTriggerPressed = false;

                if(Input.GetAxis("Left Trigger") == 0f)
                {
                    leftTriggerCheck = true;
                }
            }
            
            rightMouseClicked = Input.GetButtonDown("Right Click") ? true : false;

            controllerInteractClicked = Input.GetButtonDown("Controller Interact") || rightTriggerPressed;
            
            interactClicked = Input.GetButtonDown("Interact");

            escapeClicked = Input.GetButtonDown("Escape");

            controllerInputVector.x = Input.GetAxis("Controller X");
            controllerInputVector.y = Input.GetAxis("Controller Y");
        }

        private void CameraInputData()
        {
            mouseInputVector.x = Input.GetAxis("Camera X");
            mouseInputVector.y = Input.GetAxis("Camera Y");

            zoomClicked = Input.GetButtonDown("Zoom");
            zoomReleased = Input.GetButtonUp("Zoom");
        }

        private void MovementInputData()
        {
            movementInputVector.x = Input.GetAxisRaw("Horizontal Movement");
            movementInputVector.y = Input.GetAxisRaw("Vertical Movement");

            movementInputVector.x = Input.GetAxisRaw("Horizontal Movement");
            movementInputVector.y = Input.GetAxisRaw("Vertical Movement");

            runClicked = enableRunning ? Input.GetButtonDown("Run") : false;
            runReleased = enableRunning ? Input.GetButtonUp("Run") : false;

            if(runClicked)
            {
                isRunning = true;
            }

            if(runReleased)
            {
                isRunning = false;
            }

            crouchClicked = enableCrouching ? Input.GetButtonDown("Crouch") : false;
            jumpClicked = enableJumping ? Input.GetButtonDown("Jump") : false;
        }

        public void ResetInputData()
        {
            movementInputVector = Vector2.zero;
            isRunning = false;
            runClicked = false;
            runReleased =false;
            isCrouching = false;
            crouchClicked = false;
            jumpClicked = false;

            mouseInputVector = Vector2.zero;

            leftMouseClicked = false;
            rightMouseClicked = false;
            escapeClicked = false;
        }

        public void MovementInputCheck()
        {
            if(movementInputVector == Vector2.zero)
            {
                hasMovementInput = false;
            }
            else
            {
                hasMovementInput = true;
            }
        }

        private void InputModeCheck()
        {
            if(leftMouseClicked == true || rightMouseClicked == true || interactClicked == true || controllerInteractClicked == true || movementInputVector != Vector2.zero)
            {
                GameManager.instance.inputMode = 2;

                if(!InteractionSystem.instance.mainMenu)
                {
                    LevelManager.instance.SetInputMode(2);
                }
            }
        }

    #endregion
}