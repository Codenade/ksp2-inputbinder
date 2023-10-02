using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class SaveAsDialogBehaviour : MonoBehaviour
    {
        private TMP_InputField _tbName;
        private GameObject _warnExist;
        private Button _btnNeg;
        private Button _btnPos;
        private Toggle _tglDef;

        private void Awake()
        {
            var dialogT = transform.Find("Dialog/");
            _tbName = dialogT.Find("NameBox").GetComponent<TMP_InputField>();
            _warnExist = dialogT.Find("TextExists").gameObject;
            var grpBtn = dialogT.Find("ButtonGroup");
            _btnNeg = grpBtn.Find("Negative").GetComponent<Button>();
            _btnPos = grpBtn.Find("Positive").GetComponent<Button>();
            _tglDef = dialogT.Find("SetDefault").GetComponent<Toggle>();
            _tbName.onValueChanged.AddListener(TextInputChanged);
            _tbName.onSelect.AddListener(InputFieldSelected);
            _tbName.onDeselect.AddListener(InputFieldDeselected);
            _tbName.inputValidator = ScriptableObject.CreateInstance<FilenameValidator>();
            _btnNeg.onClick.AddListener(OnNegative);
            _btnPos.onClick.AddListener(OnPositive);
            _btnPos.interactable = false;
            _warnExist.SetActive(false);
            _tglDef.isOn = false;
        }

        private void OnDisable()
        {
            _tbName.text = string.Empty;
            _btnPos.interactable = false;
            _warnExist.SetActive(false);
            _tglDef.isOn = false;
        }

        private void OnNegative()
        {
            Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.Default);
        }

        private void OnPositive()
        {
            var am = Inputbinder.Instance.ActionManager;
            am.ProfileName = _tbName.text;
            if (am.SaveOverrides() && _tglDef.isOn)
            {
                GlobalConfiguration.DefaultProfile = _tbName.text;
                GlobalConfiguration.Save(Path.Combine(BepInEx.Paths.ConfigPath, "inputbinder/inputbinder.cfg"));
            }
            Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.Default);
        }

        private void TextInputChanged(string value)
        {
            if (value != string.Empty)
            {
                _warnExist.SetActive(Inputbinder.Instance.ActionManager.CheckProfileExists(value));
                _btnPos.interactable = true;
            }
            else
            {
                _warnExist.SetActive(false);
                _btnPos.interactable = false;
            }
        }

        private void InputFieldSelected(string value) => InputSystem.DisableDevice(Keyboard.current.device);

        private void InputFieldDeselected(string value) => InputSystem.EnableDevice(Keyboard.current.device);
    }

    internal class FilenameValidator : TMP_InputValidator
    {
        public override char Validate(ref string text, ref int pos, char ch)
        {
            bool invalid = Path.GetInvalidFileNameChars().Contains(ch) || Path.GetInvalidPathChars().Contains(ch);
            if (!invalid)
            {
                text = text.Insert(pos, ch.ToString());
                pos++;
            }
            return invalid ? '\0' : ch;
        }
    }
}
