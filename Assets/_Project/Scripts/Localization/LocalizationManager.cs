using System;
using System.Collections.Generic;
using UnityEngine;

namespace UzOpenWorld.Localization
{
    public enum GameLanguage { Uzbek = 0, Russian = 1, English = 2 }

    /// <summary>
    /// JSON asosidagi lokalizatsiya. Fayllar: Assets/_Project/Resources/Localization/{uz|ru|en}.json
    /// Kod ichida matn yozilmaydi — faqat kalit: LocalizationManager.Get("ui.jump").
    /// Yangi til qo'shish: enum'ga qiymat + Codes massiviga kod + yangi JSON fayl.
    /// </summary>
    [DefaultExecutionOrder(-500)]
    public class LocalizationManager : MonoBehaviour
    {
        public static LocalizationManager Instance { get; private set; }
        public static event Action LanguageChanged;

        const string PrefKey = "settings.language"; // Phase 5'da SaveManager'ga o'tadi
        static readonly string[] Codes = { "uz", "ru", "en" };

        public GameLanguage Current { get; private set; }

        Dictionary<string, string> _table = new Dictionary<string, string>();
        Dictionary<string, string> _fallback; // English: kalit topilmasa

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;

            _fallback = LoadTable("en");
            int saved = PlayerPrefs.GetInt(PrefKey, (int)DetectDefault());
            SetLanguage((GameLanguage)Mathf.Clamp(saved, 0, Codes.Length - 1));
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetLanguage(GameLanguage lang)
        {
            Current = lang;
            _table = LoadTable(Codes[(int)lang]);
            PlayerPrefs.SetInt(PrefKey, (int)lang);
            LanguageChanged?.Invoke();
        }

        public void CycleLanguage()
        {
            SetLanguage((GameLanguage)(((int)Current + 1) % Codes.Length));
        }

        public static string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            var inst = Instance;
            if (inst != null)
            {
                if (inst._table.TryGetValue(key, out var v)) return v;
                if (inst._fallback != null && inst._fallback.TryGetValue(key, out v)) return v;
            }
            return "#" + key; // topilmagan kalit ko'zga tashlanib turadi
        }

        public static string Format(string key, params object[] args) => string.Format(Get(key), args);

        static GameLanguage DetectDefault()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Russian: return GameLanguage.Russian;
                case SystemLanguage.English: return GameLanguage.English;
                default: return GameLanguage.Uzbek;
            }
        }

        static Dictionary<string, string> LoadTable(string code)
        {
            var dict = new Dictionary<string, string>();
            var asset = Resources.Load<TextAsset>("Localization/" + code);
            if (asset == null)
            {
                Debug.LogWarning($"[Localization] Resources/Localization/{code}.json topilmadi");
                return dict;
            }

            var file = JsonUtility.FromJson<LocalizationFile>(asset.text);
            if (file?.entries == null) return dict;
            foreach (var e in file.entries)
                if (!string.IsNullOrEmpty(e.key)) dict[e.key] = e.value;
            return dict;
        }

        [Serializable]
        class LocalizationFile
        {
            public string language;
            public Entry[] entries;
        }

        [Serializable]
        class Entry
        {
            public string key;
            public string value;
        }
    }
}
