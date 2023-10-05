using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class ProfileLoadDialogBehaviour : MonoBehaviour
    {
        protected GameObject _ldGrp;
        protected GameObject _delGrp;
        protected TextMeshProUGUI _lblDel;
        protected Button _btnDelPos;
        protected string _delName;

        private void Awake()
        {
            var dialog = transform.Find("Dialog");
            var grpMain = dialog.Find("SubgrpMain");
            var grpLd = grpMain.Find("GrpLd");
            var grpDel = grpMain.Find("GrpDel");
            _delGrp = grpDel.gameObject;
            _ldGrp = grpLd.gameObject;
            var btnRet = dialog.Find("BtnRet").GetComponent<Button>();
            btnRet.onClick.AddListener(Return);
            var profileListPop = grpLd.Find("List").gameObject.AddComponent<ProfileListPopulator>();
            profileListPop.DeletionRequested += DeletionRequested;
            _lblDel = grpDel.Find("WarnDel").GetComponent<TextMeshProUGUI>();
            var grpBtnDel = grpDel.Find("BtnGrpDel");
            _btnDelPos = grpBtnDel.Find("Positive").GetComponent<Button>();
            _btnDelPos.onClick.AddListener(DeletionConfirmed);
            _btnDelPos.interactable = false;
            var btnDelNeg = grpBtnDel.Find("Negative").GetComponent<Button>();
            btnDelNeg.onClick.AddListener(DeletionCanceled);
        }

        private void DeletionRequested(string name)
        {
            _delName = name;
            _ldGrp.SetActive(false);
            _lblDel.text = "Do you want to delete \"" + name + "\"?";
            _delGrp.SetActive(true);
            StartCoroutine(DelayBtnDelay());
        }

        private void DeletionConfirmed()
        {
            InputActionManager am = Inputbinder.Instance.ActionManager;
            try
            {
                File.Delete(Path.Combine(am.ProfileBasePath, _delName + am.ProfileExtension));
            }
            catch (Exception e)
            {
                QLog.Error("Could not delete profile \"" + _delName + "\": " + e.ToString());
            }
            _btnDelPos.interactable = false;
            _delGrp.SetActive(false);
            _ldGrp.SetActive(true);
        }

        private void DeletionCanceled()
        {
            StopCoroutine(DelayBtnDelay());
            _btnDelPos.interactable = false;
            _delGrp.SetActive(false);
            _ldGrp.SetActive(true);
        }

        private void OnDisable()
        {
            StopCoroutine(DelayBtnDelay());
            _btnDelPos.interactable = false;
            _delGrp.SetActive(false);
            _ldGrp.SetActive(true);
        }

        private void Return()
        {
            Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.Default);
        }

        private IEnumerator DelayBtnDelay()
        {
            _btnDelPos.interactable = false;
            yield return new WaitForSecondsRealtime(0.5f);
            _btnDelPos.interactable = true;
        }
    }
}
