using KSP.IO;
using KSP.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UniLinq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Codenade.Inputbinder
{
    public class InputActionManager
    {
        public Dictionary<string, NamedInputAction> Actions { get; }
        public bool IsCurrentlyRebinding => _currentRebinding is object;
        public InputAction RebindingAction => _currentRebinding?.action;
        public bool IsChangingProc => _currentProcChanging is object;
        public InputAction ChgProcAction => _currentProcChanging;

        private InputActionRebindingExtensions.RebindingOperation _currentRebinding = null;
        private InputAction _currentProcChanging = null;

        public InputActionManager()
        {
            Actions = new Dictionary<string, NamedInputAction>();
        }

        public void EnableAll()
        {
            foreach (var one in Actions.Values) one.Action.Enable();
        }

        public void DisableAll()
        {
            foreach (var one in Actions.Values) one.Action.Disable();
        }

        public void AddInputAction(InputAction action)
        {
            Actions.Add(action.name, new NamedInputAction(action));
        }

        public void AddInputAction(InputAction action, string friendlyName)
        {
            Actions.Add(action.name, new NamedInputAction(action, friendlyName));
        }

        public void RemoveInputAction(InputAction action)
        {
            Actions.Remove(action.name);
        }

        public void Rebind(InputAction action)
        {
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] Rebind starting");
            if (_currentRebinding is object || _currentProcChanging is object)
                return;
            action.Disable();
            _currentRebinding = action.PerformInteractiveRebinding()
                                      .OnComplete((result) => BindingComplete())
                                      .OnMatchWaitForAnother(0.1f)
                                      .WithCancelingThrough("<Keyboard>/escape")
                                      .Start();
        }

        public void BindingComplete()
        {
            if (_currentRebinding is null)
                return;
            var action = _currentRebinding.action;
            _currentRebinding.Dispose();
            _currentRebinding = null;
            action.Enable();
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] Binding complete: {action.name} with path {action.bindings[0].effectivePath}");
        }

        public void CancelBinding()
        {
            if (_currentRebinding is null)
                return;
            _currentRebinding.Cancel();
            BindingComplete();
        }

        public bool ChangeProcessors(InputAction action)
        {
            if (_currentRebinding is object)
            {
                return false;
            }
            
            if (_currentProcChanging is null)
            {
                _currentProcChanging = action;
                return true;
            }
            else
            {
                if (action == _currentProcChanging)
                    return true;
            }
            return false;
        }

        public void CompleteChangeProcessors()
        {
            _currentProcChanging = null;
        }

        public static InputActionManager LoadFromJson(string path)
        {
            var data = IOProvider.FromJsonFile<Dictionary<string, InputActionData>>(path);
            var manager = new InputActionManager();
            foreach (var input in data)
            {
                var action = new InputAction(input.Key);
                action.AddBinding()
                    .WithPath(input.Value.Path)
                    .WithProcessors(input.Value.Processors);
                action.expectedControlType = input.Value.ActionType;
                if (input.Value.Override)
                {
                    var binding = new InputBinding()
                    {
                        overridePath = input.Value.PathOverride,
                        overrideProcessors = input.Value.ProcessorsOverride
                    };
                    action.ApplyBindingOverride(0, binding);
                }
                manager.AddInputAction(action, input.Value.FriendlyName);
            }
            return manager;
        }

        public void SaveToJson(string path)
        {
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] Saving settings ...");
            var store = new Dictionary<string, InputActionData>();
            foreach (var nia in Actions)
            {
                var data = new InputActionData();
                data.FriendlyName = nia.Value.FriendlyName;
                data.ActionType = nia.Value.Action.expectedControlType;
                data.Path = nia.Value.Action.bindings[0].path;
                data.Processors = nia.Value.Action.processors;
                data.Override = nia.Value.Action.bindings[0].hasOverrides;
                data.PathOverride = nia.Value.Action.bindings[0].overridePath;
                data.ProcessorsOverride = nia.Value.Action.bindings[0].overrideProcessors;
                store.Add(nia.Key, data);
            }
            IOProvider.ToJsonFile(path, store);
        }

        public class NamedInputAction
        {
            public InputAction Action { get; set; }
            public string Name { get { return Action.name; } }
            public string FriendlyName { get; set; }

            public NamedInputAction(InputAction action)
            {
                Action = action;
                FriendlyName = action.name;
            }

            public NamedInputAction(InputAction action, string friendlyName)
            {
                Action = action;
                FriendlyName = friendlyName;
            }
        }
    }

    public struct InputActionData
    {
        [JsonProperty("friendly_name")]
        public string FriendlyName { get; set; }
        [JsonProperty("action_type")]
        public string ActionType { get; set; }
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
