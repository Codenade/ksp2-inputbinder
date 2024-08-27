using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class ProcRemoveBehaviour : MonoBehaviour
    {
        internal event Action<string> OnRemove;
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
            if (ProcessorUtilities.ExtractSingleProcessor(binding.overrideProcessors ?? "", _processorName, out var toRemove))
            {
                OnRemove?.Invoke(toRemove);
                binding.overrideProcessors = binding.overrideProcessors.Remove(binding.overrideProcessors.IndexOf(toRemove), toRemove.Length).Trim(';').Replace(";;", ";");
            }
            else
                return;
            if (binding.overrideProcessors == "")
                binding.overrideProcessors = null;
            Inputbinder.Instance.ActionManager.ProcBindInfo.Action.ApplyBindingOverride(Inputbinder.Instance.ActionManager.ProcBindInfo.BindingIndex, binding);
            Destroy(gameObject.transform.parent.gameObject);
        }
    }
}
