using System;
using Newtonsoft.Json;

namespace Organilog.Models.Response
{
    public class IssueResponse
    {
        [JsonProperty("1")]
        public int ServerId { get; set; }

        [JsonProperty("2")]
        public int IssueUserId { get; set; }

        [JsonProperty("3")]
        public int IssueClientId { get; set; }

        [JsonProperty("4")]
        public int IssueAdresseId { get; set; }

        [JsonProperty("5")]
        public int IssueCategoryId { get; set; }

        [JsonProperty("6")]
        public int IssueOrigineId { get; set; }

        [JsonProperty("7")]
        public int IssueFilialeId { get; set; }

        [JsonProperty("8")]
        public int IssueCodeId { get; set; }

        [JsonProperty("9")]
        public string IssueNom { get; set; }

        [JsonProperty("10")]
        public string IssueContent { get; set; }

        [JsonProperty("11")]
        public int IssueStatus { get; set; }

        [JsonProperty("12")]
        public int IssuePriority { get; set; }

        [JsonProperty("13")]
        public string IssueEstimatedHours { get; set; }

        [JsonProperty("14")]
        public string IssueDemandeurCivilite { get; set; }

        [JsonProperty("15")]
        public string IssueDemandeurName { get; set; }

        [JsonProperty("16")]
        public string IssueDemandeurAdresse { get; set; }

        [JsonProperty("17")]
        public string IssueDemandeurPhone { get; set; }

        [JsonProperty("18")]
        public string IssueDemandeurEmail { get; set; }

        [JsonProperty("19")]
        public int IssueDoneRatio { get; set; }

        [JsonProperty("20")]
        public DateTime? IssueDateStart { get; set; }

        [JsonProperty("21")]
        public DateTime? IssueDateEnd { get; set; }

        [JsonProperty("22")]
        public string IssueHourStart { get; set; }

        [JsonProperty("23")]
        public string IssueHourEnd { get; set; }

        [JsonProperty("24")]
        public int IsActif { get; set; }

        [JsonProperty("25")]
        public DateTime? EditDate { get; set; }

        [JsonProperty("k")]
        public string K { get; set; }
    }
}
