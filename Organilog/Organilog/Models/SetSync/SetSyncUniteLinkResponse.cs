using Newtonsoft.Json;

namespace Organilog.Models.SetSync
{
    public class SetSyncUniteLinkResponse
    {
        [JsonProperty("appli_id")]
        public string AppId { get; set; }

        [JsonProperty("server_id")]
        public int ServerId { get; set; }

        [JsonProperty("code_id")]
        public int CodeId { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }
    }
}