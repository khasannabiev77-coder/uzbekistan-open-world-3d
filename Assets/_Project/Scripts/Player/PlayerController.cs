using UnityEngine;
using UzOpenWorld.Core;

namespace UzOpenWorld.PlayerSystem
{
    public enum MoveState { Idle, Walk, Run, Sprint, Jump, Fall }

    /// <summary>
    /// Third-person o'yinchi harakati: yurish, yugurish, sprint, sakrash, gravitatsiya.
    /// Kameraga nisbatan harakatlanadi. Input faqat InputManager orqali olinadi,
    /// shuning uchun PC, gamepad va touch'da bir xil ishlaydi.
    ///
    /// Animator ulansa (haqiqiy 3D model kelganda) quyidagi parametrlar to'ldiriladi:
    ///   float "Speed", bool "Grounded", float "VerticalSpeed".
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Speeds (m/s)")]
        public float walkSpeed = 1.9f;
        public float runSpeed = 4.6f;
        public float sprintSpeed = 7.2f;
        [Range(0.1f, 0.95f)]
        [Tooltip("Joystick shu qiymatdan kam egilsa — yurish, ko'p bo'lsa — yugurish")]
        public float runInputThreshold = 0.6f;

        [Header("Movement")]
        public float groundAcceleration = 14f;
        [Range(0f, 1f)] public float airControl = 0.35f;
        [Tooltip("Gradus/soniya")]
        public float rotationSpeed = 720f;

        [Header("Jump & Gravity")]
        public float jumpHeight = 1.15f;
        public float gravity = -22f;
        [Tooltip("Chetdan tushgandan keyin ham sakrash mumkin bo'lgan vaqt")]
        public float coyoteTime = 0.12f;
        [Tooltip("Shu balandlikdan pastga tushsa spawn nuqtasiga qaytariladi")]
        public float killY = -30f;

        [Header("References")]
        public Transform cameraTransform;
        [Tooltip("Ixtiyoriy. Haqiqiy 3D model + Animator Controller ulanganda.")]
        public Animator animator;

        public MoveState State { get; private set; }
        public float CurrentSpeed { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool InputEnabled { get; set; } = true;   // Phase 2: mashinaga o'tirganda false
        public Vector3 RespawnPoint { get; set; }

        CharacterController _cc;
        Vector3 _horizontal;
        float _vertical;
        float _lastGroundedTime = -999f;
        bool _sprintInput;

        static readonly int SpeedHash = Animator.StringToHash("Speed");
        static readonly int GroundedHash = Animator.StringToHash("Grounded");
        static readonly int VerticalHash = Animator.StringToHash("VerticalSpeed");
        bool _animatorReady;

        void Awake()
        {
            _cc = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null) cameraTransform = Camera.main.transform;
            RespawnPoint = transform.position;
            CacheAnimator();
        }

        void Update()
        {
            float dt = Time.deltaTime;
            InputManager input = InputEnabled ? InputManager.Instance : null;

            Vector2 move = input != null ? input.Move : Vector2.zero;
            _sprintInput = input != null && input.SprintHeld;
            bool jump = input != null && input.JumpPressed;

            // --- Kameraga nisbatan yo'nalish ---
            Vector3 fwd = Vector3.forward, right = Vector3.right;
            if (cameraTransform != null)
            {
                fwd = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up);
                if (fwd.sqrMagnitude < 1e-4f) fwd = Vector3.ProjectOnPlane(cameraTransform.up, Vector3.up);
                fwd.Normalize();
                right = Vector3.Cross(Vector3.up, fwd);
            }

            Vector3 dir = fwd * move.y + right * move.x;
            float inputMag = Mathf.Clamp01(move.magnitude);
            if (dir.sqrMagnitude > 1e-4f) dir.Normalize();

            float targetSpeed = 0f;
            if (inputMag > 0.05f)
                targetSpeed = _sprintInput ? sprintSpeed : (inputMag < runInputThreshold ? walkSpeed : runSpeed);

            // --- Yer bilan aloqa ---
            IsGrounded = _cc.isGrounded;
            if (IsGrounded) _lastGroundedTime = Time.time;

            float accel = groundAcceleration * (IsGrounded ? 1f : airControl);
            _horizontal = Vector3.MoveTowards(_horizontal, dir * targetSpeed, accel * dt);

            if (dir.sqrMagnitude > 1e-4f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * dt);
            }

            // --- Sakrash va gravitatsiya ---
            if (IsGrounded && _vertical < 0f) _vertical = -2f; // yerga "yopishib" turish uchun

            bool canJump = Time.time - _lastGroundedTime <= coyoteTime && _vertical <= 0f;
            if (jump && canJump)
            {
                _vertical = Mathf.Sqrt(2f * jumpHeight * -gravity);
                _lastGroundedTime = -999f;
            }

            _vertical = Mathf.Max(_vertical + gravity * dt, -50f);

            _cc.Move((_horizontal + Vector3.up * _vertical) * dt);

            Vector3 actual = _cc.velocity;
            actual.y = 0f;
            CurrentSpeed = actual.magnitude;

            if (transform.position.y < killY) Respawn();

            UpdateState();
            UpdateAnimator();
        }

        void UpdateState()
        {
            bool airborne = !IsGrounded && Time.time - _lastGroundedTime > coyoteTime;
            if (airborne) State = _vertical > 0f ? MoveState.Jump : MoveState.Fall;
            else if (CurrentSpeed < 0.1f) State = MoveState.Idle;
            else if (_sprintInput && CurrentSpeed > runSpeed * 0.9f) State = MoveState.Sprint;
            else if (CurrentSpeed > walkSpeed + 0.3f) State = MoveState.Run;
            else State = MoveState.Walk;
        }

        public void Respawn()
        {
            Teleport(RespawnPoint, transform.rotation);
        }

        /// <summary>CharacterController bilan xavfsiz teleport (Save/Load va fast travel uchun ham).</summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            _cc.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            _cc.enabled = true;
            _horizontal = Vector3.zero;
            _vertical = 0f;
        }

        void CacheAnimator()
        {
            _animatorReady = false;
            if (animator == null || animator.runtimeAnimatorController == null) return;
            foreach (var p in animator.parameters)
                if (p.nameHash == SpeedHash) { _animatorReady = true; break; }
        }

        void UpdateAnimator()
        {
            if (!_animatorReady) return;
            animator.SetFloat(SpeedHash, CurrentSpeed);
            animator.SetBool(GroundedHash, State != MoveState.Jump && State != MoveState.Fall);
            animator.SetFloat(VerticalHash, _vertical);
        }
    }
}
