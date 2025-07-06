using KSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UIElements;

namespace Codenade.Inputbinder.ui
{
    internal class ProcessorEditMenu : IMenuPanel
    {
        private readonly VisualElement _uiBase;
        private readonly Label _title;
        private readonly VisualElement _prList;
        private readonly List<PreprocItem> _pi;

        private WrappedInputAction _action;
        private int _bindingIndex;

        private bool _addingProc;

        internal ProcessorEditMenu(VisualElement uiBase, Button addBtn)
        {
            _addingProc = false;
            _uiBase = uiBase;
            _title = _uiBase.Q<Label>("Title");
            _prList = _uiBase.Q<VisualElement>("unity-content-container");
            _pi = new List<PreprocItem>();

            addBtn.clicked += ShowAddMenu;
        }

        internal void ShowFor(WrappedInputAction action, int bindingIndex)
        {
            _action = action;
            _bindingIndex = bindingIndex;

            Inputbinder.Instance.BindingUI.ActiveMenuPanel = this;
            Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.ProcessorEdit);

            _uiBase.style.display = DisplayStyle.Flex;

            _addingProc = false;
            _prList.Clear();
            _pi.Clear();

            int idx = 0;
            foreach (var i in (action.InputAction.bindings[bindingIndex].overrideProcessors ?? "").Split(';'))
            {
                if (i == "" || !i.Contains('(') || !i.Contains(')')) continue;

                VisualElement pe = AssetDb.PreprocEntryUITK.Entry.Instantiate();
                _prList.Add(pe);

                string name = i[..i.IndexOf('(')];
                PreprocItem pi = new PreprocItem(name);
                _pi.Add(pi);

                var fld = pe.Q<Foldout>("Foldout");
                fld.text = name;
                var r = pe.Q<Button>("RemoveBtn");
                r.clicked += () =>
                {
                    pe.RemoveFromHierarchy();
                    _pi.Remove(pi);
                    PreprocValuesUpdate();
                };

                string par = i[(i.IndexOf('(') + 1)..i.IndexOf(')')];

                var type = InputSystem.TryGetProcessor(name);
                if (type is null)
                    return;
                var fields = type.GetFields();
                pi.Variables.Clear();

                foreach (var arg in par.Split(','))
                {
                    var pair = arg.Split('=');
                    var ftype = typeof(object);
                    foreach (var f in fields)
                    {
                        if (f.Name == pair[0])
                        {
                            ftype = f.FieldType;
                            break;
                        }
                    }
                    if (ftype.IsNumericType())
                    {
                        if (float.TryParse(pair[1], out var result))
                        {
                            var sld = new Slider(pair[0], GlobalConfiguration.SliderMin, GlobalConfiguration.SliderMax);
                            sld.value = result;
                            sld.RegisterValueChangedCallback((v) => { (pi.Variables[pair[0]] as PreprocValueItemFloat).Value = v.newValue; PreprocValuesUpdate(); });
                            fld.Add(sld);
                            pi.Variables.Add(pair[0], new PreprocValueItemFloat(result, sld));
                        }
                        else
                            QLog.Warn($"[Processors] cannot parse {pair[0]} as float");
                        continue;
                    }
                    if (Type.GetTypeCode(ftype) == TypeCode.Boolean)
                    {
                        if (bool.TryParse(pair[1], out var result))
                        {
                            var tgl = new Toggle(pair[0]);
                            tgl.value = result;
                            tgl.RegisterValueChangedCallback((v) => { (pi.Variables[pair[0]] as PreprocValueItemBoolean).Value = v.newValue; PreprocValuesUpdate(); });
                            fld.Add(tgl);
                            pi.Variables.Add(pair[0], new PreprocValueItemBoolean(result, tgl));
                        }
                        else
                            QLog.Warn($"[Processors] cannot parse {pair[0]} as bool");
                        continue;
                    }
                    if (Type.GetTypeCode(ftype) == TypeCode.Object)
                        continue;
                    QLog.Warn($"[Processors] incompatible field type {ftype} of {pair[0]}");
                }

                idx++;
            }

            _title.text = $"Editing Pre-Processors for {action.FriendlyName}[{bindingIndex}]";
        }

