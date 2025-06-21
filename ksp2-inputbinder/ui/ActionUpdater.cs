using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Codenade.Inputbinder.ui
{
    internal class ActionUpdater
    {
        internal List<IBindingUpdater> BindingUpdaters { get; private set; }
        private readonly WrappedInputAction a;
        private readonly Foldout ah;
        private readonly VisualElement rootVE;

        internal ActionUpdater(WrappedInputAction a, VisualElement rootVE)
        {
            BindingUpdaters = new List<IBindingUpdater>();
            ah = rootVE.Q<Foldout>();
            this.rootVE = rootVE;
            this.a = a;
            Clean();
        }

        internal void Update(InputAction a)
        {
            if (this.a.InputAction != a) return;
            foreach (var u in BindingUpdaters) u.Update();
        }

        internal void Mark(SearchOperation op)
        {
            Show();
            op.Done += Clean;
        }

        internal void Show() => rootVE.style.display = DisplayStyle.Flex;

        internal void Hide() => rootVE.style.display = DisplayStyle.None;

        internal void Clean()
        {
            ah.text = a.FriendlyName;
        }
    }
}
