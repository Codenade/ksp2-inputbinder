using I2.Loc;
using KSP.Api.CoreTypes;
using KSP.UI.Binding;
using System;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class AppBarButton : IDisposable
    {
        public bool Created { get { return _wasCreated; } }
        public string Text
        {
            get { return _button.GetChild("Content").GetChild("TXT-title").GetComponent<TextMeshProUGUI>().text; }
            set { _button.GetChild("Content").GetChild("TXT-title").GetComponent<TextMeshProUGUI>().text = value; }
        }
        public Sprite Icon
        {
            get { return _button.GetChild("Content").GetChild("GRP-icon").GetChild("ICO-asset").GetComponent<Image>().sprite; }
            set { _button.GetChild("Content").GetChild("GRP-icon").GetChild("ICO-asset").GetComponent<Image>().sprite = value; }
        }
        public bool State
        {
            get { return _button.GetComponent<ToggleExtended>().isOn; }
            set { _button.GetComponent<UIValue_WriteBool_Toggle>().BindValue(new Property<bool>(value)); }
        }

        private bool _wasCreated;
        private GameObject _button;

        public AppBarButton(string id, string text, Action<bool> action = null, Sprite icon = null)
        {
            _wasCreated = CreateButton(id, text, action, icon, out _button);
            _wasCreated &= _button is object;
        }

        private static bool CreateButton(string id, string name, Action<bool> action, Sprite icon, out GameObject button)
        {
            var appbargroup = GameObject.Find("GameManager/Default Game Instance(Clone)/UI Manager(Clone)/Popup Canvas/Container/ButtonBar/BTN-App-Tray/appbar-others-group");
            var copyit = appbargroup?.GetChild("BTN-Resource-Manager");
            button = null;
            if (copyit is null) return false;
            var newbutton = UnityEngine.Object.Instantiate(copyit, appbargroup.transform);
            newbutton.name = id;
            var text = newbutton.GetChild("Content").GetChild("TXT-title").GetComponent<TextMeshProUGUI>();
            text.text = name;
            var loc = text.gameObject.GetComponent<Localize>();
            if (loc is object) UnityEngine.Object.Destroy(loc);
            if (icon is object) newbutton.GetChild("Content").GetChild("GRP-icon").GetChild("ICO-asset").GetComponent<Image>().sprite = icon;
            var toggle = newbutton.GetComponent<ToggleExtended>();
            if (action is object) toggle.onValueChanged.AddListener(x => action(x));
            newbutton.GetComponent<UIValue_WriteBool_Toggle>().BindValue(new Property<bool>(false));
            button = newbutton;
            return true;
        }

        public void Dispose()
        {
            _wasCreated = false;
            if (_button is object) UnityEngine.Object.Destroy(_button);
            _button = null;
        }
    }
}
