using Newtonsoft.Json;
using Organilog.Models.Response;
using SQLite;
using System;
using System.Windows.Input;
using TinyMVVM;

namespace Organilog.Models
{
    [Table("UniteLink")]
    public class UniteLink : TinyModel
    {
        [JsonProperty("ulId")]
        [PrimaryKey]
        public Guid Id { get; set; }

        [JsonProperty("ulIdServer")]
        public int ServerId { get; set; }

        [JsonProperty("ulUserId")]
        public int UserId { get; set; }

        [JsonProperty("ulFkUnUUID")]
        public Guid FkUniteAppliId { get; set; }

        [JsonProperty("ulFkUnIdServer")]
        public int FkUniteServerId { get; set; }

        [JsonProperty("ulLinkTable")]
        public string LinkTableName { get; set; }

        [JsonProperty("ulFkColUUID")]
        public Guid FkColumnAppliId { get; set; }

        [JsonProperty("ulFkColIdServer")]
        public int FkColumnServerId { get; set; }

        [JsonProperty("ulValue")]
        public string UniteValue { get; set; }

        [Ignore]
        public string UniteValueUI { get; set; }


        [JsonProperty("ulOn")]
        public int IsActif { get; set; }

        //[JsonProperty("ulSynchronizationDate")]
        [Ignore]
        public DateTime? SynchronizationDate { get; set; }

        //[JsonProperty("ulAddDate")]
        [Ignore]
        public DateTime? AddDate { get; set; }


        //[JsonProperty("ulModifOn")]
        [Ignore]
        public DateTime? EditDate { get; set; }

        //[JsonProperty("ulLastViewDate")]
        [Ignore]
        public DateTime? LastViewDate { get; set; }

        [Ignore]
        public Unite Unite { get; set; }

        [Ignore]
        public string UniteTitle { get; set; }

        [Ignore]
        public string UniteDisplay { get; set; }

        [Ignore]
        public int UniteType { get; set; }

        public bool IsToSync { get; set; }

        public bool IsWebLink { get; set; }

        public bool IsNotWebLink { get; set; }

        public UniteLink()
        {
        }

        public UniteLink(UniteLinkResponse response)
        {
            if (!string.IsNullOrWhiteSpace(response.K) && Guid.TryParse(response.K, out Guid id))
                Id = id;
            else
                Id = Guid.NewGuid();
            AddDate = response.AddDate;
            ServerId = response.ServerId;
            FkUniteServerId = response.FkUniteServerId;
            LinkTableName = response.LinkTableName;
            FkColumnServerId = response.FkColumnServerId;
            UniteValue = response.UniteValue;
            IsActif = response.IsActif;
            EditDate = response.EditDate;
        }

        public void UpdateFromResponse(UniteLinkResponse response)
        {
            FkUniteServerId = response.FkUniteServerId;
            LinkTableName = response.LinkTableName;
            FkColumnServerId = response.FkColumnServerId;
            UniteValue = response.UniteValue;
            IsActif = response.IsActif;
            EditDate = response.EditDate;
        }
    }
}