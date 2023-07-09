using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codenade.Inputbinder
{
    internal class Page2ListPopulator : MonoBehaviour
    {
        private void OnEnable()
        {
            Reload();
        }

        public void Reload()
        {
            for (var j = gameObject.transform.childCount - 1; j >= 0; j--)
                DestroyImmediate(gameObject.transform.GetChild(j).gameObject);
            foreach (var p in InputSystem.ListProcessors())
            {
                if (InputSystem.TryGetProcessor(p).BaseType != typeof(InputProcessor<float>))
                    continue;
                var temp = Instantiate(Inputbinder.Instance.BindingUI.Assets[BindingUI.PrefabKeys.ProcessorAddGroup], gameObject.transform);
                temp.GetChild("ProcessorName").GetComponent<TextMeshProUGUI>().text = p;
                temp.GetChild("ProcessorAddButton").AddComponent<ProcNextBehaviour>().Initialize(p);
            }
        }
    }
}
