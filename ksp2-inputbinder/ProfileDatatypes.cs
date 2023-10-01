using Newtonsoft.Json;
using System.Collections.Generic;

namespace Codenade.Inputbinder
{
    public struct InputProfileData
    {
        [JsonProperty("fileVersion")]
        public int FileVersion { get; set; }
        [JsonProperty("gameVersion")]
        public string GameVersion { get; set; }
        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }
        [JsonProperty("actions")]
        public Dictionary<string, InputActionData> Actions { get; set; }
    }

    public struct InputActionData
    {
        [JsonProperty("bindings", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public BindingData[] Bindings { get; set; }
    }

    public struct BindingData
    {
        [JsonProperty("overridePath", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string OverridePath { get; set; }
        [JsonProperty("overrideProcessors", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string OverrideProcessors { get; set; }
    }
}
