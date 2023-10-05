using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class SaveButtonBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public bool Extended
        {
            get => _btnSec.gameObject.activeSelf;
            set
            {
                _btnSec.gameObject.SetActive(value);
                _arrowT.localRotation = Quaternion.Euler(0, 0, value ? 180 : 0);
            }
        }
        public bool interactable
        {
            get => _btnPri.interactable;
            set
            {
                _btnPri.interactable = value;
                _btnExt.interactable = value;
                _btnSec.interactable = value;
            }
        }

        public event Action PrimaryClick;
        public event Action SecondaryClick;

        private Button _btnPri;
        private Button _btnExt;
        private Button _btnSec;
        private ClickListener _clickListener;
        private RectTransform _arrowT;

        internal void Init()
        {
            _btnPri = transform.Find("Main/ActionPrimary").GetComponent<Button>();
            _btnExt = transform.Find("Main/ActionExtend").GetComponent<Button>();
            _btnSec = transform.Find("ActionSecondary").GetComponent<Button>();
            _arrowT = transform.Find("Main/ActionExtend/Arrow").GetComponent<RectTransform>();
            _btnPri.onClick.AddListener(BtnPriClicked);
            _btnExt.onClick.AddListener(BtnExtClicked);
            _btnSec.onClick.AddListener(BtnSecClicked);
        }

        private void BtnPriClicked()
        {
            Extended = false;
            PrimaryClick?.Invoke();
        }

        private void BtnExtClicked() => Extended = !Extended;

        private void BtnSecClicked()
        {
            Extended = false;
            SecondaryClick?.Invoke();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_clickListener != null) Destroy(_clickListener);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (Extended && _clickListener == null)
            {
                _clickListener = gameObject.AddComponent<ClickListener>();
                _clickListener.Click += () => Extended = false;
            }
        }
    }

    internal class ClickListener : MonoBehaviour
    {
        internal event Action Click;

        private void Update()
        {
            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
                Click?.Invoke();
        }
    }
}
