using KSP;
using KSP.Game;
using KSP.IO;
using KSP.Logging;
using KSP.Modding;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codenade.Inputbinder
{
    public class BindingUI : KerbalMonoBehaviour
    {
        // TODO: Add Binding editor and Interactions
        // TODO: Migrate to other UI system
        // TODO: Lock game input while interacting with ui

        private Rect _windowRect = new Rect(400, 300, 100, 400);
        private Rect _windowProcRect = new Rect(0, 0, 300, 100);
        private float _maxWidth = 0f;
        private int _procFlowPos = 0;
        private string _procTemp = "";
        private Dictionary<string, object> _procValStore = null;
        private Vector2 _scrollPos = Vector2.zero;
        private InputActionManager _actionManager;
        private KSP2Mod _mod;

        public BindingUI()
        {
            _procValStore = new Dictionary<string, object>();
        }

        private void Awake()
        {
            _actionManager = Inputbinder.Instance.ActionManager;
            _mod = Inputbinder.Instance.Mod;
        }

        public void Show()
        {
            enabled = true;
        }

        public void Hide()
        {
            _actionManager.CancelBinding();
            _actionManager.CompleteChangeProcessors();
            enabled = false;
        }

        private void OnGUI()
        {
            var lblStyle = new GUIStyle(GUI.skin.label)
            {
                stretchHeight = true,
                stretchWidth = true,
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleLeft
            };
            var scrollStyle = new GUIStyle(GUI.skin.scrollView)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };
            _windowRect = GUILayout.Window(925380980, _windowRect, idMain =>
            {
                if (_actionManager.Actions is object)
                {
                    _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true, GUILayout.MinWidth(_maxWidth + 24));
                    var actionNum = 1;
                    foreach (var item in _actionManager.Actions)
                    {
                        for (var idx = 0; idx < item.Value.Action.bindings.Count; idx++)
                        {
                            var binding = item.Value.Action.bindings[idx];
                            if (idx == 0)
                            {
                                GUILayout.BeginHorizontal();
                                GUILayout.Label(item.Value.FriendlyName, GUILayout.Width(150));
                                if (GUILayout.Button(item.Value.IsUiExtended ? "Collapse" : "Extend"))
                                    item.Value.IsUiExtended = !item.Value.IsUiExtended;
                                GUILayout.FlexibleSpace();
                                GUILayout.EndHorizontal();
                                var lastItemWidth = GUILayoutUtility.GetLastRect().width;
                                _maxWidth = lastItemWidth > _maxWidth ? lastItemWidth : _maxWidth;
                            }
                            if (!item.Value.IsUiExtended)
                                break;
                            else
                            {
                                GUILayout.BeginHorizontal();
                                var treeLbl = "";
                                if (!binding.isComposite && !binding.isPartOfComposite)
                                    treeLbl = idx < item.Value.Action.bindings.Count - 1 ? "├" : "└";
                                else if (binding.isComposite)
                                {
                                    var hasMoreComposite = false;
                                    for (var i = idx + 1; i < item.Value.Action.bindings.Count; i++)
                                    {
                                        if (item.Value.Action.bindings[i].isComposite)
                                        {
                                            hasMoreComposite = true;
                                            break;
                                        }
                                    }
                                    treeLbl = hasMoreComposite ? "├" : "└";
                                }
                                else
                                {
                                    var nextIsPart = idx + 1 < item.Value.Action.bindings.Count;
                                    if (nextIsPart) 
                                        nextIsPart = item.Value.Action.bindings[idx + 1].isPartOfComposite;
                                    var hasMoreComposite = false;
                                    for (var i = idx + 1; i < item.Value.Action.bindings.Count; i++)
                                    {
                                        if (item.Value.Action.bindings[i].isComposite)
                                        {
                                            hasMoreComposite = true;
                                            break;
                                        }
                                    }
                                    treeLbl = (hasMoreComposite ? "│" : "  ") + " " + (nextIsPart ? "├" : "└");
                                }
                                GUILayout.Label($"{treeLbl} {(binding.isComposite ? "composite" : binding.name)}", lblStyle, GUILayout.Width(300));
                                GUILayout.FlexibleSpace();
                                string pathStr = binding.effectivePath;
                                if (_actionManager.IsCurrentlyRebinding)
                                    pathStr = (_actionManager.RebindInfo.Binding == binding) ? "Press ESC to cancel" : pathStr;
                                if (!binding.isComposite)
                                    if (GUILayout.Button(new GUIContent(pathStr, binding.isComposite ? "Cannot rebind composite root" : "Click to change binding"), GUILayout.Width(150)) && !binding.isComposite)
                                        _actionManager.Rebind(item.Value.Action, idx);
                                if (!binding.isPartOfComposite)
                                {
                                    if (GUILayout.Button(new GUIContent("Processors", "Change input modifiers"), GUILayout.Width(100)))
                                        _actionManager.ChangeProcessors(item.Value.Action, idx);
                                }
                                else
                                    GUILayout.Space(100);
                                GUILayout.EndHorizontal();
                                var lastItemWidth = GUILayoutUtility.GetLastRect().width;
                                _maxWidth = lastItemWidth > _maxWidth ? lastItemWidth : _maxWidth;
                            }
                        }
                        if (actionNum < _actionManager.Actions.Count)
                            GUILayout.Space(10);
                        actionNum++;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndScrollView();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Save"))
                        _actionManager.SaveToJson(IOProvider.JoinPath(_mod.ModRootPath, "input.json"));
                    if (GUILayout.Button("Close"))
                        Hide();
                    GUILayout.EndHorizontal();
                }
                GUI.DragWindow();
            }, "Inputbinder");
            if (_actionManager.IsChangingProc)
            {
                var pwtxt = "";
                switch (_procFlowPos)
                {
                    case 0:
                        pwtxt = $"Change input modifiers of {_actionManager.ProcBindInfo.Action.name}";
                        break;
                    case 1:
                        pwtxt = $"Select a modifier to add to {_actionManager.ProcBindInfo.Action.name}";
                        break;
                    case 2:
                        pwtxt = $"Properties of processor {_procTemp} from {_actionManager.ProcBindInfo.Action.name}";
                        break;
                    default:
                        pwtxt = "Change input modifiers";
                        break;
                }
                _windowProcRect = GUILayout.Window(213124455, new Rect(_windowRect.x + _windowRect.width, _windowRect.y, _windowProcRect.width, _windowProcRect.height),
                idProc =>
                {
                    GUILayout.BeginVertical();
                    if (_procFlowPos == 0)
                    {
                        if (_actionManager.ProcBindInfo.Binding.overrideProcessors is object)
                        {
                            foreach (var proc in _actionManager.ProcBindInfo.Binding.effectiveProcessors.Split(';'))
                            {
                                if (proc == "")
                                    continue;
                                GUILayout.BeginHorizontal();
                                if (GUILayout.Button(proc))
                                {
                                    _procFlowPos = 6;
                                    _procTemp = proc.Substring(0, proc.IndexOf('('));
                                    _procValStore.Clear();
                                    var st = proc.IndexOf('(') + 1;
                                    foreach (var prop in proc.Substring(st, proc.IndexOf(')') - st).Split(','))
                                    {
                                        string[] spl = prop.Split('=');
                                        string name = spl[0];
                                        string val = spl[1];
                                        if (float.TryParse(val, out float val_float))
                                        {
                                            _procValStore.Add(name, val_float);
                                            continue;
                                        }
                                        if (bool.TryParse(val, out bool val_bool))
                                            _procValStore.Add(name, val_bool);
                                    }
                                }
                                if (GUILayout.Button("-", GUILayout.Width(20)))
                                {
                                    var binding = _actionManager.ProcBindInfo.Binding;
                                    binding.overrideProcessors = binding.overrideProcessors.Replace(proc, "").Trim(';');
                                    _actionManager.ProcBindInfo.Action.ApplyBindingOverride(_actionManager.ProcBindInfo.BindingIndex, binding);
                                }
                                GUILayout.EndHorizontal();
                            }
                        }
                        if (GUILayout.Button("Add Processor")) _procFlowPos = 1;
                    }
                    else if (_procFlowPos == 1)
                    {
                        foreach (var avail in InputSystem.ListProcessors())
                        {
                            if (InputSystem.TryGetProcessor(avail).BaseType != typeof(InputProcessor<float>))
                                continue;
                            if (GUILayout.Button(avail))
                            {
                                _procTemp = avail;
                                _procValStore.Clear();
                                _procFlowPos = 2;
                                break;
                            }
                        }
                    }
                    else if (_procFlowPos == 2 || _procFlowPos == 6)
                    {
                        var type = InputSystem.TryGetProcessor(_procTemp);
                        if (type is object)
                        {
                            foreach (var field in type.GetFields())
                            {
                                if (!field.IsPublic)
                                    continue;
                                if (field.FieldType.IsNumericType())
                                {
                                    float val = 0;
                                    if (_procValStore.ContainsKey(field.Name))
                                        val = (float)_procValStore[field.Name];
                                    else
                                        _procValStore.Add(field.Name, val);
                                    GUILayout.Label(field.Name);
                                    if (float.TryParse(GUILayout.TextField(val.ToString()), out var tval))
                                        val = tval;
                                    val = GUILayout.HorizontalSlider(val, -3, 3);
                                    GUILayout.Space(4);
                                    _procValStore[field.Name] = val;
                                }
                                else if (Type.GetTypeCode(field.FieldType) == TypeCode.Boolean)
                                {
                                    bool val = false;
                                    if (_procValStore.ContainsKey(field.Name))
                                    {
                                        val = (bool)_procValStore[field.Name];
                                    }
                                    else
                                    {
                                        _procValStore.Add(field.Name, val);
                                    }
                                    val = GUILayout.Toggle(val, field.Name);
                                    GUILayout.Space(4);
                                    _procValStore[field.Name] = val;
                                }
                            }
                        }
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("< Back"))
                    {
                        if (_procFlowPos < 1) _actionManager.CompleteChangeProcessors();
                        else if (_procFlowPos == 6) _procFlowPos = 0;
                        else _procFlowPos--;
                    }
                    if (_procFlowPos == 2)
                    {
                        if (GUILayout.Button("Add"))
                        {
                            var binding = _actionManager.ProcBindInfo.Binding;
                            if (binding.overrideProcessors is object)
                            {
                                if (binding.overrideProcessors.Length > 0)
                                    _procTemp = InputBinding.Separator + _procTemp;
                            }
                            _procTemp += '(';
                            int item_no = 1;
                            foreach (var ent in _procValStore)
                            {
                                _procTemp += $"{ent.Key}={ent.Value}{(item_no == _procValStore.Count ? "" : ",")}";
                                item_no++;
                            }
                            _procTemp += ')';
                            if (binding.overrideProcessors is object)
                                binding.overrideProcessors += _procTemp;
                            else
                                binding.overrideProcessors = _procTemp;
                            _actionManager.ProcBindInfo.Action.ApplyBindingOverride(_actionManager.ProcBindInfo.BindingIndex, binding);
                            _procTemp = "";
                            _procValStore.Clear();
                            _procFlowPos = 0;
                        }
                    }
                    if (_procFlowPos == 6)
                    {
                        if (GUILayout.Button("Apply"))
                        {
                            var binding = _actionManager.ProcBindInfo.Binding;
                            string original = $"{_procTemp}()";
                            string func_name = _procTemp;
                            if (binding.overrideProcessors is object)
                            {
                                original = binding.overrideProcessors;
                            }
                            _procTemp += '(';
                            int item_no = 1;
                            foreach (var ent in _procValStore)
                            {
                                _procTemp += $"{ent.Key}={ent.Value}{(item_no == _procValStore.Count ? "" : ",")}";
                                item_no++;
                            }
                            _procTemp += ')';
                            var start_idx = original.IndexOf(func_name);
                            var end_idx = original.IndexOf(')', start_idx);
                            binding.overrideProcessors = original.Replace(original.Substring(start_idx, end_idx - start_idx + 1), _procTemp);
                            _actionManager.ProcBindInfo.Action.ApplyBindingOverride(_actionManager.ProcBindInfo.BindingIndex, binding);
                            _procTemp = "";
                            _procValStore.Clear();
                            _procFlowPos = 0;
                        }
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }, new GUIContent(pwtxt));
            }
        }
    }
}
