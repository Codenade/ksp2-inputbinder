using UnityEngine.UIElements;

namespace Codenade.Inputbinder.ui
{
    internal interface IBindingUpdater
    {
        void Update();
        void Mark(SearchOperation op);
    }

    internal class CompositeUpdater : IBindingUpdater
    {
        private readonly WrappedInputAction a;
        private readonly int biIndex;
        private readonly Label coBiName;
        private readonly Label coBiPreproc;

        internal CompositeUpdater(WrappedInputAction a, int biIndex, VisualElement rootVE)
        {
            this.a = a;
            this.biIndex = biIndex;

            coBiName = rootVE.Q<Label>("CoBiName");
            coBiPreproc = rootVE.Q<Label>("PreprocValue");
            rootVE.Q<Button>("PreprocBtnEdit").clicked += () => Inputbinder.Instance.BindingUI.ProcessorEditMenu.ShowFor(a, biIndex);

            Update();
        }

        public void Mark(SearchOperation op)
        {
            coBiName.text = Utils.MarkText(a.InputAction.bindings[biIndex].effectivePath, op.Query);
            coBiPreproc.text = Utils.MarkText(a.InputAction.bindings[biIndex].effectiveProcessors, op.Query);
            op.Done += Update;
        }

        public void Update()
        {
            var ib = a.InputAction.bindings[biIndex];
            coBiName.text = ib.effectivePath;
            coBiPreproc.text = ib.effectiveProcessors;
        }
    }

    internal class BindingUpdater : IBindingUpdater
    {
        private readonly WrappedInputAction a;
        private readonly int biIndex;
        private readonly Label biName;
        private readonly Label biPath;
        private readonly Label biPreprocLbl;

        internal BindingUpdater(WrappedInputAction a, int biIndex, VisualElement rootVE)
        {
            this.a = a;
            this.biIndex = biIndex;

            var ib = a.InputAction.bindings[biIndex];

            biName = rootVE.Q<Label>("BindingName");
            rootVE.Q<Button>("RebindButton").clicked += () => Inputbinder.Instance.ActionManager.Rebind(a.InputAction, biIndex);

            biPath = rootVE.Q<Label>("BindingPath");
            rootVE.Q<Button>("ClearButton").clicked += () => InputActionManager.ClearBinding(ib, a.InputAction);

            var procSec = rootVE.Q<VisualElement>("Processors");
            biPreprocLbl = procSec.Q<Label>("BindingProcessors");
            procSec.Q<Button>("EditProcessorsButton").clicked += () => Inputbinder.Instance.BindingUI.ProcessorEditMenu.ShowFor(a, biIndex);

            if (!ib.isPartOfComposite)
            {
                procSec.style.display = DisplayStyle.Flex;
            }
            else procSec.style.display = DisplayStyle.None;

            Update();
        }

        public void Mark(SearchOperation op)
        {
            var ib = a.InputAction.bindings[biIndex];
            biName.text = Utils.MarkText(ib.name, op.Query);
            biPath.text = Utils.MarkText(ib.effectivePath, op.Query);
            if (!ib.isPartOfComposite) biPreprocLbl.text = Utils.MarkText(ib.effectiveProcessors, op.Query);
            op.Done += Update;
        }

        public void Update()
        {
            var ib = a.InputAction.bindings[biIndex];
            biName.text = ib.name;
            biPath.text = ib.effectivePath;
            biPreprocLbl.text = ib.effectivePath;
        }
    }
}
