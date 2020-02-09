using Newtonsoft.Json;
using System;

namespace Organilog.Models.Response
{
   public class EquipmentResponse
    {
        [JsonProperty("0")]
        public DateTime? AddDate { get; set; }

        [JsonProperty("1")]
        public int ServerId { get; set; }

        [JsonProperty("2")]
        public int ClientServerId { get; set; }

        [JsonProperty("3")]
        public int AddressServerId { get; set; }

        [JsonProperty("4")]
        public string CodeId { get; set; }

        [JsonProperty("5")]
        public string Title { get; set; }

        [JsonProperty("6")]
        public int CodeEQU { get; set; }

        [JsonProperty("7")]
        public DateTime? DateBuy { get; set; }

        [JsonProperty("8")]
        public DateTime? DateInstall { get; set; }

        [JsonProperty("9")]
        public DateTime? DateGuaranteeStart { get; set; }

        [JsonProperty("10")]
        public DateTime? DateGuaranteeEnd { get; set; }

        [JsonProperty("11")]
        public string Comment { get; set; }

        [JsonProperty("12")]
        public int IsActif { get; set; }

        [JsonProperty("13")]
        public DateTime? EditDate { get; set; }

        [JsonProperty("k")]
        public string K { get; set; }
    }
}

/*
 * 
 *   k = UUID
   1 = server ID
   2 = foreign key client ID
   3 = foreign key adresse ID
   4 = codeID
   5 = title
   6 = code of equipment
   7 = date buy (YYYY-MM-DD)
   8 = date install (YYYY-MM-DD)
   9 = date guarantee start
   10 = date guarantee end
   11 = comment
   12 = (int) is actif
   13 = last_modif_date
   14 = nonce

 */
