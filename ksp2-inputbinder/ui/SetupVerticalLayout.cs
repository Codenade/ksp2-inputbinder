using UnityEngine;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    [RequireComponent(typeof(VerticalLayoutGroup))]
    internal class SetupVerticalLayout : MonoBehaviour
    {
        private void Awake()
        {
            var vlg = gameObject.GetComponent<VerticalLayoutGroup>();
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            Destroy(this);
        }
    }
}
