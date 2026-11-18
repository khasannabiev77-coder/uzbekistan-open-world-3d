using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UzOpenWorld.Core;
using UzOpenWorld.Localization;
using UzOpenWorld.UI.Mobile;

namespace UzOpenWorld.UI
{
    /// <summary>
    /// Phase 1 HUD: FPS, til tugmasi, PC maslahatlari va mobil boshqaruv
    /// (chapda floating joystick, o'ngda kamera zonasi + Sakrash / Yugurish / Amal tugmalari).
    /// Hammasi responsive: CanvasScaler (1920x1080, balandlik bo'yicha) + SafeArea.
    /// Phase 5'da UIManager va prefablarga ko'chiriladi.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        public Vector2 referenceResolution = new Vector2(1920f, 1080f);

        GameObject _mobileRoot, _pcHint, _clickHint;
        MobileButton _sprintButton;

        public void Build()
        {
            EnsureEventSystem();

            var canvasRt = UIFactory.CreateRect("HUD Canvas", transform);
            var canvas = canvasRt.gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasRt.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = referenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f; // landscape: 16:9 dan 21:9 gacha tugmalar bir xil o'lchamda
            canvasRt.gameObject.AddComponent<GraphicRaycaster>();

            var safe = UIFactory.CreateRect("SafeArea", canvasRt);
            UIFactory.Stretch(safe);
            safe.gameObject.AddComponent<SafeArea>();

            BuildMobileControls(safe); // birinchi — qolganlar ustida chiziladi
            BuildTopBar(safe);
            BuildHints(safe);

            var input = InputManager.Instance;
            if (input != null)
            {
                input.MobileControlsChanged += OnMobileChanged;
                OnMobileChanged(input.UseMobileControls);
            }
        }

        void OnDestroy()
        {
            if (InputManager.Instance != null) InputManager.Instance.MobileControlsChanged -= OnMobileChanged;
        }

        void Update()
        {
            if (_clickHint == null) return;
            var input = InputManager.Instance;
            bool show = input != null && !input.UseMobileControls && Cursor.lockState != CursorLockMode.Locked;
            if (_clickHint.activeSelf != show) _clickHint.SetActive(show);
        }

        void OnMobileChanged(bool mobile)
        {
            if (_mobileRoot != null) _mobileRoot.SetActive(mobile);
            if (_pcHint != null) _pcHint.SetActive(!mobile);

            if (!mobile && InputManager.Instance != null)
            {
                InputManager.Instance.SetTouchSprint(false);
                InputManager.Instance.SetTouchMove(Vector2.zero);
                if (_sprintButton != null) _sprintButton.SetToggled(false);
            }
        }

        // ------------------------------------------------------------------ top bar

        void BuildTopBar(RectTransform parent)
        {
            var fps = UIFactory.CreateText("FPS", parent, 30, TextAnchor.UpperLeft, Color.white);
            UIFactory.Place(fps.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -18), new Vector2(420, 50));
            fps.gameObject.AddComponent<FPSCounter>();

