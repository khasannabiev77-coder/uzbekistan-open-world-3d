using UnityEngine;
using UnityEngine.UI;

namespace UzOpenWorld.UI
{
    /// <summary>FPS va kadr vaqtini ko'rsatadi (real telefonda performance tekshirish uchun).</summary>
    [RequireComponent(typeof(Text))]
    public class FPSCounter : MonoBehaviour
    {
        public float updateInterval = 0.5f;

        Text _text;
        float _accum;
        int _frames;

        void Awake() => _text = GetComponent<Text>();

        void Update()
        {
            _accum += Time.unscaledDeltaTime;
            _frames++;
            if (_accum < updateInterval) return;

            float fps = _frames / _accum;
            float ms = _accum * 1000f / _frames;
            _text.text = $"FPS {fps:0}  ({ms:0.0} ms)";
            _text.color = fps >= 50f ? new Color(0.5f, 1f, 0.5f) : fps >= 28f ? new Color(1f, 0.9f, 0.4f) : new Color(1f, 0.45f, 0.4f);
            _accum = 0f;
            _frames = 0;
        }
    }
}
