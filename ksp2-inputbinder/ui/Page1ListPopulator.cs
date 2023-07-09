using Castle.Core.Internal;
using TMPro;
using UnityEngine;

namespace Codenade.Inputbinder
{
    internal class Page1ListPopulator : MonoBehaviour
    {
        private void OnEnable()
        {
            Reload();
        }

        public void Reload()
        {
            for (var j = gameObject.transform.childCount - 1; j >= 0; j--)
                DestroyImmediate(gameObject.transform.GetChild(j).gameObject);
            foreach (var p in (Inputbinder.Instance.ActionManager.ProcBindInfo.Binding.overrideProcessors ?? "").Split(';'))
            {
                if (p.IsNullOrEmpty() || p.IndexOf('(') < 0 || p.IndexOf(')') < 0)
                    continue;
                var temp = Instantiate(Inputbinder.Instance.BindingUI.Assets[BindingUI.PrefabKeys.ProcessorGroup], gameObject.transform);
                var strt = p.IndexOf('(');
                var name = p.Substring(0, strt);
                var values = p.Substring(strt).Trim('(', ')');
                var infoGroup = temp.GetChild("ProcessorInfoGroup");
                infoGroup.GetChild("ProcessorName").GetComponent<TextMeshProUGUI>().text = name;
                infoGroup.GetChild("ProcessorValues").GetComponent<TextMeshProUGUI>().text = values;
                temp.GetChild("ProcessorEditButton").AddComponent<ProcNextBehaviour>().Initialize(name);
                temp.GetChild("ProcessorRemoveButton").AddComponent<ProcRemoveBehaviour>().Initialize(name);
            }
        }
    }
}
