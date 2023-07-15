using I2.Loc;
using KSP.Api.CoreTypes;
using KSP.UI.Binding;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class AppBarButton : MonoBehaviour
    {
        public event Action Destroying;

        public string Text
        {
            get { return _text.text; }
            set { _text.text = value; }
        }
        public Sprite Icon
        {
            get { return _image.sprite; }
            set
            {
                if (value is object)
                {
                    _image.sprite = value;
                    _iconAsset.SetActive(true);
                }
                else
                {
                    _iconAsset.SetActive(false);
                }
            }
        }
        public bool State
        {
            get { return _toggleExtended.isOn; }
            set { _writeBool.BindValue(new Property<bool>(value)); }
        }

        private GameObject _iconAsset;
        private Image _image;
        private UIValue_ReadBool_SetAlpha _buttonExtended;
        private ToggleExtended _toggleExtended;
        private TextMeshProUGUI _text;
        private UIValue_WriteBool_Toggle _writeBool;

        private void OnClick(bool state)
        {
            _buttonExtended.SetValue(false);
        }

        public static AppBarButton CreateButton(string id, string name, Action<bool> action, Sprite icon = null)
        {
            var buttonGroup = GameObject.Find("GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Scaled Popup Canvas/Container/ButtonBar/BTN-App-Tray/appbar-others-group");
            var copyit = buttonGroup?.GetChild("BTN-Resource-Manager");
            if (copyit is null) return null;
            var newbutton = Instantiate(copyit, buttonGroup.transform);
            newbutton.name = id;
            var text = newbutton.GetChild("Content").GetChild("TXT-title").GetComponent<TextMeshProUGUI>();
            text.text = name;
            var loc = text.gameObject.GetComponent<Localize>();
            if (loc is object) Destroy(loc);
            var toggle = newbutton.GetComponent<ToggleExtended>();
            var buttonbehaviour = newbutton.AddComponent<AppBarButton>();
            toggle.onValueChanged.AddListener(x => buttonbehaviour.OnClick(x));
            if (action is object) toggle.onValueChanged.AddListener(x => action(x));
            buttonbehaviour._writeBool = newbutton.GetComponent<UIValue_WriteBool_Toggle>();
            buttonbehaviour._writeBool.BindValue(new Property<bool>(false));
            buttonbehaviour._toggleExtended = toggle;
            buttonbehaviour._text = text;
            buttonbehaviour._buttonExtended = buttonGroup.GetComponent<UIValue_ReadBool_SetAlpha>();
            buttonbehaviour._iconAsset = newbutton.GetChild("Content").GetChild("GRP-icon").GetChild("ICO-asset");
            buttonbehaviour._image = buttonbehaviour._iconAsset.GetComponent<Image>();
            if (icon is object) buttonbehaviour._image.sprite = icon;
            else buttonbehaviour._iconAsset.SetActive(false);
            return buttonbehaviour;
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            Destroying?.Invoke();
        }
    }
}
