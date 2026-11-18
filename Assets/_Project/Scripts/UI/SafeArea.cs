using UnityEngine;

namespace UzOpenWorld.UI
{
    /// <summary>
    /// RectTransform'ni Screen.safeArea'ga moslaydi: iPhone notch, Android "punch-hole",
    /// dumaloq burchaklar ostida tugmalar qolib ketmaydi. Ekran aylansa/o'lcham o'zgarsa qayta hisoblaydi.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeArea : MonoBehaviour
    {
        RectTransform _rt;
        Rect _lastSafe;
        Vector2Int _lastRes;

        void Awake()
        {
            _rt = (RectTransform)transform;
            Apply();
        }

        void Update()
        {
            if (Screen.safeArea != _lastSafe || Screen.width != _lastRes.x || Screen.height != _lastRes.y)
                Apply();
        }

        void Apply()
        {
            _lastSafe = Screen.safeArea;
            _lastRes = new Vector2Int(Screen.width, Screen.height);
            if (Screen.width <= 0 || Screen.height <= 0) return;

            Vector2 min = _lastSafe.position;
            Vector2 max = _lastSafe.position + _lastSafe.size;
            min.x /= Screen.width; max.x /= Screen.width;
            min.y /= Screen.height; max.y /= Screen.height;

            _rt.anchorMin = min;
            _rt.anchorMax = max;
            _rt.offsetMin = Vector2.zero;
            _rt.offsetMax = Vector2.zero;
        }
    }
}
