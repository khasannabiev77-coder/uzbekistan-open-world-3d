using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UzOpenWorld.UI.Mobile
{
    /// <summary>
    /// Touch tugma: bosilganda/qo'yib yuborilganda event beradi (Button'dan farqli — "hold" ni qo'llaydi).
    /// Toggle holatini ham ko'rsata oladi (masalan, Sprint).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class MobileButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        public Color normalColor = new Color(1f, 1f, 1f, 0.22f);
        public Color pressedColor = new Color(1f, 1f, 1f, 0.5f);
        public Color toggledColor = new Color(0.08f, 0.6f, 0.66f, 0.65f);

        public event Action Pressed;
        public event Action Released;
        public bool IsHeld { get; private set; }

        bool _toggled;
        Image _image;

        void Awake()
        {
            _image = GetComponent<Image>();
            Refresh();
        }

        public void OnPointerDown(PointerEventData e)
        {
            IsHeld = true;
            Refresh();
            Pressed?.Invoke();
        }

        public void OnPointerUp(PointerEventData e)
        {
            IsHeld = false;
            Refresh();
            Released?.Invoke();
        }

        public void SetToggled(bool on)
        {
            _toggled = on;
            Refresh();
        }

        void OnDisable()
        {
            if (IsHeld)
            {
                IsHeld = false;
                Released?.Invoke();
            }
            Refresh();
        }

        void Refresh()
        {
            if (_image == null) return;
            _image.color = IsHeld ? pressedColor : _toggled ? toggledColor : normalColor;
        }
    }
}
