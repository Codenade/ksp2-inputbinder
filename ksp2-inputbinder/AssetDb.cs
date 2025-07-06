using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

namespace Codenade.Inputbinder
{
    public static class AssetDb
    {
        public static readonly AssetEntry<VisualTreeAsset> MainWindowUITK = new AssetEntry<VisualTreeAsset>("Codenade.Inputbinder/MainWindowUITK");
        public static readonly AssetEntry<VisualTreeAsset> ActionCategoryUITK = new AssetEntry<VisualTreeAsset>("Codenade.Inputbinder/ActionCategory.uxml");
        public static readonly AssetEntry<VisualTreeAsset> InputActionUITK = new AssetEntry<VisualTreeAsset>("Codenade.Inputbinder/InputAction.uxml");
        public static readonly AssetEntry<VisualTreeAsset> CompositeBindingUITK = new AssetEntry<VisualTreeAsset>("Codenade.Inputbinder/CompositeInputBinding.uxml");
        public static readonly AssetEntry<VisualTreeAsset> SingleBindingUITK = new AssetEntry<VisualTreeAsset>("Codenade.Inputbinder/SingleInputBinding.uxml");
        public static readonly AssetEntry<VisualTreeAsset> ProfileElementUITK = new AssetEntry<VisualTreeAsset>("Codenade.Inputbinder/ProfileElement.uxml");
        public static readonly AssetEntry<VisualTreeAsset> PreprocEntryUITK = new AssetEntry<VisualTreeAsset>("Codenade.Inputbinder/Preproc.uxml");
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
