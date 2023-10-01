using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class ProcRemoveBehaviour : MonoBehaviour
    {
        private string _processorName;

        private void Awake()
        {
            _processorName = "";
        }

        public void Initialize(string processorName)
        {
            _processorName = processorName;
        }

        private void OnEnable()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            if (_processorName == "")
                return;
            var binding = Inputbinder.Instance.ActionManager.ProcBindInfo.Binding;
            if (ProcessorUtilities.ExtractSingleProcessor(binding.overrideProcessors ?? "", _processorName, out var toReplace))
                binding.overrideProcessors = binding.overrideProcessors.Replace(toReplace, "").Trim(';');
            else
                return;
            if (binding.overrideProcessors == "")
                binding.overrideProcessors = null;
            Inputbinder.Instance.ActionManager.ProcBindInfo.Action.ApplyBindingOverride(Inputbinder.Instance.ActionManager.ProcBindInfo.BindingIndex, binding);
            Destroy(gameObject.transform.parent.gameObject);
        }
    }
}
