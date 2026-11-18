using UnityEngine;
using UnityEngine.UI;
using UzOpenWorld.Localization;

namespace UzOpenWorld.UI
{
    /// <summary>UI elementlarini koddan yaratish uchun yordamchi (Phase 1 prefablarsiz ishlashi uchun).</summary>
    public static class UIFactory
    {
        static Font _font;
        static Sprite _circle;

        public static Font DefaultFont
        {
            get
            {
                if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return _font;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (_circle == null) _circle = CreateCircleSprite(128);
                return _circle;
            }
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static void Place(RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
        }

        public static Image CreateImage(string name, Transform parent, Color color, Sprite sprite = null, bool raycast = true)
        {
            var rt = CreateRect(name, parent);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            return img;
        }

        public static Text CreateText(string name, Transform parent, int fontSize, TextAnchor align, Color color)
        {
            var rt = CreateRect(name, parent);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = DefaultFont;
            t.fontSize = fontSize;
            t.alignment = align;
            t.color = color;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            rt.gameObject.AddComponent<Shadow>().effectDistance = new Vector2(2f, -2f);
            return t;
        }

        public static Text CreateLocalizedText(string name, Transform parent, string key, int fontSize, TextAnchor align, Color color)
        {
            var t = CreateText(name, parent, fontSize, align, color);
            t.gameObject.AddComponent<LocalizedText>().SetKey(key);
            return t;
        }

        static Sprite CreateCircleSprite(int size)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "UI_Circle",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            float r = size * 0.5f;
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                byte a = (byte)(Mathf.Clamp01(r - d) * 255f); // yumshoq (anti-aliased) chet
                px[y * size + x] = new Color32(255, 255, 255, a);
            }
            tex.SetPixels32(px);
            tex.Apply(false, true);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
