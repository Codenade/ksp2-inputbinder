using KSP.IO;
using KSP.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputActionSetupExtensions;

namespace Codenade.Inputbinder
{
    public class InputActionManager
    {
        public Dictionary<string, NamedInputAction> Actions { get; private set; }
        public bool IsCurrentlyRebinding => _rebindInfo is object;
        public bool IsChangingProc => _procBindInfo is object;

        public RebindInformation RebindInfo => _rebindInfo;
        public ProcRebindInformation ProcBindInfo => _procBindInfo;

        private RebindInformation _rebindInfo;
        private ProcRebindInformation _procBindInfo;

        public InputActionManager()
        {
            Actions = new Dictionary<string, NamedInputAction>();
            _rebindInfo = null;
            _procBindInfo = null;
        }

        public void EnableAll()
        {
            foreach (var one in Actions.Values) one.Action.Enable();
        }

        public void DisableAll()
        {
            foreach (var one in Actions.Values) one.Action.Disable();
        }

        public void Add(InputAction action, bool isFromGame = false) => Add(action, action.name, isFromGame);

        public void Add(InputAction action, string friendlyName, bool isFromGame = false) => Actions.Add(action.name, new NamedInputAction(action, friendlyName, isFromGame));

        public void Bind(InputAction action, bool isFromGame = false) => Bind(action, action.name, isFromGame);

        public void Bind(InputAction action, string friendlyName, bool isFromGame = false)
        {
            if (!Actions.ContainsKey(action.name))
                Add(action, isFromGame);
            else
            {
                var stored = Actions[action.name];
                for (var i = 0; i < stored.Action.bindings.Count; i++)
                {
                    if (!stored.Action.bindings[i].isComposite)
                        ; // TODO: Support saving and loading of in-game bindings
                }
            }
        }

        public void Remove(InputAction action)
        {
            Actions.Remove(action.name);
        }

        public void Remove(string name)
        {
            Actions.Remove(name);
        }

        [Obsolete("Lacks bidning index")]
        public void Rebind(InputAction action) => Rebind(action, -1);

        public void Rebind(InputAction action, int bindingIndex)
        {
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] Rebind starting");
            if (IsCurrentlyRebinding || IsChangingProc)
                return;
            action.Disable();
            var operation = action.PerformInteractiveRebinding(bindingIndex)
                                      .OnComplete((result) => BindingComplete())
                                      .OnMatchWaitForAnother(0.1f)
                                      .WithCancelingThrough(Keyboard.current.escapeKey)
                                      .Start();
            _rebindInfo = new RebindInformation(bindingIndex, operation);
        }

