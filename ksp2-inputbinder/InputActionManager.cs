using Castle.Core.Internal;
using KSP.Game;
using KSP.IO;
using KSP.Logging;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;
using UnityEngine.InputSystem.Controls;
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

        public void AddAction(InputAction action, bool isFromGame = false) => AddAction(action, action.name, isFromGame);

        public void AddAction(InputAction action, string friendlyName, bool isFromGame = false)
        {
            if (action.expectedControlType == "Vector2")
            {
                for (var j = action.bindings.Count - 1; j >= 0; j--)
                    action.ChangeBinding(j).Erase();
                action.AddCompositeBinding("2DVector")
                    .With("left", "")
                    .With("right", "")
                    .With("down", "")
                    .With("up", "");
                if (action == GameManager.Instance.Game.Input.Flight.CameraZoom)
                {
                    action.AddCompositeBinding("2DAxis")
                        .With("x", "")
                        .With("y", "/Mouse/scroll/y");
                    try
                    {
                        var ovrd = action.bindings[5];
                        ovrd.overrideProcessors = "ScaleVector2(x=0,y=0.0005)";
                        action.ApplyBindingOverride(5, ovrd);
                    }
                    catch (Exception e)
                    {
                        GlobalLog.Error(LogFilter.UserMod, $"[{Constants.Name}] {e}");
                    }
                }
                else
                    action.AddCompositeBinding("2DAxis")
                        .With("x", "")
                        .With("y", "");
            }
            Actions.Add(action.name, new NamedInputAction(action, friendlyName, isFromGame));
            if (Inputbinder.Instance.BindingUI is object && Inputbinder.Instance.BindingUI.IsVisible)
            {
                Inputbinder.Instance.BindingUI.Hide();
                Inputbinder.Instance.BindingUI.Show();
            }
        }

        public bool ContainsAction(string name) => Actions.ContainsKey(name);

        public bool TryGetAction(string name, out NamedInputAction val) => Actions.TryGetValue(name, out val);

        public void RemoveAction(InputAction action) => Actions.Remove(action.name);

        public void RemoveAction(string name) => Actions.Remove(name);

        public void Rebind(InputAction action, int bindingIndex)
        {
            if (IsCurrentlyRebinding || IsChangingProc)
                return;
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] Rebind starting");
            var wasEnabled = action.enabled;
            action.Disable();
            var operation = action.PerformInteractiveRebinding(bindingIndex)
                                      .OnComplete(result => BindingComplete())
                                      .OnCancel(result => BindingComplete())
                                      .WithoutGeneralizingPathOfSelectedControl()
                                      .WithControlsHavingToMatchPath("*")
                                      .WithCancelingThrough(Keyboard.current.escapeKey);
            // Workaround for change of expected control type for AxisComposite being changed from ButtonControl to AxisControl in InputSystem (1.3.0 -> 1.5.0):
            // Forcing Button control type
            if (action.bindings[bindingIndex].isPartOfComposite)
                if (InputSystem.TryGetBindingComposite(action.ChangeBinding(bindingIndex).PreviousCompositeBinding(null).binding.GetNameOfComposite()) == typeof(AxisComposite))
                    operation.WithExpectedControlType<ButtonControl>();
            operation.Start();
            _rebindInfo = new RebindInformation(bindingIndex, operation, wasEnabled);
            if (_rebindInfo?.Operation is object)
                Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.Rebinding);
        }

        public void BindingComplete()
        {
            if (!IsCurrentlyRebinding)
                return;
            Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.Default);
            var action = _rebindInfo.Operation.action;
            var bindingInfo = _rebindInfo.Binding;
            var wasEnabled = _rebindInfo.WasEnabled;
            _rebindInfo.Operation.Dispose();
            _rebindInfo = null;
            if (wasEnabled)
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

        public static void ClearBinding(InputBinding binding, InputAction action)
        {
            var modifiedBinding = binding;
            var idx = action.bindings.IndexOf(bdg => binding == bdg);
            modifiedBinding.overridePath = Constants.BindingClearPath;
            action.ApplyBindingOverride(idx, modifiedBinding);
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
            else if (action != _procBindInfo.Action || (bindingIndex >= 0 && _procBindInfo.Binding != action.bindings[bindingIndex]))
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
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] Loading settings ...");
            if (!IOProvider.FileExists(path))
                return new InputActionManager();
            var data = IOProvider.FromJsonFile<Dictionary<string, InputActionData>>(path);
            var manager = new InputActionManager();
            foreach (var input in data)
            {
                if (!input.Value.IsFromGame)
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
                                binding.overridePath = b.PathOverride.IsNullOrEmpty() ? null : b.PathOverride;
                                binding.overrideProcessors = b.ProcessorsOverride.IsNullOrEmpty() ? null : b.ProcessorsOverride;
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
                            var compositeSyntax = action.AddCompositeBinding(input.Value.Bindings[i].Name, processors: input.Value.Bindings[i].Processors);
                            while (binding_list.Count > 0)
                            {
                                var one = binding_list.Dequeue();
                                compositeSyntax.With(one.Name, one.Path);
                            }
                            for (var i1 = binding_comp_start; i1 < action.bindings.Count; i1++)
                            {
                                if (!input.Value.Bindings[i1].Override)
                                    continue;
                                var ovrd = action.bindings[i1];
                                ovrd.overridePath = input.Value.Bindings[i1].PathOverride.IsNullOrEmpty() ? null : input.Value.Bindings[i1].PathOverride;
                                ovrd.overrideProcessors = input.Value.Bindings[i1].ProcessorsOverride.IsNullOrEmpty() ? null : input.Value.Bindings[i1].ProcessorsOverride;
                                action.ApplyBindingOverride(i1, ovrd);
                            }
                        }
                    }
                    manager.AddAction(action, input.Value.FriendlyName);
                }
                else
                {
                    var action = GameManager.Instance.Game.Input.FindAction(input.Key, true);
                    for (int i = 0; i < input.Value.Bindings.Length; i++)
                    {
                        if (i < action.bindings.Count && input.Value.Bindings[i].Override)
                        {
                            var saved = input.Value.Bindings[i];
                            var binding = action.bindings[i];
                            binding.overridePath = saved.PathOverride.IsNullOrEmpty() ? null : saved.PathOverride;
                            binding.overrideProcessors = saved.ProcessorsOverride.IsNullOrEmpty() ? null : saved.ProcessorsOverride;
                            action.ApplyBindingOverride(i, binding);
                        }
                    }
                    manager.AddAction(action, true);
                }
            }
            return manager;
        }

        public bool SaveToJson(string path)
        {
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] Saving settings ...");
            if (IOProvider.FileExists(path))
                if (IOProvider.IsFileReadonly(path))
                {
                    GlobalLog.Error(LogFilter.UserMod, $"[{Constants.Name}] Cannot save settings, input.json is read-only");
                    return false;
                }
            var store = new Dictionary<string, InputActionData>();
            foreach (var nia in Actions)
            {
                var data = new InputActionData();
                var eAction = nia.Value;
                data.FriendlyName = eAction.FriendlyName;
                data.ActionType = eAction.Action.expectedControlType;
                data.IsFromGame = eAction.IsFromGame;
                var bindings = new BindingData[eAction.Action.bindings.Count];
                for (var i = 0; i < eAction.Action.bindings.Count; i++)
                {
                    var binding = new BindingData();
                    var eBinding = eAction.Action.bindings[i];
                    binding.Name = eBinding.name ?? "";
                    binding.IsComposite = eBinding.isComposite;
                    binding.IsPartOfComposite = eBinding.isPartOfComposite;
                    binding.Path = eBinding.path ?? "";
                    binding.Processors = eBinding.processors ?? "";
                    binding.Override = eBinding.hasOverrides;
                    binding.PathOverride = eBinding.overridePath ?? "";
                    binding.ProcessorsOverride = eBinding.overrideProcessors ?? "";
                    bindings[i] = binding;
                }
                data.Bindings = bindings;
                store.Add(nia.Key, data);
            }
            IOProvider.ToJsonFile(path, store);
            return true;
        }
    }
}
