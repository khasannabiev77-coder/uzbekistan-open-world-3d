using UnityEngine;
using UnityEngine.UI;

namespace UzOpenWorld.Localization
{
    /// <summary>
    /// UI Text'ga qo'shiladi va til o'zgarganda matnni avtomatik yangilaydi.
    /// Inspector: Key = "ui.jump" kabi.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class LocalizedText : MonoBehaviour
    {
        [SerializeField] string key;
        Text _text;

        public void SetKey(string newKey)
        {
            key = newKey;
            Refresh();
        }

        void OnEnable()
        {
            LocalizationManager.LanguageChanged += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            LocalizationManager.LanguageChanged -= Refresh;
        }

        void Refresh()
        {
            if (_text == null) _text = GetComponent<Text>();
            _text.text = LocalizationManager.Get(key);
        }
    }
}
