using Newtonsoft.Json;

namespace Organilog.Models.Response
{
    public class SyncResponse
    {
        [JsonProperty("SUCCESS")]
        public string Success { get; set; }
    }
}