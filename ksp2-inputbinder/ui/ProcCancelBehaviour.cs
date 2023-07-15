using UnityEngine;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class ProcCancelBehaviour : MonoBehaviour
    {
        private void OnEnable()
        {
            GetComponent<Button>().onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            GetComponent<Button>().onClick.RemoveListener(OnClick);
        }

        private void OnClick()
        {
            switch (Inputbinder.Instance.BindingUI.CurrentStatus)
            {
                case BindingUI.Status.ProcessorList:
                    Inputbinder.Instance.ActionManager.CompleteChangeProcessors();
                    Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.Default);
                    break;
                case BindingUI.Status.ProcessorAdd:
                    Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.ProcessorList);
                    break;
                case BindingUI.Status.ProcessorConfirm:
                    Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.ProcessorAdd);
                    break;
                case BindingUI.Status.ProcessorEdit:
                    Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.ProcessorList);
                    break;
            }
        }
    }
}
