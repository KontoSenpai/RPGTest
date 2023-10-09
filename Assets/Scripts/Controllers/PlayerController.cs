using RPGTest.Models;
using RPGTest.Interactibles;
using RPGTest.Managers;
using UnityEngine;
using RPGTest.UI;
using RPGTest.Inputs;
using System.Collections;
using RPGTest.Helpers;
using UnityEngine.InputSystem;

namespace RPGTest.Controllers
{

    public partial class PlayerController : MonoBehaviour
    {
        [SerializeField] private bool IsDebug = false;

        public float Speed = 5f;
        public float JumpHeight = 2f;
        public float DashDistance = 5f;
        public float WalkScale = 0.33f;

        public float Gravity = -9.81f;
        public Vector3 Drag;
        [SerializeField] private float Height;
        [SerializeField] private float GroundDistance = 0.5f;
        [SerializeField] private float MaxAngle = 45;
        [SerializeField] private LayerMask GroundLayer;

        private CharacterController m_controller;
        private Animator m_animator;

        private Controls m_playerInput;
        private Vector2 m_playerDirection;

        private Vector3 _velocity;

        private RaycastHit m_groundCheckHit;
        private float m_groundAngle = 90;

        private bool m_wasGrounded;
        private bool m_isGrounded = true;

        private float m_jumpTimeStamp = 0;
        private float m_minJumpInterval = 1.3f;
        private bool m_jumpCoolDownOver;
        private UIManager m_UIManager;

        //Interactions
        private GameObject m_interactibleTarget;

        public void Awake()
        {
            m_playerInput = new Controls();
            
            m_playerInput.Player.Jump.performed += Jump;

            m_playerInput.Player.Interact.performed += Interact;
            m_playerInput.Player.Cancel.performed += Cancel;

            m_playerInput.Player.OpenMenu.performed += OpenMenu;
            m_playerInput.Player.Debug.performed += DebugOperation;
        }

        public void OnEnable() => m_playerInput.Enable();
        public void OnDisable() => m_playerInput.Disable();

        #region Trigger Events
        private void OnTriggerEnter(Collider other)
        {
            switch (other.tag)
            {
                case "Interactible":
                    m_interactibleTarget = other.gameObject;
                    break;
                case "Enemy":
                    other.GetComponent<InteractibleEnemy>().Interact(this.gameObject, other);
                    break;
                case "LoadingZone":
                    other.GetComponent<InteractibleExit>().Interact();
                    this.enabled = false;
                    break;
                case "Climbable":
                    InitiateClimbing(other);
                    break;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            switch (other.tag)
            {
                case "Interactible":
                    m_interactibleTarget = null;
                    break;
                case "Climbable":
                    ExitClimbing(other);
                    break;
            }
        }
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            m_controller = GetComponent<CharacterController>();
            m_animator = GetComponent<Animator>();
            m_UIManager = FindObjectOfType<UIManager>();
            m_UIManager.MenuChanged += OnMenuChanged;
        }

        private void OnMenuChanged(bool inMenu)
        {
            this.enabled = !inMenu;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (m_isClimbing)
            {
                if (m_playerDirection.x != 0 || m_playerDirection.y != 0)
                    ApplyClimbingMovement();
                return;
            }

            m_isGrounded = CheckGround();
            m_animator.SetBool("Grounded", m_isGrounded);
            if (m_isGrounded && _velocity.y < 0)
                _velocity.y = 0f;

            CalculateGroundAngle();
            ApplyGroundMovement();

            // While in the air, we need to check if we're looking in the right direction to begin a climb
            if (!m_isGrounded && !m_isClimbing && m_currentClimbingCollider != null && (m_playerDirection.x != 0 || m_playerDirection.y != 0))
            {
                if (ValidateClimbingAngle(m_currentClimbingCollider))
                {
                    BeginClimbing(m_currentClimbingCollider);
                }
            } else if (m_isGrounded && !m_isClimbing && m_climbingInteration != null)
            {
                ClimbingInteractionVisibility(true);
            }
        }

        #region InputSystem Events
        public void Move(InputAction.CallbackContext callbackContext)
        {
            m_playerDirection = callbackContext.ReadValue<Vector2>();                
        }

        public void Interact(InputAction.CallbackContext callbackContext)
        {
            if (m_isGrounded && m_interactibleTarget != null)
            {
                var interactible = m_interactibleTarget.GetComponent<IInteractible>();
                interactible.Interact();
            }
            else if(m_isGrounded && m_climbingInteration != null)
            {
                BeginClimbing();
            }
        }

        public void Cancel(InputAction.CallbackContext callbackContext)
        {
            if(m_isClimbing)
            {
                ExitClimbing();
            }
        }

        /// <summary>
        /// InputSystem event. Will initialize the PauseMenu on call
        /// </summary>
        /// <param name="obj"></param>
        public void OpenMenu(InputAction.CallbackContext callbackContext)
        {
            var UIMenu = FindObjectOfType<UIManager>().GetUIComponent<UI_Menu>();
            UIMenu.OpenDefault();
        }

