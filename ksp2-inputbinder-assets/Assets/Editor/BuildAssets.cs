public static class BuildAssets
{
    public static void PerformBuild()
    {
        UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.CleanPlayerContent();
        UnityEditor.AddressableAssets.Settings.AddressableAssetSettings.BuildPlayerContent();
    }
}
