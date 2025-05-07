using EasyTalk.Controller;
using EasyTalk.Display;
using System.Collections.Generic;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace EasyTalk.Demo
{
    /// <summary>
    /// Handles movement and camera tilt controls for the player in the EasyTalk Demo.
    /// </summary>
    public class PlayerController : DialogueListener
    {
        /// <summary>
        /// The player camera.
        /// </summary>
        [SerializeField]
        private Camera playerCamera;

        /// <summary>
        /// Controls the movement speed.
        /// </summary>
        [SerializeField]
        private float movementSpeed = 5.0f;

        /// <summary>
        /// Controls how fast the player can turn.
        /// </summary>
        [SerializeField]
        private float lookRotationSpeed = 3.0f;

#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// An Input Action Asset used to setup the player input controls.
        /// </summary>
        [SerializeField]
        private InputActionAsset playerCharacterControls;

        /// <summary>
        /// A multipler used to control analog stick control sensitivity.
        /// </summary>
        private float stickMultiplier = 80.0f;
#endif

        /// <summary>
        /// The current camera tilt/pitch.
        /// </summary>
        private float cameraTilt = 0.0f;

        /// <summary>
        /// A flag used to control whether the player can move around.
        /// </summary>
        private bool movementEnabled = true;

        /// <summary>
        /// A flag used to control whether the player can rotate the camera.
        /// </summary>
        private bool cameraRotationEnabled = true;

        /// <summary>
        /// The current movement direction for the player.
        /// </summary>
        private Vector3 moveDirection = Vector3.zero;

        /// <summary>
        /// The left/right camera look rotation amount.
        /// </summary>
        private float leftRightRotation = 0.0f;

        /// <summary>
        /// The up/down camera look rotation amount.
        /// </summary>
        private float upDownRotation = 0.0f;

        /// <summary>
        /// The strafe/horizontal movement amount.
        /// </summary>
        private float horizontal = 0.0f;

        /// <summary>
        /// The forward/reverse movement amount.
        /// </summary>
        private float vertical = 0.0f;

        // Start is called before the first frame update
        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            playerCamera.transform.localRotation = Quaternion.Euler(cameraTilt, 0.0f, 0.0f);

#if ENABLE_INPUT_SYSTEM
            if (playerCharacterControls != null)
            {
                playerCharacterControls.Enable();
            }
#endif
        }

        private void Update()
        {
            //Reset movement and rotation values
            moveDirection = Vector3.zero;
            leftRightRotation = 0.0f;
            upDownRotation = 0.0f;
            horizontal = 0.0f;
            vertical = 0.0f;

            //Get the current input values for vertical and horizontal movement and camera rotation.
#if ENABLE_INPUT_SYSTEM
            CheckMovementAndRotationUsingNewInput();
#else
            CheckMovementAndRotationUsingOldInput();
#endif
        }

#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Get movement and look rotation values using newer input system.
        /// </summary>
        private void CheckMovementAndRotationUsingNewInput()
        {
            Vector2 joystickRotation = playerCharacterControls["JoystickRotation"].ReadValue<Vector2>() * stickMultiplier;
            leftRightRotation = joystickRotation.x;
            upDownRotation = joystickRotation.y;

            Vector2 mouseRotation = playerCharacterControls["MouseRotation"].ReadValue<Vector2>();

            horizontal = playerCharacterControls["StrafeMovement"].ReadValue<float>();
            vertical = playerCharacterControls["ForwardMovement"].ReadValue<float>();

            if (Mathf.Abs(mouseRotation.x) > Mathf.Abs(leftRightRotation))
            {
                leftRightRotation = mouseRotation.x;
            }

            if (Mathf.Abs(mouseRotation.y) > Mathf.Abs(upDownRotation))
            {
                upDownRotation = mouseRotation.y;
            }
        }
#endif

        /// <summary>
        /// Get movement and look rotation values using old input manager.
        /// </summary>
        private void CheckMovementAndRotationUsingOldInput()
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            leftRightRotation = Input.GetAxis("Mouse X") * 20.0f;
            upDownRotation = Input.GetAxis("Mouse Y") * 20.0f;
        }

        // Update is called once per frame
        void FixedUpdate()
        {

            Rigidbody rb = GetComponent<Rigidbody>();

            //Handle player movement.
            if (movementEnabled)
            {
                //Add in forward/reverse movement.
                if (Mathf.Abs(vertical) > 0.0f)
                {
                    moveDirection += this.transform.forward * vertical;
                }

                //Add in the horizontal movement.
                if (Mathf.Abs(horizontal) > 0.0f)
                {
                    moveDirection += this.transform.right * horizontal;
                }

                //Set the rigid body velocity based on the movement direction.
                if (moveDirection.magnitude > 0)
                {
                    Vector3 velocity = (moveDirection).normalized * movementSpeed * Time.fixedDeltaTime;
                    rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
                }
                else
                {
                    rb.linearVelocity = new Vector3(0.0f, rb.linearVelocity.y, 0.0f);
                }
            }
            else
            {
                rb.linearVelocity = new Vector3(0.0f, rb.linearVelocity.y, 0.0f);
            }

            //Handle camera rotation.
            if (cameraRotationEnabled)
            {
                //Handle yaw.
                if (Mathf.Abs(leftRightRotation) > 0.0f)
                {
                    this.transform.Rotate(Vector3.up, leftRightRotation * Time.fixedDeltaTime * lookRotationSpeed, Space.Self);
                }

                //Handle tilt/pitch.
                if (Mathf.Abs(upDownRotation) > 0.0f)
                {
                    float tiltAngle = upDownRotation * Time.fixedDeltaTime * -1.0f * lookRotationSpeed;
                    cameraTilt = Mathf.Clamp(cameraTilt + tiltAngle, -85.0f, 85.0f);
                    playerCamera.transform.localRotation = Quaternion.Euler(cameraTilt, 0.0f, 0.0f);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the player can move around using input controls.
        /// </summary>
        public bool MovementEnabled
        {
            get { return movementEnabled; }
            set { movementEnabled = value; }
        }

        /// <summary>
        /// Enables controls for rotating the camera around and locks the mouse cursor.
        /// </summary>
        public void EnableCameraRotation()
        {
            this.cameraRotationEnabled = true;
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// Disables controls for rotating the camera around and unlocks the mouse cursor.
        /// </summary>
        public void DisableCameraRotation()
        {
            this.cameraRotationEnabled = false;
            Cursor.lockState = CursorLockMode.Confined;
        }
    }
}