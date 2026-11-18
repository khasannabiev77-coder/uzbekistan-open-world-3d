using System.Collections.Generic;
using UnityEngine;

namespace UzOpenWorld.World
{
    /// <summary>
    /// Prototip uchun materiallar keshi. Bir xil rangga bitta material (SRP Batcher + static batching
    /// uchun yaxshi). Shader render pipeline'ning default materialidan olinadi, shuning uchun
    /// URP'da URP/Lit ishlaydi va build'da shader "strip" bo'lib ketmaydi.
    /// </summary>
    public class MaterialLibrary
    {
        readonly Material _template;
        readonly Dictionary<Color, Material> _cache = new Dictionary<Color, Material>();
        static Texture2D _windowTexture;

        public MaterialLibrary(Material template)
        {
            if (template == null)
            {
                var tmp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                template = tmp.GetComponent<Renderer>().sharedMaterial;
                Object.DestroyImmediate(tmp);
            }
            _template = template;
        }

        public Material Get(Color color, float smoothness = 0.15f)
        {
            if (_cache.TryGetValue(color, out var m)) return m;
            m = new Material(_template) { name = "Proto_" + ColorUtility.ToHtmlStringRGB(color), enableInstancing = true };
            m.color = color;
            SetSmoothness(m, smoothness);
            _cache[color] = m;
            return m;
        }

        /// <summary>Derazali bino materiali. Tiling = (qavatdagi derazalar soni, qavatlar soni).</summary>
        public Material Building(Color wallColor, Vector2 tiling)
        {
            var m = new Material(_template) { name = "Proto_Building", enableInstancing = true };
            m.color = wallColor;
            m.mainTexture = WindowTexture;
            m.mainTextureScale = tiling;
            SetSmoothness(m, 0.3f);
            return m;
        }

        static void SetSmoothness(Material m, float s)
        {
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", s);   // URP
            if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", s);   // Built-in
        }

        /// <summary>Protsedural 32x32 deraza teksturasi: oq devor (rang material bilan beriladi) + to'q deraza.</summary>
        static Texture2D WindowTexture
        {
            get
            {
                if (_windowTexture != null) return _windowTexture;
                const int size = 32;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, true)
                {
                    name = "Proto_Windows",
                    wrapMode = TextureWrapMode.Repeat,
                    filterMode = FilterMode.Bilinear,
                    anisoLevel = 2
                };
                var px = new Color32[size * size];
                var wall = new Color32(255, 255, 255, 255);
                var glass = new Color32(62, 82, 104, 255);
                var frame = new Color32(200, 200, 200, 255);
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    bool window = x >= 7 && x < 25 && y >= 9 && y < 27;
                    bool frameLine = window && (x == 16 || y == 18);
                    px[y * size + x] = frameLine ? frame : window ? glass : wall;
                }
                tex.SetPixels32(px);
                tex.Apply(true, true); // mipmap + GPU'ga yuklab, CPU nusxasini o'chirish (RAM tejash)
                _windowTexture = tex;
                return tex;
            }
        }
    }
}
