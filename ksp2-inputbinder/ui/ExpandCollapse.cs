using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class ExpandCollapse : MonoBehaviour
    {
        bool _expanded;
        TextMeshProUGUI _text;
        GameObject _bindings;

        public void Initialize(bool expanded)
        {
            _expanded = expanded;
            _bindings = gameObject.transform.parent.parent.gameObject.GetChild("Bindings");
            _bindings.SetActive(expanded);
            _text = GetComponentInChildren<TextMeshProUGUI>();
            if (_expanded)
                _text.text = "Collapse";
            else
                _text.text = "Expand";
            GetComponent<Button>().onClick.AddListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            SetExpanded(!_expanded);
        }

        public void SetExpanded(bool expanded)
        {
            _bindings.SetActive(expanded);
            _text.text = expanded ? "Collapse" : "Expand";
            _expanded = expanded;
        }
    }
}
