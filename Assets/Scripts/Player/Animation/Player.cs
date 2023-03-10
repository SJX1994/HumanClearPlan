// Copyright © 2022, Maeve "Molasses" Garside
// Licensed under the MIT license, making this script copyleft. 
// Check the LICENSE.md file for further information.
// https://github.com/SyrupMedia/media.syrup.unity.player/releases/tag/v1.0.0

using UnityEngine;

namespace SyrupPlayer
{
    public class Player : MonoBehaviour
    {
        #region Variables
        public int playerID;
        [Header("Physics")]
        public bool applyGravity;
        public float gravity = -9.81f;
        private Vector3 verticalVelocity;
        private bool isGrounded;   
        public Transform groundCheck;
        public Transform display;
        public float groundDistance = 0.5f;
        public LayerMask groundLayer;
        public KeyCode JumpKey = KeyCode.Space;

        [Header("Graphics")]
        public Animator animator;

        [Header("Movement")]
        public CharacterController controller;



        [Header("Controls")]
        [Tooltip("Locks the player's mouse.")]
        public bool lockedMouse;

        [Header("Locomotion")]
        [Tooltip("Determines the player's movement speed.")]
        [Range(1f, 10f)]
        [Min(1f)]
        public float movementSpeed = 5f;
        [Range(0.1f, 0.5f)]
        [Min(0.1f)]
        public float turnSmoothing = 0.1f;
        private float turnVelocity;
        private string playerHorizontal;
        private string playerVertical;
        // private RaycastHit hit;
        // private Ray ray;
        #endregion

        #region Unity Methods

        // Awake() is called when the script instance is being loaded.
        private void Awake()
        {

        }

        // Start() is called before the first frame update.
        private void Start()
        {
            if (lockedMouse)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        // Update() is called once per frame.
        private void FixedUpdate()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                Cursor.lockState = CursorLockMode.None;
                lockedMouse = false;
            }
            switch(playerID)
            {
                case 1:
                     playerHorizontal = "P1Horizontal";
                     playerVertical = "P1Vertical";
                    break;
                case 2:
                     playerHorizontal = "P2Horizontal";
                     playerVertical = "P2Vertical";
                    break;
               
            }
            float horizontal = Input.GetAxis(playerHorizontal);
            float vertical = Input.GetAxis(playerVertical);

            animator.SetFloat("horizontal", horizontal);
            animator.SetFloat("vertical", vertical);

            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

            if (direction.magnitude >= 0.1f)
            {
                animator.SetBool("isWalking", true);

                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnSmoothing);

                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
                controller.Move(moveDirection.normalized * movementSpeed * Time.deltaTime);
            }
            else
            {
                animator.SetBool("isWalking", false);
            }

            if (applyGravity == true)
            {
                // This is the same approach featured in Brackeys' FPS Controller.
                // https://youtu.be/_QajrabyTJc?t=1132
                isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
                

                 if(Input.GetKey(JumpKey) && isGrounded)
                 {
                      Debug.Log($"{transform.name} `JumpKey` was pressed.");
                      verticalVelocity.y += 2.6f;
                 }
                groundCheck.GetComponent<JellyChecker>().HitEffect();
               
                if (isGrounded && verticalVelocity.y < 0)
                {
                    
                    verticalVelocity.y = -2.5f;
                    
                }

                verticalVelocity.y += gravity * Time.deltaTime;

                controller.Move(verticalVelocity * Time.deltaTime); 
            }
           
        }

        #endregion
    }
}