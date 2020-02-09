using System;
using Newtonsoft.Json;

namespace Organilog.Models.Response
{
    public class IssueReplyResponse
    {
        [JsonProperty("1")]
        public int ServerId { get; set; }

        [JsonProperty("2")]
        public int FKIssueId { get; set; }

        [JsonProperty("3")]
        public string Message { get; set; }

        [JsonProperty("4")]
        public string CommentPrivate { get; set; }

        [JsonProperty("5")]
        public string Type { get; set; }

        [JsonProperty("6")]
        public int IsActif { get; set; }

        [JsonProperty("7")]
        public DateTime? EditDate { get; set; }

        [JsonProperty("8")]
        public DateTime? AddDate { get; set; }

        [JsonProperty("9")]
        public int AuthorType { get; set; }

        [JsonProperty("10")]
        public int AddedByFkUserId { get; set; }


        [JsonProperty("k")]
        public string K { get; set; }
    }
}
