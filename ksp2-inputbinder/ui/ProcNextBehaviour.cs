using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class ProcNextBehaviour : MonoBehaviour
    {
        private InputActionManager _actionManager;
        private string _processorName;

        private void Awake()
        {
            _processorName = "";
            _actionManager = Inputbinder.Instance.ActionManager;
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
            switch (Inputbinder.Instance.BindingUI.CurrentStatus)
            {
                case BindingUI.Status.ProcessorList:
                    if (_processorName == "") Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.ProcessorAdd);
                    else
                    {
                        _actionManager.ProcBindInfo.ProcessorName = _processorName;
                        Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.ProcessorEdit);
                    }
                    break;
                case BindingUI.Status.ProcessorAdd:
                    if (_processorName == "") QLog.Warn($"No processor name assigned to this {name}");
                    else
                    {
                        _actionManager.ProcBindInfo.ProcessorName = _processorName;
                        Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.ProcessorConfirm);
                    }
                    break;
                case BindingUI.Status.ProcessorConfirm:
                    SaveProcessor(true);
                    break;
                case BindingUI.Status.ProcessorEdit:
                    SaveProcessor(false);
                    break;
            }
        }

        private void SaveProcessor(bool add)
        {
            if (add)
            {
                var temp = _actionManager.ProcBindInfo.ProcessorName;
                var binding = _actionManager.ProcBindInfo.Binding;
                if (binding.overrideProcessors is object)
                {
                    if (binding.overrideProcessors.Length > 0)
                        temp = InputBinding.Separator + temp;
                }
                temp += '(';
                int item_no = 1;
                foreach (var ent in _actionManager.ProcBindInfo.Values)
                {
                    temp += $"{ent.Key}={ent.Value}{(item_no == _actionManager.ProcBindInfo.Values.Count ? "" : ",")}";
                    item_no++;
                }
                temp += ')';
                if (binding.overrideProcessors is object)
                    binding.overrideProcessors += temp;
                else
                    binding.overrideProcessors = temp;
                _actionManager.ProcBindInfo.Action.ApplyBindingOverride(_actionManager.ProcBindInfo.BindingIndex, binding);
                _actionManager.ProcBindInfo.ProcessorName = "";
                _actionManager.ProcBindInfo.Values.Clear();
                Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.ProcessorList);
            }
            else
            {
                var temp = _actionManager.ProcBindInfo.ProcessorName;
                var binding = _actionManager.ProcBindInfo.Binding;
                string original = $"{temp}()";
                if (binding.overrideProcessors is object)
                {
                    original = binding.overrideProcessors;
                }
                temp += '(';
                int item_no = 1;
                foreach (var ent in _actionManager.ProcBindInfo.Values)
                {
                    temp += $"{ent.Key}={ent.Value}{(item_no == _actionManager.ProcBindInfo.Values.Count ? "" : ",")}";
                    item_no++;
                }
                temp += ')';
                var start_idx = original.IndexOf(_actionManager.ProcBindInfo.ProcessorName);
                var end_idx = original.IndexOf(')', start_idx);
                binding.overrideProcessors = original.Replace(original.Substring(start_idx, end_idx - start_idx + 1), temp);
                _actionManager.ProcBindInfo.Action.ApplyBindingOverride(_actionManager.ProcBindInfo.BindingIndex, binding);
                _actionManager.ProcBindInfo.ProcessorName = "";
                _actionManager.ProcBindInfo.Values.Clear();
                Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.ProcessorList);
            }
            KSP.Game.GameManager.Instance.Game.SettingsMenuManager.ShowChangesAppliedNotification();
        }
    }
}
