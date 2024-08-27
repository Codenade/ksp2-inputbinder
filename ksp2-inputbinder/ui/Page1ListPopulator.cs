using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class Page1ListPopulator : MonoBehaviour
    {
        private static Sprite sprtRing;
        private static Sprite sprtRingTop;
        private static Sprite sprtRingBottom;
        private static Sprite sprtRingBoth;
        private static List<Image> waitingForBoth = new List<Image>();
        private static List<Image> waitingForBottom = new List<Image>();

        private TextMeshProUGUI _value;
        private TextMeshProUGUI[] _intermediates;
        private string[] _processors;
        private InputAction _action;
        private MethodInfo _currentStatePointerInfo;

        private void Awake()
        {
            _currentStatePointerInfo = typeof(InputControl).GetProperty("currentStatePtr", BindingFlags.Instance | BindingFlags.NonPublic).GetMethod;
            _value = transform.parent.Find("ValueGroup/Value").gameObject.GetComponent<TextMeshProUGUI>();
            var catOp = Addressables.LoadContentCatalogAsync(Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Path.Join("addressables", "catalog.json")));
            catOp.Completed += CatalogLoaded;

            void CatalogLoaded(AsyncOperationHandle<IResourceLocator> hOpc)
            {
                if (hOpc.Status == AsyncOperationStatus.Succeeded)
                {
                    var resLoc = hOpc.Result;
                    Addressables.LoadAssetAsync<Sprite>("Codenade.Inputbinder/Icons/ring.png").Completed += (x) => AssetLoaded(x, ref sprtRing);
                    Addressables.LoadAssetAsync<Sprite>("Codenade.Inputbinder/Icons/ring-top.png").Completed += (x) => AssetLoaded(x, ref sprtRingTop);
                    Addressables.LoadAssetAsync<Sprite>("Codenade.Inputbinder/Icons/ring-bottom.png").Completed += (x) => AssetLoaded(x, ref sprtRingBottom);
                    Addressables.LoadAssetAsync<Sprite>("Codenade.Inputbinder/Icons/ring-both.png").Completed += (x) => AssetLoaded(x, ref sprtRingBoth);
                }
                else
                    QLog.Error("Page1ListPopulator could not load Catalog!");
            }

            void AssetLoaded(AsyncOperationHandle<Sprite> hOp, ref Sprite sprt)
            {
                if (hOp.Status == AsyncOperationStatus.Succeeded)
                    sprt = hOp.Result;
                else
                    QLog.Error("Page1ListPopulator could not load Asset!");
                if (sprtRing is object && sprtRingTop is object && sprtRingBottom is object && sprtRingBoth is object)
                {
                    _value.transform.parent.Find("Icon").GetComponent<Image>().sprite = sprtRingTop;
                    foreach (var img in waitingForBottom)
                        img.sprite = sprtRingBottom;
                    foreach (var img in waitingForBoth)
                        img.sprite = sprtRingBoth;
                }
            }
        }


        private unsafe void ValueUpdate(InputAction.CallbackContext ctx)
        {
            object originalValue = ctx.control.ReadValueFromStateAsObject(System.Reflection.Pointer.Unbox(_currentStatePointerInfo.Invoke(ctx.control, null)));
            if (originalValue is object)
            {
                _value.text = originalValue.ToString();
                int idx = 0;
                foreach (var i in _intermediates)
                {
                    if (idx == _intermediates.Length - 1)
                        originalValue = $"{ctx.control.GetMinValue()} {_action.ReadValueAsObject()} {ctx.control.GetMaxValue()}";
                    else
                        originalValue = ProcessorUtilities.Process(_processors[idx], originalValue, ctx.control);
                    i.text = originalValue?.ToString() ?? string.Empty;
                    idx++;
                }
            }
            else
            {
                _value.text = string.Empty;
                foreach (var lbl in _intermediates)
                    lbl.text = string.Empty;
            }
        }

        private void OnEnable()
        {
            Reload();
            if (GlobalConfiguration.ShowValuesInProcessorsSection)
            {
                _action.started += ValueUpdate;
                _action.performed += ValueUpdate;
                _action.canceled += ValueUpdate;
            }
        }

        private void OnDisable()
        {
            if (GlobalConfiguration.ShowValuesInProcessorsSection)
            {
                _action.started -= ValueUpdate;
                _action.performed -= ValueUpdate;
                _action.canceled -= ValueUpdate;
            }
        }

        public void Reload()
        {
            for (var j = gameObject.transform.childCount - 1; j >= 0; j--)
                DestroyImmediate(gameObject.transform.GetChild(j).gameObject);

            _action = Inputbinder.Instance.ActionManager.ProcBindInfo.Action;
            _processors = (Inputbinder.Instance.ActionManager.ProcBindInfo.Binding.overrideProcessors ?? string.Empty).Split(';');
            if (_processors.Length == 1 && _processors[0] == string.Empty)
                _processors = new string[0];
            if (GlobalConfiguration.ShowValuesInProcessorsSection)
                _intermediates = new TextMeshProUGUI[_processors.Length];
            else
                _intermediates = new TextMeshProUGUI[0];

            int idx = 0;
            foreach (var p in _processors)
            {
                if (p == string.Empty)
                    continue;
                var temp = Instantiate(Inputbinder.Instance.BindingUI.Assets[BindingUI.PrefabKeys.ProcessorGroup], gameObject.transform);
                var strt = p.IndexOf('(');
                var name = p.Substring(0, strt);
                var values = p.Substring(strt).Trim('(', ')');
                var infoGroup = temp.GetChild("ProcessorInfoGroup");
                infoGroup.GetChild("ProcessorName").GetComponent<TextMeshProUGUI>().text = name;
                infoGroup.GetChild("ProcessorValues").GetComponent<TextMeshProUGUI>().text = values;
                temp.GetChild("ProcessorEditButton").AddComponent<ProcNextBehaviour>().Initialize(name);
                var procRemove = temp.GetChild("ProcessorRemoveButton").AddComponent<ProcRemoveBehaviour>();
                procRemove.Initialize(name);
                procRemove.OnRemove += ProcRemove_OnRemove;
                if (GlobalConfiguration.ShowValuesInProcessorsSection)
                {
                    var intermediateValueGameObject = Instantiate(_value.transform.parent.gameObject, gameObject.transform);
                    intermediateValueGameObject.name = p + "_Intermediate";
                    _intermediates[idx] = intermediateValueGameObject.transform.Find("Value").GetComponent<TextMeshProUGUI>();
                    if (idx == _processors.Length - 1)
                    {
                        var img = _intermediates[idx].transform.parent.Find("Icon").GetComponent<Image>();
                        if (sprtRingBottom == null)
                            waitingForBottom.Add(img);
                        else
                            img.sprite = sprtRingBottom;
                    }
                    else
                    {
                        var img = _intermediates[idx].transform.parent.Find("Icon").GetComponent<Image>();
                        if (sprtRingBoth == null)
                            waitingForBoth.Add(img);
                        else
                            img.sprite = sprtRingBoth;
                    }
                }
                idx++;
            }
            _value.text = string.Empty;
            foreach (var lbl in _intermediates)
                lbl.text = string.Empty;
        }

        private void ProcRemove_OnRemove(string toRemove)
        {
            if (!GlobalConfiguration.ShowValuesInProcessorsSection)
                return;
            var newProcessors = new List<string>();
            var newIntermediates = new List<TextMeshProUGUI>();
            var idx = 0;
            foreach (var proc in _processors)
            {
                if (proc != toRemove)
                {
                    newProcessors.Add(proc);
                    newIntermediates.Add(_intermediates[idx]);
                }
                else
                    Destroy(_intermediates[idx].transform.parent.gameObject);
                idx++;
            }
            _processors = newProcessors.ToArray();
            _intermediates = newIntermediates.ToArray();
            _intermediates[^1].transform.parent.Find("Icon").GetComponent<Image>().sprite = sprtRingBottom;
        }
    }
}