        public void BindingComplete()
        {
            if (!IsCurrentlyRebinding)
                return;
            var action = _rebindInfo.Operation.action;
            var bindingInfo = _rebindInfo.Binding;
            _rebindInfo.Operation.Dispose();
            _rebindInfo = null;
            action.Enable();
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] Binding complete: {action.name} {bindingInfo.name} with path {bindingInfo.effectivePath}");
        }

        public void CancelBinding()
        {
            if (!IsCurrentlyRebinding)
                return;
            _rebindInfo.Operation.Cancel();
            BindingComplete();
        }

        public void ChangeProcessors(InputAction action) => ChangeProcessors(action, -1);

        public void ChangeProcessors(InputAction action, int bindingIndex)
        {
            if (IsCurrentlyRebinding)
                return;
            if (!IsChangingProc)
            {
                var chgIdx = 0;
                if (bindingIndex < 0)
                    for (var i = 0; i < action.bindings.Count; i++)
                    {
                        if (!(action.bindings[i].isComposite || action.bindings[i].isPartOfComposite))
                            chgIdx = i;
                        else
                            return;
                    }
                else
                    chgIdx = bindingIndex;
                _procBindInfo = new ProcRebindInformation(chgIdx, action);
            }
            else if (action != _procBindInfo.Action || (bindingIndex >= 0 ? _procBindInfo.Binding != action.bindings[bindingIndex] : false))
            {
                CompleteChangeProcessors();
                ChangeProcessors(action, bindingIndex);
            }
            else
                CompleteChangeProcessors();
        }

        public void CompleteChangeProcessors()
        {
            _procBindInfo = null;
        }

        public static InputActionManager LoadFromJson(string path)
        {
            // TODO: Handle file not existent
            var data = IOProvider.FromJsonFile<Dictionary<string, InputActionData>>(path);
            var manager = new InputActionManager();
            foreach (var input in data)
            {
                var action = new InputAction(input.Key);
                action.expectedControlType = input.Value.ActionType;
                for (var i = 0; i < input.Value.Bindings.Length; i++)
                {
                    var b = input.Value.Bindings[i];
                    if (b.IsPartOfComposite)
                        continue;
                    if (!b.IsComposite)
                    {
                        action.AddBinding()
                            .WithName(b.Name)
                            .WithPath(b.Path)
                            .WithProcessors(b.Processors);
                        if (b.Override)
                        {
                            var binding = action.bindings[i];
                            binding.overridePath = b.PathOverride;
                            binding.overrideProcessors = b.ProcessorsOverride;
                            action.ApplyBindingOverride(i, binding);
                        }
                    }
                    else
                    {
                        var binding_comp_start = i;
                        var binding_list = new Queue<BindingData>();
                        for (var i1 = binding_comp_start + 1; (i1 < input.Value.Bindings.Length) && input.Value.Bindings[i1].IsPartOfComposite; i1++)
                        {
                            binding_list.Enqueue(input.Value.Bindings[i1]);
                        }
                        var compositeSyntax = action.AddCompositeBinding("1DAxis");
                        while (binding_list.Count > 0)
                        {
                            var one = binding_list.Dequeue();
                            compositeSyntax.With(one.Name, one.Path);
                        }
                        for (var i1 = binding_comp_start + 1; i1 < action.bindings.Count; i1++)
                        {
                            if (!input.Value.Bindings[i1].Override)
                                continue;
                            var ovrd = action.bindings[i1];
                            ovrd.overridePath = input.Value.Bindings[i1].PathOverride;
                            ovrd.overrideProcessors = input.Value.Bindings[i1].ProcessorsOverride;
                            action.ApplyBindingOverride(ovrd);
                        }
                    }
                }
                manager.Add(action, input.Value.FriendlyName, input.Value.IsFromGame);
            }
            return manager;
        }

        public void SaveToJson(string path)
        {
            // TODO: handle game's bindings
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] Saving settings is disabled right now, it would lead to undefined behaviour");
            return;
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] Saving settings ...");
            var store = new Dictionary<string, InputActionData>();
            foreach (var nia in Actions)
            {
                var data = new InputActionData();
                data.FriendlyName = nia.Value.FriendlyName;
                data.ActionType = nia.Value.Action.expectedControlType;
                data.IsFromGame = nia.Value.IsFromGame;
                var b_arr = new BindingData[nia.Value.Action.bindings.Count];
                for (var y = 0; y < nia.Value.Action.bindings.Count; y++)
                {
                    b_arr[y].Name = nia.Value.Action.bindings[y].name;
                    b_arr[y].IsComposite = nia.Value.Action.bindings[y].isComposite;
                    b_arr[y].IsPartOfComposite = nia.Value.Action.bindings[y].isPartOfComposite;
                    b_arr[y].Path = nia.Value.Action.bindings[y].path;
                    b_arr[y].Processors = nia.Value.Action.bindings[y].processors;
                    b_arr[y].Override = nia.Value.Action.bindings[y].hasOverrides;
                    b_arr[y].PathOverride = nia.Value.Action.bindings[y].overridePath;
                    b_arr[y].ProcessorsOverride = nia.Value.Action.bindings[y].overrideProcessors;
                }
                store.Add(nia.Key, data);
            }
            IOProvider.ToJsonFile(path, store);
        }
    }
}
