using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class SaveButtonBehaviour : MonoBehaviour
    {
        public bool Extended
        {
            get => _btnSec.gameObject.activeSelf;
            set
            {
                _btnSec.gameObject.SetActive(value);
                _arrowT.localRotation = Quaternion.Euler(0, 0, value ? 180 : 0);
                if (!value) _clickAction?.Dispose();
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
        private InputAction _clickAction;
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

        private void BtnExtClicked()
        {
            Extended = !Extended;
            if (Extended)
            {
                if (_clickAction is object) return;
                _clickAction = new InputAction
                {
                    expectedControlType = "Button"
                };
                _clickAction.AddBinding(UnityEngine.InputSystem.Mouse.current.leftButton);
                _clickAction.AddBinding(UnityEngine.InputSystem.Mouse.current.rightButton);
                _clickAction.canceled += (_) => Extended = false;
                //_clickAction.Enable();
            }
            else
                _clickAction?.Dispose();
        }

        private void BtnSecClicked()
        {
            Extended = false;
            SecondaryClick?.Invoke();
        }
    }
}
