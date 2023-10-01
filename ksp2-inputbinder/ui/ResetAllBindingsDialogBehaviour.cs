using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class ResetAllBindingsDialogBehaviour : MonoBehaviour
    {
        Button pos;

        private void Awake()
        {
            var btnGrp = transform.Find("Dialog/Content/ButtonGroup");
            var btnPos = btnGrp.Find("Positive").gameObject;
            var btnNeg = btnGrp.Find("Negative").gameObject;
            pos = btnPos.GetComponent<Button>();
            pos.onClick.AddListener(PositiveClicked);
            btnNeg.GetComponent<Button>().onClick.AddListener(() => Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.Default));
            pos.interactable = false;
        }

        private void PositiveClicked()
        {
            Inputbinder.Instance.ActionManager.RemoveAllOverrides();
            Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.Default);
        }

        private IEnumerator ResetButtonEnabler()
        {
            pos.interactable = false;
            yield return new WaitForSecondsRealtime(1);
            pos.interactable = true;
        }

        private void OnEnable()
        {
            StartCoroutine(ResetButtonEnabler());
        }

        private void OnDisable()
        {
            StopCoroutine(ResetButtonEnabler());
            pos.interactable = false;
        }
    }
}