        public void DebugOperation(InputAction.CallbackContext callbackContext)
        {
            var gameManager = FindObjectOfType<GameManager>();
            if (SaveFileModel.Instance == null)
            {
                gameManager.CreateSaveFile("FILE01");
            }
            gameManager.UpdateCurrentSceneState(transform.position, transform.eulerAngles);
            SaveFileModel.Instance.SaveToFile();
        }

        /// <summary>
        /// Makes the character jump if standing on ground or climbing
        /// </summary>
        public void Jump(InputAction.CallbackContext callbackContext)
        {
            m_jumpCoolDownOver = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

            if(m_jumpCoolDownOver && m_isClimbing)
            {
                Vector3 tmpForward = transform.forward;
                transform.forward = -transform.forward;
                m_isClimbing = false;
                m_jumpTimeStamp = Time.time;
                _velocity.x -= tmpForward.x * 3;
                _velocity.y = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                _velocity.z -= tmpForward.z * 3;
                StartCoroutine(ResetHorizontalVelocity());
            }
            else if (m_jumpCoolDownOver && m_isGrounded && (m_groundCheckHit.collider == null || Mathf.Abs(m_groundAngle) < MaxAngle))
            {
                m_wasGrounded = m_isGrounded;
                m_jumpTimeStamp = Time.time;
                _velocity.y += Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }

            if (!m_isGrounded && m_wasGrounded)
            {
                m_animator.SetTrigger("Jump");
            }
        }
        #endregion

        #region Movement Methods
        /// <summary>
        /// Simple check to verify if the player is standing on ground or jumping/climbing/etc...
        /// </summary>
        private bool CheckGround()
        {
            var pos = transform.position;
            pos.y += Height;

            RaycastHit hit;
            bool isGrounded = Physics.BoxCast(pos, new Vector3(0.1f, 0.1f, 0.1f), -transform.up, out hit, transform.rotation, Height, GroundLayer);

            return isGrounded;
        }

        /// <summary>
        /// Apply gravity. Not applied when climbing. 
        /// </summary>
        /// <param name="direction">Direction in which the gravity takes effect</param>
        private void ApplyGravity(Vector3 direction)
        {
            _velocity.y += Gravity * Time.deltaTime;
            _velocity.y /= 1 + Drag.y * Time.deltaTime;

            m_controller.Move(_velocity * Time.deltaTime);
        }

        /// <summary>
        /// Calculate ground angle to determine if the player can move or not in a certain direction.
        /// </summary>
        private void CalculateGroundAngle()
        {
            if (!m_isGrounded)
            {
                m_groundAngle = 0;
                return;
            }

            var offset = transform.position + (transform.forward / 2);
            offset.y += Height * 2;

            if (Physics.Raycast(offset, Vector3.down, out m_groundCheckHit, 2, GroundLayer))
            {
                m_groundAngle = Vector3.Angle(m_groundCheckHit.normal, transform.forward) - 90;
            }
            else
            {
                m_groundAngle = 90;
            }
        }

        private IEnumerator ResetHorizontalVelocity()
        {
            do
            {
                _velocity.x = Mathf.Lerp(_velocity.x, 0, 0.2f);
                _velocity.z = Mathf.Lerp(_velocity.z, 0, 0.2f);
                yield return new WaitForSeconds(0.1f);
            } while (!_velocity.x.IsInRange(-0.1f, 0.1f) || !_velocity.z.IsInRange(-0.1f, 0.1f));
            _velocity.x = 0;
            _velocity.z = 0;
        }
        #endregion

        #region debug
        void OnDrawGizmosSelected()
        {
            if (!IsDebug)
                return;

            Gizmos.color = Color.yellow;

            //Draw a ray to check the ground angle
            Vector3 groundVector = transform.position + (transform.forward / 2);
            groundVector.y += Height * 2;
            Gizmos.DrawRay(groundVector, -transform.up * 2);

            //Draw a ray forward from GameObject head
            Vector3 posHead = transform.position;
            posHead.y += Height * 1.25f;
            posHead -= transform.forward;
            Gizmos.DrawRay(posHead, transform.forward * 3);

            //Draw a ray foward from below GameObject feets
            Vector3 posFeet = transform.position;
            posFeet.y -= Height;
            posFeet -= transform.forward;
            Gizmos.DrawRay(posFeet, transform.forward * 3);

            Vector3 pos = transform.position;
            pos.y += Height;
            //Draw a Ray downward from GameObject toward the maximum distance
            Gizmos.DrawRay(pos, -transform.up * Height);
            //Draw a cube at the maximum distance
            Gizmos.DrawWireCube(pos - (transform.up * Height), new Vector3(0.1f, 0.1f, 0.1f));
        }
        #endregion
    }

}