            var title = UIFactory.CreateLocalizedText("Title", parent, "game.title", 28, TextAnchor.UpperCenter, new Color(1f, 1f, 1f, 0.85f));
            UIFactory.Place(title.rectTransform, new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -18), new Vector2(800, 50));

            var langBg = UIFactory.CreateImage("LanguageButton", parent, new Color(0f, 0f, 0f, 0.45f));
            UIFactory.Place(langBg.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(-24, -14), new Vector2(300, 64));
            var btn = langBg.gameObject.AddComponent<Button>();
            btn.targetGraphic = langBg;
            btn.onClick.AddListener(() =>
            {
                if (LocalizationManager.Instance != null) LocalizationManager.Instance.CycleLanguage();
            });
            var langText = UIFactory.CreateLocalizedText("Label", langBg.transform, "ui.language_button", 28, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Stretch(langText.rectTransform);
        }

        // ------------------------------------------------------------------ hints

        void BuildHints(RectTransform parent)
        {
            var hint = UIFactory.CreateLocalizedText("PCHint", parent, "hint.pc_controls", 24, TextAnchor.LowerCenter, new Color(1f, 1f, 1f, 0.8f));
            UIFactory.Place(hint.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 24), new Vector2(1800, 60));
            _pcHint = hint.gameObject;

            var bg = UIFactory.CreateImage("ClickToPlay", parent, new Color(0f, 0f, 0f, 0.55f), null, false);
            UIFactory.Place(bg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760, 110));
            var txt = UIFactory.CreateLocalizedText("Label", bg.transform, "hint.click_to_play", 38, TextAnchor.MiddleCenter, Color.white);
            UIFactory.Stretch(txt.rectTransform);
            _clickHint = bg.gameObject;
        }

        // ------------------------------------------------------------------ mobile controls

        void BuildMobileControls(RectTransform parent)
        {
            var root = UIFactory.CreateRect("MobileControls", parent);
            UIFactory.Stretch(root);
            _mobileRoot = root.gameObject;

            // O'ng tomon: kamera zonasi (tugmalar ostida)
            var look = UIFactory.CreateImage("TouchLookArea", root, new Color(0, 0, 0, 0));
            look.rectTransform.anchorMin = new Vector2(0.45f, 0f);
            look.rectTransform.anchorMax = new Vector2(1f, 1f);
            look.rectTransform.offsetMin = look.rectTransform.offsetMax = Vector2.zero;
            look.gameObject.AddComponent<TouchLookArea>();

            // Chap tomon: floating joystick zonasi
            var zone = UIFactory.CreateImage("JoystickZone", root, new Color(0, 0, 0, 0));
            zone.rectTransform.anchorMin = new Vector2(0f, 0f);
            zone.rectTransform.anchorMax = new Vector2(0.45f, 0.75f);
            zone.rectTransform.offsetMin = zone.rectTransform.offsetMax = Vector2.zero;

            var bg = UIFactory.CreateImage("Background", zone.transform, new Color(1f, 1f, 1f, 0.16f), UIFactory.Circle, false);
            UIFactory.Place(bg.rectTransform, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(270, 250), new Vector2(300, 300));
            var handle = UIFactory.CreateImage("Handle", bg.transform, new Color(1f, 1f, 1f, 0.55f), UIFactory.Circle, false);
            UIFactory.Place(handle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(130, 130));

            var joystick = zone.gameObject.AddComponent<VirtualJoystick>();
            joystick.background = bg.rectTransform;
            joystick.handle = handle.rectTransform;
            joystick.ValueChanged += v =>
            {
                if (InputManager.Instance != null) InputManager.Instance.SetTouchMove(v);
            };

            // O'ng pastki burchak: tugmalar
            var jump = CreateButton(root, "JumpButton", "ui.jump", new Vector2(-200, 220), 210f);
            jump.Pressed += () =>
            {
                if (InputManager.Instance != null) InputManager.Instance.PressTouchJump();
            };

            _sprintButton = CreateButton(root, "SprintButton", "ui.sprint", new Vector2(-440, 150), 165f);
            _sprintButton.Pressed += () =>
            {
                var input = InputManager.Instance;
                if (input == null) return;
                bool on = !input.TouchSprint; // mobil'da toggle qulayroq
                input.SetTouchSprint(on);
                _sprintButton.SetToggled(on);
            };

            var interact = CreateButton(root, "InteractButton", "ui.interact", new Vector2(-200, 460), 160f);
            interact.Pressed += () =>
            {
                if (InputManager.Instance != null) InputManager.Instance.PressTouchInteract();
            };
        }

        static MobileButton CreateButton(RectTransform parent, string name, string labelKey, Vector2 pos, float size)
        {
            var img = UIFactory.CreateImage(name, parent, new Color(1f, 1f, 1f, 0.22f), UIFactory.Circle);
            UIFactory.Place(img.rectTransform, new Vector2(1, 0), new Vector2(0.5f, 0.5f), pos, new Vector2(size, size));
            var btn = img.gameObject.AddComponent<MobileButton>();
            var label = UIFactory.CreateLocalizedText("Label", img.transform, labelKey, Mathf.RoundToInt(size * 0.15f), TextAnchor.MiddleCenter, Color.white);
            UIFactory.Stretch(label.rectTransform);
            return btn;
        }

        static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<InputSystemUIInputModule>();
        }
    }
}
