using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Organilog.Models.SetSync
{
   
    public class SetSyncEquipment
    {
        [JsonProperty("SUCCESS")]
        public string Success { get; set; }

        [JsonProperty("equipements")]
        public List<EquipmentResponse> Equipements { get; set; } = new List<EquipmentResponse>();

        [JsonProperty("equipements_links")]
        public List<LinkInterventionEquipmentLineResponse> EquipmentsLines { get; set; } = new List<LinkInterventionEquipmentLineResponse>();
    }
    public class EquipmentResponse
    {
        [JsonProperty("appli_id")]
        public string AppId { get; set; }

        [JsonProperty("server_id")]
        public int ServerId { get; set; }

        [JsonProperty("code_id")]
        public string CodeId { get; set; }
    }

}
