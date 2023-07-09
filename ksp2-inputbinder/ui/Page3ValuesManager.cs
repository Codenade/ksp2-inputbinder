using KSP;
using KSP.Logging;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class Page3ValuesManager : MonoBehaviour
    {
        private InputActionManager _actionManager;

        private void Awake()
        {
            _actionManager = Inputbinder.Instance.ActionManager;
        }

        private void OnEnable()
        {
            Reload();
        }

        public void Reload()
        {
            for (var j = gameObject.transform.childCount - 1; j >= 0; j--)
                DestroyImmediate(gameObject.transform.GetChild(j).gameObject);
            var type = InputSystem.TryGetProcessor(_actionManager.ProcBindInfo.ProcessorName);
            if (type is null)
                return;
            ParseFields(type);
            foreach (var f in type.GetFields())
            {
                if (f.FieldType.IsNumericType())
                {
                    var temp = Instantiate(Inputbinder.Instance.BindingUI.Assets[BindingUI.PrefabKeys.ProcessorValueGroup], gameObject.transform);
                    if (!_actionManager.ProcBindInfo.Values.ContainsKey(f.Name))
                        _actionManager.ProcBindInfo.Values.Add(f.Name, 0f);
                    temp.GetChild("ValueName").GetComponent<TextMeshProUGUI>().text = f.Name;
                    temp.AddComponent<ProcValueGroupControllerFloat>().Initialize(f.Name);
                }
                else if (Type.GetTypeCode(f.FieldType) == TypeCode.Boolean)
                {
                    var temp = Instantiate(Inputbinder.Instance.BindingUI.Assets[BindingUI.PrefabKeys.ProcessorValueGroupBool], gameObject.transform);
                    if (!_actionManager.ProcBindInfo.Values.ContainsKey(f.Name))
                        _actionManager.ProcBindInfo.Values.Add(f.Name, false);
                    temp.AddComponent<ProcValueGroupControllerBool>().Initialize(f.Name);
                }
            }
        }

        private void ParseFields(Type procType)
        {
            var fields = procType.GetFields();
            var input = ProcessorUtilities.ExtractSingleProcessor(_actionManager.ProcBindInfo.Binding.overrideProcessors ?? (_actionManager.ProcBindInfo.ProcessorName + "()"), _actionManager.ProcBindInfo.ProcessorName);
            var start = input.IndexOf('(') + 1;
            var temp = input.Substring(start, input.IndexOf(')') - start);
            var args = temp.Split(',');
            _actionManager.ProcBindInfo.Values.Clear();
            foreach (var arg in args)
            {
                var pair = arg.Split('=');
                var type = typeof(object);
                foreach (var f in fields)
                {
                    if (f.Name == pair[0])
                    {
                        type = f.FieldType;
                        break;
                    }    
                }
                if (type.IsNumericType())
                {
                    if (float.TryParse(pair[1], out var result))
                        _actionManager.ProcBindInfo.Values.Add(pair[0], result);
                    else
                        GlobalLog.Warn(LogFilter.UserMod, $"[{Constants.Name}] [Processors] cannot parse {pair[0]} as float");
                    continue;
                }
                if (Type.GetTypeCode(type) == TypeCode.Boolean)
                {
                    if (bool.TryParse(pair[1], out var result))
                        _actionManager.ProcBindInfo.Values.Add(pair[0], result);
                    else
                        GlobalLog.Warn(LogFilter.UserMod, $"[{Constants.Name}] [Processors] cannot parse {pair[0]} as bool");
                    continue;
                }
                if (Type.GetTypeCode(type) == TypeCode.Object)
                    continue;
                GlobalLog.Warn(LogFilter.UserMod, $"[{Constants.Name}] [Processors] incompatible field type {type} of {pair[0]}");
            }
        }
    }

    internal class ProcValueGroupControllerFloat : MonoBehaviour
    {
        private string _fieldName;

        public void Initialize(string fieldName)
        {
            _fieldName = fieldName;
            var slider = gameObject.GetChild("Slider").GetComponent<Slider>();
            slider.value= (float)Inputbinder.Instance.ActionManager.ProcBindInfo.Values[_fieldName];
            var inputField = gameObject.GetChild("InputField").GetComponent<TMP_InputField>();
            inputField.GetComponent<TMP_InputField>().text = ((float)Inputbinder.Instance.ActionManager.ProcBindInfo.Values[_fieldName]).ToString();
            slider.onValueChanged.AddListener(SliderChanged);
            inputField.onSubmit.AddListener(TextInputChanged);
            inputField.onSelect.AddListener(InputFieldSelected);
            inputField.onDeselect.AddListener(InputFieldDeselected);
        }

        private void SliderChanged(float value)
        {
            Inputbinder.Instance.ActionManager.ProcBindInfo.Values[_fieldName] = value;
            gameObject.GetChild("InputField").GetComponent<TMP_InputField>().SetTextWithoutNotify(value.ToString());
        }

        private void TextInputChanged(string value)
        {
            float convertedValue = (float)Inputbinder.Instance.ActionManager.ProcBindInfo.Values[_fieldName];
            if (!float.TryParse(value, out convertedValue))
                return;
            Inputbinder.Instance.ActionManager.ProcBindInfo.Values[_fieldName] = convertedValue;
            gameObject.GetChild("Slider").GetComponent<Slider>().SetValueWithoutNotify(convertedValue);
        }

        private void InputFieldSelected(string value)
        {
            InputSystem.DisableDevice(Keyboard.current.device);
        }

        private void InputFieldDeselected(string value)
        {
            TextInputChanged(gameObject.GetChild("InputField").GetComponent<TMP_InputField>().text);
            InputSystem.EnableDevice(Keyboard.current.device);
        }
    }

    internal class ProcValueGroupControllerBool : MonoBehaviour
    {
        private string _fieldName;

        public void Initialize(string fieldName)
        {
            _fieldName = fieldName;
            var toggle = gameObject.GetChild("Toggle").GetComponent<Toggle>();
            toggle.isOn = (bool)Inputbinder.Instance.ActionManager.ProcBindInfo.Values[_fieldName];
            toggle.onValueChanged.AddListener(OnToggle);
        }

        private void OnToggle(bool value)
        {
            Inputbinder.Instance.ActionManager.ProcBindInfo.Values[_fieldName] = value;
        }
    }
}
