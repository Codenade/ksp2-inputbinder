using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

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
            var im = typeof(InputSystem).GetField("s_Manager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
            var col = im.GetType().GetField("m_Layouts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(im);
            Dictionary<InternedString, Type> types = (Dictionary<InternedString, Type>)col.GetType().GetField("layoutTypes", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).GetValue(col);
            var t = types[new InternedString(Inputbinder.Instance.ActionManager.ProcBindInfo.Action.expectedControlType)].BaseType;
            foreach (var p in InputSystem.ListProcessors())
            {
                if (InputSystem.TryGetProcessor(p).BaseType.GetGenericArguments()[0] != t.GetGenericArguments()[0])
                    continue;
                var temp = Instantiate(Inputbinder.Instance.BindingUI.Assets[BindingUI.PrefabKeys.ProcessorAddGroup], gameObject.transform);
                temp.GetChild("ProcessorName").GetComponent<TextMeshProUGUI>().text = p;
                temp.GetChild("ProcessorAddButton").AddComponent<ProcNextBehaviour>().Initialize(p);
            }
        }
    }
}
