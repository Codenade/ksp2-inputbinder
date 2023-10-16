using KSP.IO;
using System;
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
        /// <summary>
        /// Use this event to add your <see cref="InputAction"/>s before a profile is loaded.
        /// </summary>
        public static event Action<InputActionManager> BeforeRegisterActions;

        /// <summary>
        /// Inputbinder's registered <see cref="InputAction"/>s.
        /// </summary>
        public Dictionary<string, NamedInputAction> Actions { get; private set; }

        /// <summary>
        /// Is the user currently rebinding?
        /// </summary>
        /// <value>
        /// <c>false</c> if <see cref="RebindInfo"/> is <c>null</c>
        /// </value>
        public bool IsCurrentlyRebinding => _rebindInfo is object;

        /// <summary>
        /// Is the user currently editing processors?
        /// </summary>
        /// <value>
        /// <c>false</c> if <see cref="ProcBindInfo"/> is <c>null</c>
        /// </value>
        public bool IsEditingProcessors => _procBindInfo is object;

        /// <summary>
        /// This property is used to store information about an ongoing rebind operation.
        /// </summary>
        public RebindInformation RebindInfo => _rebindInfo;

        /// <summary>
        /// This property is used to store information about editing processors. Mostly used by the mod's GUI.
        /// </summary>
        public ProcEditInformation ProcBindInfo => _procBindInfo;

        /// <summary>
        /// The path to the directory where Inputbinder stores profiles.
        /// </summary>
        public string ProfileBasePath { get; set; } = Path.Combine(BepInEx.Paths.ConfigPath, "inputbinder/profiles");
        
        /// <summary>
        /// The name of the profile to use when loading or saving.
        /// </summary>
        public string ProfileName { get; set; } = GlobalConfiguration.DefaultProfile;

        /// <summary>
        /// The file extension to use for profile files.
        /// </summary>
        public string ProfileExtension { get; set; } = ".json";

        private RebindInformation _rebindInfo;
        private ProcEditInformation _procBindInfo;

        internal InputActionManager()
        {
            Actions = new Dictionary<string, NamedInputAction>();
            _rebindInfo = null;
            _procBindInfo = null;
            RegisterActions();
        }

        internal void Rebind(InputAction action, int bindingIndex)
        {
            if (IsCurrentlyRebinding || IsEditingProcessors)
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

        internal void BindingComplete()
        {
            if (!IsCurrentlyRebinding)
                return;
            Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.Default);
            var action = _rebindInfo.Operation.action;
            var bindingInfo = _rebindInfo.Binding;
            var wasEnabled = _rebindInfo.WasEnabled;
            var controlA = _rebindInfo.Operation.selectedControl.layout;
            var controlB = _rebindInfo.Operation.expectedControlType;
            HandleAutoAddingProcessors();
            _rebindInfo.Operation.Dispose();
            _rebindInfo = null;
            if (wasEnabled)
                action.Enable();
            QLog.Debug($"Binding complete: {action.name} {bindingInfo.name} with path {bindingInfo.effectivePath}; bound control of type {controlA} to binding of type {controlB}");
        }

        private void HandleAutoAddingProcessors()
        {
            var rbInfo = _rebindInfo;
            var op = rbInfo.Operation;
            var aapBindings = GlobalConfiguration.aapBindings;
            foreach (var aapb in aapBindings)
            {
                if ((aapb.A == "*" || aapb.A == "Any" || aapb.A == op.selectedControl.layout) && (aapb.B == "*" || aapb.B == "Any" || aapb.B == op.expectedControlType))
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

        internal void CancelBinding()
        {
            if (!IsCurrentlyRebinding)
                return;
            _rebindInfo.Operation.Cancel();
            BindingComplete();
        }

        internal void ChangeProcessors(InputAction action) => ChangeProcessors(action, -1);

        internal void ChangeProcessors(InputAction action, int bindingIndex)
        {
            if (IsCurrentlyRebinding)
                return;
            if (!IsEditingProcessors)
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
                _procBindInfo = new ProcEditInformation(chgIdx, action);
            }
            else if (action != _procBindInfo.Action || (bindingIndex >= 0 && _procBindInfo.Binding != action.bindings[bindingIndex]))
            {
                CompleteChangeProcessors();
                ChangeProcessors(action, bindingIndex);
            }
            else
                CompleteChangeProcessors();
        }

        internal void CompleteChangeProcessors()
        {
            _procBindInfo = null;
        }

        private void RegisterActions()
        {
            BeforeRegisterActions?.Invoke(this);
            foreach (var wia in DefaultInputActionDefinitions.WrappedInputActions)
            {
                wia.Setup?.Invoke(wia);
                Actions.Add(wia.InputAction.name, new NamedInputAction(wia.InputAction, wia.FriendlyName, wia.Source == ActionSource.Game));
            }
            foreach (var wia in GameInputUtils.LoadGameActionsToAdd(IOProvider.JoinPath(BepInEx.Paths.ConfigPath, "inputbinder/game_actions_to_add.txt")))
            {
                wia.Setup?.Invoke(wia);
                Actions.Add(wia.InputAction.name, new NamedInputAction(wia.InputAction, wia.FriendlyName, wia.Source == ActionSource.Game));
            }
        }

        /// <summary>
        /// Load the profile set by <see cref="ProfileName"/>.
        /// </summary>
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

        /// <summary>
        /// Save the profile <see cref="ProfileName"/>.
        /// </summary>
        /// <returns></returns>
        public bool SaveOverrides()
        {
            string path = Path.Combine(ProfileBasePath, ProfileName + ProfileExtension);
            return ProfileDefinitions.SaveOverrides(Actions, path);
        }

        /// <summary>
        /// Check if a profile with the given <paramref name="name"/> exists.
        /// </summary>
        /// <param name="name">The profile name to check for.</param>
        /// <returns></returns>
        public bool CheckProfileExists(string name)
        {
            string path = Path.Combine(ProfileBasePath, name + ProfileExtension);
            return File.Exists(path);
        }

        /// <summary>
        /// Set all overrides to <c>null</c>.
        /// </summary>
        public void RemoveAllOverrides()
        {
            foreach (var action in Actions.Values)
                action.Action.RemoveAllBindingOverrides();
        }
    }
}
