using Newtonsoft.Json;
using System.Collections.Generic;

namespace Organilog.Models.Response
{
    public class InterventionHistoryRespone
    {
        [JsonProperty("interventions")]
        public List<InterventionResponse> Interventions { get; set; }
    }
}