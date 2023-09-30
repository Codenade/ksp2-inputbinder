using KSP.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codenade.Inputbinder
{
    internal class ProfileDefinitions
    {
        public static bool LoadDefault(Dictionary<string, NamedInputAction> actions, string path)
        {
            return LoadVersion1(actions, path);
        }

        public static bool LoadVersion1(Dictionary<string, NamedInputAction> actions, string path)
        {
            var content = IOProvider.FromJsonFile<InputProfileData>(path);
            if (content.FileVersion == default)
            {
                QLog.Warn("Failed to load profile with file version 1");
                return false;
            }
            var data = content.Actions;
            foreach (var input in data)
            {
                if (!actions.TryGetValue(input.Key, out var matchedAction))
                {
                    QLog.WarnLine($"No match for key {input.Key}");
                    continue;
                }
                for (var ib = 0; ib < input.Value.Bindings.Length && ib < matchedAction.Action.bindings.Count; ib++)
                {
                    var bdg = matchedAction.Action.bindings[ib];
                    if (bdg.isComposite)
                    {
                        bdg.overrideProcessors = input.Value.Bindings[ib].OverrideProcessors;
                        matchedAction.Action.ApplyBindingOverride(ib, bdg);
                    }
                    else if (bdg.isPartOfComposite)
                    {
                        bdg.overridePath = input.Value.Bindings[ib].OverridePath;
                        matchedAction.Action.ApplyBindingOverride(ib, bdg);
                    }
                    else
                    {
                        bdg.overridePath = input.Value.Bindings[ib].OverridePath;
                        bdg.overrideProcessors = input.Value.Bindings[ib].OverrideProcessors;
                        matchedAction.Action.ApplyBindingOverride(ib, bdg);
                    }
                }
            }
            return true;
        }

        public static bool LoadLegacy(Dictionary<string, NamedInputAction> actions, string path)
        {
            var data = IOProvider.FromJsonFile<Dictionary<string, LegacyInputActionData>>(path);
            foreach (var input in data)
            {
                if (!actions.TryGetValue(input.Key, out var matchedAction))
                {
                    QLog.WarnLine($"No match for key {input.Key}");
                    continue;
                }
                for (var ib = 0; ib < input.Value.Bindings.Length && ib < matchedAction.Action.bindings.Count; ib++)
                {
                    if (input.Value.Bindings[ib].Override)
                    {
                        var bdg = matchedAction.Action.bindings[ib];
                        if (bdg.isComposite)
                        {
                            bdg.overrideProcessors = input.Value.Bindings[ib].ProcessorsOverride;
                            matchedAction.Action.ApplyBindingOverride(ib, bdg);
                        }
                        else if (bdg.isPartOfComposite)
                        {
                            if (input.Value.Bindings[ib].Name != bdg.name)
                            {
                                int prevCompositBindingIdx = matchedAction.Action.GetPreviousCompositeBinding(ib);
                                if (prevCompositBindingIdx != -1)
                                {
                                    var fi = matchedAction.Action.FindNamedCompositePart(prevCompositBindingIdx, input.Value.Bindings[ib].Name);
                                    if (fi != -1)
                                    {
                                        bdg = matchedAction.Action.bindings[fi];
                                        bdg.overridePath = input.Value.Bindings[ib].PathOverride;
                                        matchedAction.Action.ApplyBindingOverride(fi, bdg);
                                    }
                                }
                            }
                            else
                            {
                                bdg.overridePath = input.Value.Bindings[ib].PathOverride;
                                matchedAction.Action.ApplyBindingOverride(ib, bdg);
                            }
                        }
                        else
                        {
                            bdg.overridePath = input.Value.Bindings[ib].PathOverride;
                            bdg.overrideProcessors = input.Value.Bindings[ib].ProcessorsOverride;
                            matchedAction.Action.ApplyBindingOverride(ib, bdg);
                        }
                    }
                }
            }
            return true;
        }

        public static bool SaveOverrides(Dictionary<string, NamedInputAction> actions, string path)
        {
            QLog.Info("Saving settings ...");
            var stopwatch = Stopwatch.StartNew();
            if (IOProvider.FileExists(path))
                if (IOProvider.IsFileReadonly(path))
                {
                    stopwatch.Stop();
                    QLog.Error("Cannot save settings, input.json is read-only");
                    return false;
                }
            var actions_data = new Dictionary<string, InputActionData>();
            foreach (var nia in actions)
            {
                if (!nia.Value.Action.HasAnyOverrides())
                    continue;
                var data = new InputActionData();
                var eAction = nia.Value;
                var bindings = new BindingData[eAction.Action.bindings.Count];
                for (var i = 0; i < eAction.Action.bindings.Count; i++)
                {
                    var binding = new BindingData();
                    var eBinding = eAction.Action.bindings[i];
                    binding.OverridePath = eBinding.overridePath;
                    binding.OverrideProcessors = eBinding.overrideProcessors;
                    bindings[i] = binding;
                }
                data.Bindings = bindings;
                actions_data.Add(nia.Key, data);
            }
            var content = new InputProfileData
            {
                FileVersion = 1,
                GameVersion = Application.version,
                Timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds(),
                Actions = actions_data
            };
            IOProvider.ToJsonFile(path, content);
            stopwatch.Stop();
            QLog.Info($"Saved settings ({stopwatch.Elapsed.TotalSeconds}s)");
            return true;
        }
    }
}
