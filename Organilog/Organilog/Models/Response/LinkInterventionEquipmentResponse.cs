using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Organilog.Models.Response
{
    class LinkInterventionEquipmentResponse
    {
        [JsonProperty("0")]
        public DateTime? AddDate { get; set; }

        [JsonProperty("1")]
        public int ServerId { get; set; }

        [JsonProperty("2")]
        public int FkEquipementServerId { get; set; }


        [JsonProperty("3")]
        public int FkInterventionServerId { get; set; }

        [JsonProperty("4")]
        public int IsActif { get; set; }

        [JsonProperty("5")]
        public DateTime? EditDate { get; set; }

        [JsonProperty("6")]
        public string Nonce { get; set; }

        [JsonProperty("k")]
        public string K { get; set; }
    }
}
