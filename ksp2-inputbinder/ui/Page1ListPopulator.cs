using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codenade.Inputbinder
{
    internal class Page1ListPopulator : MonoBehaviour
    {
        private TextMeshProUGUI _textValue;
        private TextMeshProUGUI[] _intermediates;
        private string[] _processors;
        private InputAction _action;

        private void Awake()
        {
            _textValue = transform.parent.Find("TextValue").GetComponent<TextMeshProUGUI>();
        }

        private unsafe void ValueUpdate(InputAction.CallbackContext ctx)
        {
            object originalValue;
            var propInfo = typeof(InputControl).GetProperty("currentStatePtr", BindingFlags.Instance | BindingFlags.NonPublic);
            var propVal = propInfo.GetMethod.Invoke(ctx.control, null);
            void* ptr = System.Reflection.Pointer.Unbox(propVal);
            originalValue = ctx.control.ReadValueFromStateAsObject(ptr);
            _textValue.text = originalValue.ToString() ?? string.Empty;
            if (originalValue is object)
            {
                int idx = 0;
                foreach (var i in _intermediates)
                {
                    if (idx == _intermediates.Length - 1)
                        originalValue = _action.ReadValueAsObject();
                    else
                        originalValue = ProcessorUtilities.Process(_processors[idx], originalValue, ctx.control);
                    i.text = originalValue?.ToString() ?? string.Empty;
                    idx++;
                }
            }
        }

        private void OnEnable()
        {
            Reload();
            _action.started += ValueUpdate;
            _action.performed += ValueUpdate;
            _action.canceled += ValueUpdate;
        }

        private void OnDisable()
        {
            _action.started -= ValueUpdate;
            _action.performed -= ValueUpdate;
            _action.canceled -= ValueUpdate;
        }

        public void Reload()
        {
            for (var j = gameObject.transform.childCount - 1; j >= 0; j--)
                DestroyImmediate(gameObject.transform.GetChild(j).gameObject);
            _action = Inputbinder.Instance.ActionManager.ProcBindInfo.Action;
            _processors = (Inputbinder.Instance.ActionManager.ProcBindInfo.Binding.overrideProcessors ?? string.Empty).Split(';');
            _intermediates = new TextMeshProUGUI[_processors.Length];
            int idx = 0;
            foreach (var p in _processors)
            {
                if (p.IsNullOrEmpty() || p.IndexOf('(') < 0 || p.IndexOf(')') < 0)
                    continue;
                var temp = Instantiate(Inputbinder.Instance.BindingUI.Assets[BindingUI.PrefabKeys.ProcessorGroup], gameObject.transform);
                var strt = p.IndexOf('(');
                var name = p.Substring(0, strt);
                var values = p.Substring(strt).Trim('(', ')');
                var infoGroup = temp.GetChild("ProcessorInfoGroup");
                infoGroup.GetChild("ProcessorName").GetComponent<TextMeshProUGUI>().text = name;
                infoGroup.GetChild("ProcessorValues").GetComponent<TextMeshProUGUI>().text = values;
                temp.GetChild("ProcessorEditButton").AddComponent<ProcNextBehaviour>().Initialize(name);
                temp.GetChild("ProcessorRemoveButton").AddComponent<ProcRemoveBehaviour>().Initialize(name);
                var intermediateValueGameObject = Instantiate(_textValue.gameObject, gameObject.transform);
                intermediateValueGameObject.name = p + " Intermediate";
                _intermediates[idx] = intermediateValueGameObject.GetComponent<TextMeshProUGUI>();
                idx++;
            }
        }
    }
}
