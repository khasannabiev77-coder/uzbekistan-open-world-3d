using UnityEngine;
using UnityEngine.EventSystems;
using UzOpenWorld.Core;

namespace UzOpenWorld.UI.Mobile
{
    /// <summary>
    /// Ekranning o'ng qismida barmoq surilsa kamera aylanadi.
    /// Shaffof Image'ga qo'shiladi; tugmalar undan keyin (ustida) joylashadi.
    /// </summary>
    public class TouchLookArea : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        const int NoPointer = int.MinValue;
        int _pointerId = NoPointer;

        public void OnPointerDown(PointerEventData e)
        {
            if (_pointerId == NoPointer) _pointerId = e.pointerId;
        }

        public void OnDrag(PointerEventData e)
        {
            if (e.pointerId != _pointerId) return;
            var input = InputManager.Instance;
            if (input != null) input.AddTouchLook(e.delta);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (e.pointerId == _pointerId) _pointerId = NoPointer;
        }

        void OnDisable() => _pointerId = NoPointer;
    }
}
