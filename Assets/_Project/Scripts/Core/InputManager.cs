using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UzOpenWorld.Core
{
    /// <summary>
    /// Barcha kirish manbalarini (klaviatura + sichqoncha, gamepad, touch) bitta holatga birlashtiradi.
    /// Gameplay skriptlari (PlayerController, ThirdPersonCamera, keyinchalik VehicleController)
    /// faqat shu klassdan o'qiydi — qurilma turini bilishi shart emas.
    ///
    /// Talab: Unity Input System paketi (Unity 6 URP shablonida bor) va
    /// Player Settings > Active Input Handling = "Input System Package (New)" yoki "Both".
    /// </summary>
    [DefaultExecutionOrder(-100)] // Player va kameradan OLDIN yangilanadi
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        [Header("Sensitivity")]
        [Tooltip("Sichqoncha: har piksel uchun gradus")]
        public float mouseSensitivity = 0.12f;
        [Tooltip("Gamepad o'ng stick: soniyasiga gradus")]
        public float gamepadLookSpeed = 160f;
        [Tooltip("Touch: har piksel uchun gradus")]
        public float touchLookSensitivity = 0.18f;
        public bool invertY;

        [Header("Mobile")]
        [Tooltip("Editor'da mobil boshqaruvni sinash uchun. O'yin vaqtida F1 bilan almashtiriladi.")]
        public bool forceMobileControls;

        // ---- Natijaviy holat (har kadrda yangilanadi) ----
        public Vector2 Move { get; private set; }        // -1..1, uzunligi <= 1
        public Vector2 LookDelta { get; private set; }   // shu kadrdagi gradus
        public bool SprintHeld { get; private set; }
        public bool JumpPressed { get; private set; }    // faqat bosilgan kadrda true
        public bool InteractPressed { get; private set; }

        public bool UseMobileControls => forceMobileControls || Application.isMobilePlatform;
        public bool TouchSprint => _touchSprint;

        /// <summary>Mobil/PC rejimi almashganda chaqiriladi (HUD shu orqali tugmalarni yashiradi).</summary>
        public event Action<bool> MobileControlsChanged;

        Vector2 _touchMove, _touchLook;
        bool _touchJump, _touchInteract, _touchSprint;
        bool _lastMobileState;

        // ---- Touch UI shu metodlarni chaqiradi ----
        public void SetTouchMove(Vector2 v) => _touchMove = Vector2.ClampMagnitude(v, 1f);
        public void AddTouchLook(Vector2 pixelDelta) => _touchLook += pixelDelta;
        public void PressTouchJump() => _touchJump = true;
        public void PressTouchInteract() => _touchInteract = true;
        public void SetTouchSprint(bool held) => _touchSprint = held;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            _lastMobileState = UseMobileControls;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            Vector2 move = Vector2.zero, look = Vector2.zero;
            bool sprint = false, jump = false, interact = false, walkModifier = false;

            // ---------- Klaviatura ----------
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) move.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) move.y -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) move.x += 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) move.x -= 1f;
                sprint |= kb.leftShiftKey.isPressed;
                walkModifier = kb.leftCtrlKey.isPressed;
                jump |= kb.spaceKey.wasPressedThisFrame;
                interact |= kb.eKey.wasPressedThisFrame;

                if (kb.f1Key.wasPressedThisFrame) forceMobileControls = !forceMobileControls;
                if (kb.escapeKey.wasPressedThisFrame) UnlockCursor();
            }
            move = Vector2.ClampMagnitude(move, 1f);
            if (walkModifier) move *= 0.5f;

            // ---------- Rejim almashuvi ----------
            bool mobile = UseMobileControls;
            if (mobile != _lastMobileState)
            {
                _lastMobileState = mobile;
                if (mobile) UnlockCursor();
                MobileControlsChanged?.Invoke(mobile);
            }

            // ---------- Sichqoncha ----------
            var mouse = Mouse.current;
            if (mouse != null && !mobile)
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                    look += mouse.delta.ReadValue() * mouseSensitivity;
                else if (mouse.leftButton.wasPressedThisFrame && !IsPointerOverUI())
                    LockCursor();
            }

            // ---------- Gamepad ----------
            var gp = Gamepad.current;
            if (gp != null)
            {
                Vector2 ls = gp.leftStick.ReadValue();
                if (ls.sqrMagnitude > move.sqrMagnitude) move = ls;
                look += gp.rightStick.ReadValue() * gamepadLookSpeed * Time.unscaledDeltaTime;
                sprint |= gp.leftStickButton.isPressed || gp.rightShoulder.isPressed;
                jump |= gp.buttonSouth.wasPressedThisFrame;     // A / Cross
                interact |= gp.buttonWest.wasPressedThisFrame;  // X / Square
            }

            // ---------- Touch (UI orqali keladi) ----------
            if (_touchMove.sqrMagnitude > move.sqrMagnitude) move = _touchMove;
            look += _touchLook * touchLookSensitivity;
            _touchLook = Vector2.zero;
            sprint |= _touchSprint;
            jump |= _touchJump;
            interact |= _touchInteract;
            _touchJump = _touchInteract = false;

            if (invertY) look.y = -look.y;

            Move = Vector2.ClampMagnitude(move, 1f);
            LookDelta = look;
            SprintHeld = sprint;
            JumpPressed = jump;
            InteractPressed = interact;
        }

        public static void LockCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public static void UnlockCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        static bool IsPointerOverUI()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
