using Newtonsoft.Json;
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
    }

    public class ProcRebindInformation
    {
        public int BindingIndex => _bindingIndex;
        public InputBinding Binding => _action.bindings[_bindingIndex];
        public InputAction Action => _action;

        private readonly int _bindingIndex;
        private readonly InputAction _action;

        public ProcRebindInformation(int bindingIndex, InputAction action)
        {
            _bindingIndex = bindingIndex; ;
            _action = action;
        }
    }

    public class NamedInputAction
    {
        public InputAction Action { get; set; }
        public string Name { get { return Action.name; } }
        public string FriendlyName { get; set; }
        public bool IsFromGame { get; set; }

        public NamedInputAction(InputAction action, bool isFromGame = false)
        {
            Action = action;
            FriendlyName = action.name;
            IsFromGame = isFromGame;
        }

        public NamedInputAction(InputAction action, string friendlyName, bool isFromGame = false)
        {
            Action = action;
            FriendlyName = friendlyName;
            IsFromGame = isFromGame;
        }
    }

    public class InputActionData
    {
        [JsonProperty("friendly_name")]
        public string FriendlyName { get; set; }
        [JsonProperty("action_type")]
        public string ActionType { get; set; }
        [JsonProperty("is_from_game")]
        public bool IsFromGame { get; set; }
        [JsonProperty("bindings")]
        public BindingData[] Bindings { get; set; }
    }

    public class BindingData
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("is_composite")]
        public bool IsComposite { get; set; }
        [JsonProperty("is_part_of_composite")]
        public bool IsPartOfComposite { get; set; }
        [JsonProperty("path")]
        public string Path { get; set; }
        [JsonProperty("processors")]
        public string Processors { get; set; }
        [JsonProperty("override")]
        public bool Override { get; set; }
        [JsonProperty("path_override")]
        public string PathOverride { get; set; }
        [JsonProperty("processors_override")]
        public string ProcessorsOverride { get; set; }
    }
}