        private void PreprocValuesUpdate()
        {
            var sp = _action.InputAction.bindings[_bindingIndex];
            sp.overrideProcessors = string.Join(';', _pi.Select((item) => item.ToString()));
            _action.InputAction.ApplyBindingOverride(_bindingIndex, sp);
        }

        private void ShowAddMenu()
        {
            Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.ProcessorAdd);
            _addingProc = true;

            _prList.Clear();

            var im = typeof(InputSystem).GetField("s_Manager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
            var col = im.GetType().GetField("m_Layouts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(im);
            Dictionary<InternedString, Type> types = (Dictionary<InternedString, Type>)col.GetType().GetField("layoutTypes", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(col);
            var t = types[new InternedString(_action.InputAction.expectedControlType)].BaseType;
            foreach (var p in InputSystem.ListProcessors())
            {
                for (var i = 0; i < 3 && t is object; i++)
                {
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(InputControl<>))
                        break;
                    t = t.BaseType;
                }
                if (t is null || !t.IsGenericType || t.GetGenericTypeDefinition() != typeof(InputControl<>) || InputSystem.TryGetProcessor(p).BaseType.GetGenericArguments()[0] != t.GetGenericArguments()[0])
                    continue;

                var ab = new Button(() =>
                {
                    var pi = new PreprocItem(p);

                    var type = InputSystem.TryGetProcessor(p);
                    if (type is null)
                        return;

                    foreach (var field in type.GetFields())
                    {
                        if (field.FieldType.IsNumericType())
                            pi.Variables.Add(field.Name, new PreprocValueItemFloat(0f, null));
                        else if (Type.GetTypeCode(field.FieldType) == TypeCode.Boolean)
                            pi.Variables.Add(field.Name, new PreprocValueItemBoolean(false, null));
                    }

                    _pi.Add(pi);

                    PreprocValuesUpdate();
                    Back();
                });
                ab.text = p;
                _prList.Add(ab);
            }
        }

        public bool Back()
        {
            if (!_addingProc)
            {
                _prList.Clear();
                _pi.Clear();
                _uiBase.style.display = DisplayStyle.None;
                return true;
            }
            else
            {
                _addingProc = false;
                ShowFor(_action, _bindingIndex);
                Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.ProcessorEdit);
                return false;
            }
        }

        internal interface IPreprocValueItem
        {
            public string Name { get; }
            public object TypelessValue { get; }
            public VisualElement VisualElement { get; }
            public Type Type { get; }

            public abstract string ToString();
        }

        internal class PreprocValueItemBoolean : IPreprocValueItem
        {
            public string Name { get; set; }
            public object TypelessValue => Value;
            public bool Value { get; set; }
            public Toggle VisualElement { get; set; }
            public Type Type => typeof(bool);

            VisualElement IPreprocValueItem.VisualElement => VisualElement;

            internal PreprocValueItemBoolean(bool initialValue, Toggle ve)
            {
                Value = initialValue;
                VisualElement = ve;
            }
            public override string ToString() => Value.ToString();
        }

        internal class PreprocValueItemFloat : IPreprocValueItem
        {
            public string Name { get; set; }
            public object TypelessValue => Value;
            public float Value { get; set; }
            public Slider VisualElement { get; set; }
            public Type Type => typeof(float);

            VisualElement IPreprocValueItem.VisualElement => VisualElement;

            internal PreprocValueItemFloat(float initialValue, Slider ve)
            {
                Value = initialValue;
                VisualElement = ve;
            }

            public override string ToString() => Value.ToString();
        }

        internal class PreprocItem
        {
            internal string Name { get; }
            internal Dictionary<string, IPreprocValueItem> Variables { get; }

            internal PreprocItem(string name)
            {
                Name = name;
                Variables = new Dictionary<string, IPreprocValueItem>();
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder(Name);
                sb.Append('(');
                bool first = true;
                foreach (var i in Variables)
                {
                    if (!first) sb.Append(',');
                    sb.Append(i.Key + "=" + i.Value.ToString());
                    first = false;
                }
                sb.Append(')');
                return sb.ToString();
            }
        }
    }
}
