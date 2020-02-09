using Newtonsoft.Json;

namespace Organilog.Models.SetSync
{
    public class SetSyncMessageResponse
    {
        [JsonProperty("appli_id")]
        public string AppId { get; set; }

        [JsonProperty("server_id")]
        public int ServerId { get; set; }
    }
}