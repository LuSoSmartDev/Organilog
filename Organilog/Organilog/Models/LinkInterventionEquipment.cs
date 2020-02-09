using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Organilog.Models
{
    [Table("LinkInterventionEquipment")]
    public class LinkInterventionEquipment : BaseModel
    {
        [JsonProperty("lieId")]
        [PrimaryKey]
        public Guid Id { get; set; }

        [JsonProperty("lieIdServer")]
        public int ServerId { get; set; }

        [JsonProperty("lipUserId")]
        public int UserId { get; set; }

        [JsonProperty("lieFkEquipementAppliId")]
        public Guid FkEquipmentAppliId { get; set; }

        [JsonProperty("lieFkEquipementId")]
        public int FkEquipmentServerId { get; set; }

        [JsonProperty("lieFkInterventionAppliId")]
        public Guid FkInterventionAppliId { get; set; }

        [JsonProperty("lieFkInterventionId")]
        public int FkInterventionServerId { get; set; }

        private int isActif;

        [JsonProperty("lieOn")]
        public int IsActif { get => isActif; set => SetProperty(ref isActif, value); }

       
        [Ignore]
        public Equipment Equipment { get; set; }

        [JsonProperty("lieAddDate")]
        public DateTime? AddDate { get; set; }

        [JsonProperty("lieEditDate")]
        public DateTime? EditDate { get; set; }

        [JsonProperty("lieNom")]
        public string EquipmentName { get; set; }

        [JsonIgnore]
        public bool IsToSync { get; set; }

        [JsonIgnore]
        public string PropertyIgnore
        {
            get
            {
                return string.Join(",", nameof(IsToSync), nameof(EquipmentName), nameof(UserId));
            }
        }

        public LinkInterventionEquipment() {
               
        }

    }
}
