using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UzOpenWorld.UI.Mobile
{
    /// <summary>
    /// "Floating" virtual joystick: ekranning chap zonasida qayerga barmoq tegsa, joystick o'sha yerda paydo bo'ladi.
    /// Bu komponent ZONA'ga (shaffof Image) qo'shiladi; background va handle — uning bolalari.
    /// </summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform background;
        public RectTransform handle;
        [Range(0f, 0.5f)] public float deadZone = 0.08f;

        public Vector2 Value { get; private set; }
        public event Action<Vector2> ValueChanged;

        const int NoPointer = int.MinValue;
        RectTransform _zone;
        Vector3 _defaultLocalPos;
        int _pointerId = NoPointer;

        void Awake() => _zone = (RectTransform)transform;
        void Start() => _defaultLocalPos = background.localPosition;

        public void OnPointerDown(PointerEventData e)
        {
            if (_pointerId != NoPointer) return;
            _pointerId = e.pointerId;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_zone, e.position, e.pressEventCamera, out Vector2 local))
                background.localPosition = local;
            OnDrag(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != _pointerId) return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(background, e.position, e.pressEventCamera, out Vector2 local))
                return;

            float radius = background.rect.width * 0.5f;
            Vector2 v = Vector2.ClampMagnitude(local / Mathf.Max(1f, radius), 1f);
            handle.anchoredPosition = v * radius;
            SetValue(v.magnitude < deadZone ? Vector2.zero : v);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId != _pointerId) return;
            ResetStick();
        }

        void OnDisable() => ResetStick();

        void ResetStick()
        {
            _pointerId = NoPointer;
            if (background != null) background.localPosition = _defaultLocalPos;
            if (handle != null) handle.anchoredPosition = Vector2.zero;
            SetValue(Vector2.zero);
        }

        void SetValue(Vector2 v)
        {
            if (v == Value) return;
            Value = v;
            ValueChanged?.Invoke(v);
        }
    }
}
