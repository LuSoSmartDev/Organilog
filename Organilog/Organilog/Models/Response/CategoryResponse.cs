using System;
using Newtonsoft.Json;

namespace Organilog.Models.Response
{
    public class CategoryResponse
    {
        [JsonProperty("1")]
        public int ServerId { get; set; }

        [JsonProperty("2")]
        public int CodeId { get; set; }

        [JsonProperty("3")]
        public int FkClientServerId { get; set; }

        [JsonProperty("4")]
        public int FkAddressServerId { get; set; }

       
        [JsonProperty("5")]
        public string Title { get; set; }

       
        [JsonProperty("6")]
        public int IsActif { get; set; }

        [JsonProperty("7")]
        public DateTime? ModifyDate { get; set; }

        [JsonProperty("k")]
        public string K { get; set; }
    }
}
