using Newtonsoft.Json;
using System.Collections.Generic;

namespace Codenade.Inputbinder
{
    /// <summary>Root datatype containing basic information on input profiles</summary>
    public struct InputProfileData
    {
        /// <summary>Inputbinder profile version</summary>
        [JsonProperty("fileVersion")]
        public int FileVersion { get; set; }
        /// <summary>Gameversion this profile was saved with</summary>
        [JsonProperty("gameVersion")]
        public string GameVersion { get; set; }
        /// <summary>Unix timestamp of save time (UTC time)</summary>
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
