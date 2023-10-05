using Newtonsoft.Json;

namespace Codenade.Inputbinder
{
    internal struct LegacyInputActionData
    {
        [JsonProperty("friendly_name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string FriendlyName { get; set; }
        [JsonProperty("action_type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ActionType { get; set; }
        [JsonProperty("is_from_game", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsFromGame { get; set; }
        [JsonProperty("bindings", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public LegacyBindingData[] Bindings { get; set; }
    }

    internal struct LegacyBindingData
    {
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty("is_composite", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsComposite { get; set; }
        [JsonProperty("is_part_of_composite", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool IsPartOfComposite { get; set; }
        [JsonProperty("path", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Path { get; set; }
        [JsonProperty("processors", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Processors { get; set; }
        [JsonProperty("override", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Override { get; set; }
        [JsonProperty("path_override", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string PathOverride { get; set; }
        [JsonProperty("processors_override", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string ProcessorsOverride { get; set; }
    }
}
