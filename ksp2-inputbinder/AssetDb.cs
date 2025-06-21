using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Codenade.Inputbinder
{
    public static class AssetDb
    {
        public static readonly AssetEntry<GameObject> WindowProcessorsContent = new AssetEntry<GameObject>("Codenade.Inputbinder/ProcessorWindowContent");
        public static readonly AssetEntry<GameObject> ProcessorGroup = new AssetEntry<GameObject>("Codenade.Inputbinder/ProcessorGroup");
        public static readonly AssetEntry<GameObject> ProcessorAddGroup = new AssetEntry<GameObject>("Codenade.Inputbinder/ProcessorAddGroup");
        public static readonly AssetEntry<GameObject> ProcessorValueGroup = new AssetEntry<GameObject>("Codenade.Inputbinder/ProcessorValueGroup");
        public static readonly AssetEntry<GameObject> ProcessorValueGroupBool = new AssetEntry<GameObject>("Codenade.Inputbinder/ProcessorValueGroupBool");
        public static readonly AssetEntry<GameObject> ProcessorSaveButton = new AssetEntry<GameObject>("Codenade.Inputbinder/ProcessorSaveButton");
        public static readonly AssetEntry<GameObject> ConfirmResetAllBindingsOverlay = new AssetEntry<GameObject>("Codenade.Inputbinder/ConfirmResetAllDialogOverlay");
        public static readonly AssetEntry<GameObject> SaveAsDialogOverlay = new AssetEntry<GameObject>("Codenade.Inputbinder/SaveAsDialogOverlay");
        public static readonly AssetEntry<GameObject> ProfileElement = new AssetEntry<GameObject>("Codenade.Inputbinder/ProfileElement");
        public static readonly AssetEntry<GameObject> LoadDialogOverlay = new AssetEntry<GameObject>("Codenade.Inputbinder/LoadDialogOverlay");
        public static readonly AssetEntry<VisualTreeAsset> MainWindowUITK = new AssetEntry<VisualTreeAsset>("Codenade.Inputbinder/MainWindowUITK");
        public static readonly AssetEntry<VisualTreeAsset> ActionCategoryUITK = new AssetEntry<VisualTreeAsset>("Codenade.Inputbinder/ActionCategory.uxml");
        public static readonly AssetEntry<VisualTreeAsset> InputActionUITK = new AssetEntry<VisualTreeAsset>("Codenade.Inputbinder/InputAction.uxml");
        public static readonly AssetEntry<VisualTreeAsset> CompositeBindingUITK = new AssetEntry<VisualTreeAsset>("Codenade.Inputbinder/CompositeInputBinding.uxml");
        public static readonly AssetEntry<VisualTreeAsset> SingleBindingUITK = new AssetEntry<VisualTreeAsset>("Codenade.Inputbinder/SingleInputBinding.uxml");
    }

    public interface IAssetEntry
    {
        public string Key { get; }
        public object EntryAsObject { get; set; }
        public System.Type Type { get; }
    }

    public class AssetEntry<T> : IAssetEntry where T : Object
    {
        public string Key { get; private set; }
        public T Entry { get; set; }
        public object EntryAsObject { get => Entry; set => Entry = (T)value; }
        public System.Type Type => typeof(T);
        public static implicit operator T(AssetEntry<T> a) => a.Entry;

        public AssetEntry(string key) => Key = key;
    }
}
