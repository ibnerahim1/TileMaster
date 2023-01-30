using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace YsoCorp {
    namespace GameUtils {

        public class I18nElement : BaseManager {

            private string _key;
            private bool usingTMPro = false;
            private Text _text;
            private TextMeshProUGUI _textTMPro;

            protected override void Awake() {
                base.Awake();
                this._text = this.GetComponent<Text>();
                if (this._text == null) {
                    this._textTMPro = this.GetComponent<TextMeshProUGUI>();
                    if (this._textTMPro != null) {
                        this._key = this._textTMPro.text;
                        this.usingTMPro = true;
                    } else {
                        Debug.LogError("Text component not found for translation");
                    }
                } else {
                    this._key = this._text.text;
                }
                this.UpdateText();
                this.ycManager.i18nManager.AddElement(this);
            }

            protected override void OnDestroyNotQuitting() {
                this.ycManager.i18nManager.DelElement(this);
            }

            public void UpdateText() {
                if (this.usingTMPro) {
                    this._textTMPro.text = this.ycManager.i18nManager.GetString(this._key);
                } else {
                    this._text.text = this.ycManager.i18nManager.GetString(this._key);
                }
            }

        }

    }
}