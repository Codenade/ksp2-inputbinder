using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputActionRebindingExtensions;

namespace Codenade.Inputbinder
{
    public class RebindInformation
    {
        public int BindingIndex => _bindingIndex;
        public bool WasEnabled => _wasEnabled;
        public InputBinding Binding => _operation.bindingMask ?? _operation.action.bindings[_bindingIndex];
        public RebindingOperation Operation => _operation;

        private readonly int _bindingIndex;
        private readonly bool _wasEnabled;
        private readonly RebindingOperation _operation;

        public RebindInformation(int bindingIndex, RebindingOperation operation, bool wasEnabled)
        {
            _bindingIndex = bindingIndex;;
            _operation = operation;
            _wasEnabled = wasEnabled;
        }

        public override string ToString() => $"{_operation.action.name}, index: {_bindingIndex}";
    }

    public class ProcRebindInformation
    {
        public int BindingIndex => _bindingIndex;
        public InputBinding Binding => _action.bindings[_bindingIndex];
        public InputAction Action => _action;
        public string ProcessorName { get; set; }
        public Dictionary<string, object> Values { get; }

        private readonly int _bindingIndex;
        private readonly InputAction _action;

        public ProcRebindInformation(int bindingIndex, InputAction action)
        {
            _bindingIndex = bindingIndex; ;
            _action = action;
            ProcessorName = "";
            Values = new Dictionary<string, object>();
        }
    }

    public class NamedInputAction
    {
        public InputAction Action { get; set; }
        public string Name { get { return Action.name; } }
        public string FriendlyName { get; set; }
        public bool IsFromGame { get; set; }
        public bool IsUiExtended { get; set; }

        public NamedInputAction(InputAction action, bool isFromGame = false)
        {
            Action = action;
            FriendlyName = action.name;
            IsFromGame = isFromGame;
            IsUiExtended = Constants.UiActionsExtendDefault;
        }

        public NamedInputAction(InputAction action, string friendlyName, bool isFromGame = false)
        {
            Action = action;
            FriendlyName = friendlyName;
            IsFromGame = isFromGame;
            IsUiExtended = Constants.UiActionsExtendDefault;
        }
    }
}
