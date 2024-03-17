using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class BindingGroup : MonoBehaviour
    {
        private int _bindingIndex;
        private InputAction _action;
        private TextMeshProUGUI _pathTxt;
        private TextMeshProUGUI _procText;
        private string _toMark;

        public void Initialize(InputAction action, int bindingIndex)
        {
            _toMark = null;
            _action = action;
            _bindingIndex = bindingIndex;
            var bindingInfoGroup = gameObject.GetChild("BindingInfoGroup");
            var modifyBindingGroup = gameObject.GetChild("ModifyBindingGroup");
            bindingInfoGroup.GetChild("BindingName").GetComponent<TextMeshProUGUI>().text = action.bindings[bindingIndex].name;
            _pathTxt = bindingInfoGroup.GetChild("BindingPath").GetComponent<TextMeshProUGUI>();
            _pathTxt.text = action.bindings[bindingIndex].effectivePath;
            _procText = bindingInfoGroup.GetChild("BindingProcessors").GetComponent<TextMeshProUGUI>();
            _procText.text = action.bindings[bindingIndex].effectiveProcessors;
            if (action.bindings[bindingIndex].isPartOfComposite)
            {
                modifyBindingGroup.GetChild("ProcessorButton").SetActive(false);
                bindingInfoGroup.GetChild("BindingProcessors").SetActive(false);
            }
            else
                modifyBindingGroup.GetChild("ProcessorButton").GetComponent<Button>().onClick.AddListener(OnModifyClicked);
            modifyBindingGroup.GetChild("RebindButton").GetComponent<Button>().onClick.AddListener(OnRebindClicked);
            modifyBindingGroup.GetChild("ClearButton").GetComponent<Button>().onClick.AddListener(OnClearClicked);
        }

        public void Initialize(InputAction action, int bindingIndex, string toMark)
        {
            _toMark = toMark;
            _action = action;
            _bindingIndex = bindingIndex;
            var bindingInfoGroup = gameObject.GetChild("BindingInfoGroup");
            var modifyBindingGroup = gameObject.GetChild("ModifyBindingGroup");
            bindingInfoGroup.GetChild("BindingName").GetComponent<TextMeshProUGUI>().text = action.bindings[bindingIndex].name;
            _pathTxt = bindingInfoGroup.GetChild("BindingPath").GetComponent<TextMeshProUGUI>();
            _pathTxt.text = Utils.MarkText(action.bindings[bindingIndex].effectivePath, toMark);
            _procText = bindingInfoGroup.GetChild("BindingProcessors").GetComponent<TextMeshProUGUI>();
            _procText.text = action.bindings[bindingIndex].effectiveProcessors;
            if (action.bindings[bindingIndex].isPartOfComposite)
            {
                modifyBindingGroup.GetChild("ProcessorButton").SetActive(false);
                bindingInfoGroup.GetChild("BindingProcessors").SetActive(false);
            }
            else
                modifyBindingGroup.GetChild("ProcessorButton").GetComponent<Button>().onClick.AddListener(OnModifyClicked);
            modifyBindingGroup.GetChild("RebindButton").GetComponent<Button>().onClick.AddListener(OnRebindClicked);
            modifyBindingGroup.GetChild("ClearButton").GetComponent<Button>().onClick.AddListener(OnClearClicked);
        }

        private void Update()
        {
            _pathTxt.text = Utils.MarkText(_action.bindings[_bindingIndex].effectivePath, _toMark);
            _procText.text = _action.bindings[_bindingIndex].effectiveProcessors;
        }

        private void OnRebindClicked()
        {
            Inputbinder.Instance.ActionManager.Rebind(_action, _bindingIndex);
        }

        private void OnModifyClicked()
        {
            Inputbinder.Instance.ActionManager.ChangeProcessors(_action, _bindingIndex);
            if (Inputbinder.Instance.ActionManager.IsChangingProc) Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.ProcessorList);
        }

        private void OnClearClicked()
        {
            InputActionManager.ClearBinding(_action.bindings[_bindingIndex], _action);
        }
    }
}
