using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class ExpandCollapse : MonoBehaviour
    {
        bool _expanded;
        TextMeshProUGUI _text;
        GameObject _target;

        public void Initialize(bool expanded)
        {
            Initialize(expanded, null);
        }

        public void Initialize(bool expanded, GameObject target)
        {
            _expanded = expanded;
            _target = target ?? gameObject.transform.parent.parent.gameObject.GetChild("Bindings");
            _target.SetActive(expanded);
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
            _target.SetActive(expanded);
            _text.text = expanded ? "Collapse" : "Expand";
            _expanded = expanded;
        }
    }
}
