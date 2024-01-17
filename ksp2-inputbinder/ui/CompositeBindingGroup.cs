using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class CompositeBindingGroup : MonoBehaviour
    {
        private int _bindingIndex;
        private InputAction _action;
        private TextMeshProUGUI _txtProc;

        public void Initialize(InputAction action, int bindingIndex)
        {
            _action = action;
            _bindingIndex = bindingIndex;
            var bindingHeader = gameObject.GetChild("CompositeBindingHeader");
            bindingHeader.GetChild("ModifyBindingGroup").GetChild("ProcessorButton").GetComponent<Button>().onClick.AddListener(OnModifyClicked);
            bindingHeader.GetChild("BindingInfoGroup").GetChild("BindingName").GetComponent<TextMeshProUGUI>().text = action.bindings[bindingIndex].effectivePath;
            _txtProc = bindingHeader.GetChild("BindingInfoGroup").GetChild("BindingPath").GetComponent<TextMeshProUGUI>();
            _txtProc.text = action.bindings[bindingIndex].effectiveProcessors;
        }

        private void OnModifyClicked()
        {
            Inputbinder.Instance.ActionManager.ChangeProcessors(_action, _bindingIndex);
            if (Inputbinder.Instance.ActionManager.IsChangingProc) Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.ProcessorList);
        }

        private void Update()
        {
            _txtProc.text = _action.bindings[_bindingIndex].effectiveProcessors;
        }
    }
}
