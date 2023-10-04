using KSP.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        public string ProfileBasePath { get; set; } = Path.Combine(BepInEx.Paths.ConfigPath, "inputbinder/profiles");
        public string ProfileName { get; set; } = GlobalConfiguration.DefaultProfile;
        public string ProfileExtension { get; set; } = ".json";

        private RebindInformation _rebindInfo;
        private ProcRebindInformation _procBindInfo;

        public InputActionManager()
        {
            Actions = new Dictionary<string, NamedInputAction>();
            _rebindInfo = null;
            _procBindInfo = null;
            RegisterActions();
        }

        public void AddAction(InputAction action, bool isFromGame = false) => AddAction(action, action.name, isFromGame);

        public void AddAction(InputAction action, string friendlyName, bool isFromGame = false)
        {
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
            QLog.Debug("Rebind starting");
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
                    // TODO: Check if adding WithExpectedControlType(string) would improve anything
                    operation.WithExpectedControlType<ButtonControl>();
            // Always use control type AxisControl for added axis bindings
            if (action.bindings[bindingIndex].name == "Axis")
                // Both of the following WithExpectedControlType variants are important as they assign different internal variables
                operation.WithExpectedControlType<AxisControl>().WithExpectedControlType("Axis");
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
            HandleAutoAddingProcessors();
            _rebindInfo.Operation.Dispose();
            _rebindInfo = null;
            if (wasEnabled)
                action.Enable();
            QLog.Debug($"Binding complete: {action.name} {bindingInfo.name} with path {bindingInfo.effectivePath}");
        }

        private void HandleAutoAddingProcessors()
        {
            var rbInfo = _rebindInfo;
            var op = rbInfo.Operation;
            var aapBindings = GlobalConfiguration.aapBindings;
            foreach (var aapb in aapBindings)
            {
                if (aapb.A == op.selectedControl.layout && aapb.B == op.expectedControlType)
                {
                    var bi = rbInfo.BindingIndex;
                    var mod = op.action.bindings[bi];
                    if (mod.isPartOfComposite)
                    {
                        bi = GameInputUtils.GetPreviousCompositeBinding(op.action, bi);
                        if (bi == -1)
                            return;
                        mod = op.action.bindings[bi];
                    }
                    if (mod.overrideProcessors is null || mod.overrideProcessors == string.Empty)
                        mod.overrideProcessors = aapb.ProcessorsToAdd;
                    else if (!mod.overrideProcessors.Contains(aapb.ProcessorsToAdd))
                        mod.overrideProcessors += ';' + aapb.ProcessorsToAdd;
                    else
                        return;
                    op.action.ApplyBindingOverride(bi, mod);
                }
            }
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

        private void RegisterActions()
        {
            foreach (var wia in DefaultInputActionDefinitions.WrappedInputActions)
            {
                wia.Setup?.Invoke(wia);
                Actions.Add(wia.InputAction.name, new NamedInputAction(wia.InputAction, wia.FriendlyName, wia.Source == ActionSource.Game));
            }
            foreach (var wia in GameInputUtils.Load(IOProvider.JoinPath(BepInEx.Paths.ConfigPath, "inputbinder/game_actions_to_add.txt")))
            {
                wia.Setup?.Invoke(wia);
                Actions.Add(wia.InputAction.name, new NamedInputAction(wia.InputAction, wia.FriendlyName, wia.Source == ActionSource.Game));
            }
        }

        public void LoadOverrides()
        {
            string path = Path.Combine(ProfileBasePath, ProfileName + ProfileExtension);
            if (!IOProvider.FileExists(path))
                return;
            QLog.Info($"Loading settings ...");
            var stopwatch = Stopwatch.StartNew();
            bool flag = ProfileDefinitions.LoadDefault(Actions, path);
            if (!flag) flag = ProfileDefinitions.LoadLegacy(Actions, path);
            stopwatch.Stop();
            if (flag)
                QLog.Info($"Loaded settings ({stopwatch.Elapsed.TotalSeconds}s)");
            else
                QLog.Error($"Failed to load settings ({stopwatch.Elapsed.TotalSeconds}s)");
        }

        public bool SaveOverrides()
        {
            string path = Path.Combine(ProfileBasePath, ProfileName + ProfileExtension);
            return ProfileDefinitions.SaveOverrides(Actions, path);
        }

        public bool CheckProfileExists(string name)
        {
            string path = Path.Combine(ProfileBasePath, name + ProfileExtension);
            return File.Exists(path);
        }

        public void RemoveAllOverrides()
        {
            foreach (var action in Actions.Values)
                action.Action.RemoveAllBindingOverrides();
        }
    }
}
