using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Organilog.Models.SetSync
{
   
  public class SetIssueResponse
        {
        [JsonProperty("SUCCESS")]
        public string Success { get; set; }

        [JsonProperty("issues")]
        public List<IssueResponse> Issues { get; set; } = new List<IssueResponse>();

        [JsonProperty("issues_lines")]
        public List<IssueLineResponse> IssuesLines { get; set; } = new List<IssueLineResponse>();
    }

    public class IssueResponse
    {
        [JsonProperty("appli_id")]
        public string AppId { get; set; }

        [JsonProperty("server_id")]
        public int ServerId { get; set; }

        [JsonProperty("code_id")]
        public int CodeId { get; set; }
    }

    public class IssueLineResponse
    {
        [JsonProperty("appli_id")]
        public string AppId { get; set; }

        [JsonProperty("server_id")]
        public int ServerId { get; set; }
    }

}
