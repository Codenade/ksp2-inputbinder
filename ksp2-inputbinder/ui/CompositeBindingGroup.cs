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

        public void Initialize(InputAction action, int bindingIndex)
        {
            _action = action;
            _bindingIndex = bindingIndex;
            var bindingHeader = gameObject.GetChild("CompositeBindingHeader");
            bindingHeader.GetChild("ModifyBindingGroup").GetChild("ProcessorButton").GetComponent<Button>().onClick.AddListener(OnModifyClicked);
            bindingHeader.GetChild("BindingInfoGroup").GetChild("BindingPath").GetComponent<TextMeshProUGUI>().text = action.bindings[bindingIndex].effectivePath;
        }

        private void OnModifyClicked()
        {
            Inputbinder.Instance.ActionManager.ChangeProcessors(_action, _bindingIndex);
            if (Inputbinder.Instance.ActionManager.IsChangingProc) Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.ProcessorList);
        }
    }
}
