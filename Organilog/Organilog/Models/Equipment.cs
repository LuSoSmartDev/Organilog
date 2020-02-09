using Newtonsoft.Json;
using Organilog.Models.Response;
using SQLite;
using System;
using System.Collections.ObjectModel;
using TinyMVVM;

namespace Organilog.Models
{
    [Table("Equipment")]
    public class Equipment : BaseModel
    {
        [JsonProperty("equId")]
        [PrimaryKey]
        public Guid Id { get; set; }

        [JsonProperty("equIdServer")]
        public int ServerId { get; set; }

        [JsonProperty("equFkClientAppliId")]
        public Guid ClientAppliId { get; set; }

        [JsonProperty("equFkAdresseAppliId")]
        public Guid AdresseAppliId { get; set; }

        [JsonProperty("equFkClientId")]
        public int ClientServerId { get; set; }

        [JsonProperty("equFkAdresseId")]
        public int AdresseServerId { get; set; }

        [JsonProperty("equTitle")]
        public string Title { get; set; }

        [JsonProperty("equCode")]
        public string CodeId { get; set; }

        [JsonProperty("equDateBuy")]
        public DateTime? DateBuy { get; set; }

        [JsonProperty("equDateInstall")]
        public DateTime? DateInstall { get; set; }

        [JsonProperty("equDateGuaranteeStart")]
        public DateTime? DateGuaranteeStart { get; set; }

        [JsonProperty("equDateGuaranteeEnd")]
        public DateTime? DateGuaranteeEnd { get; set; }

        [JsonProperty("equComment")]
        public string Comment { get; set; }

        [JsonProperty("equOn")]
        public int IsActif { get; set; }

        [JsonProperty("equNonce")]
        public string Nonce { get; set; }

        
       [JsonProperty("equCategory")]
        public string Category { get; set; }

        [JsonIgnore]
        public DateTime? EditDate { get; set; }

        [JsonIgnore]
        public int UserId { get; set; }

        [JsonIgnore]
        public int AccountId { get; set; }


        [Ignore]
        public Client Client { get; set; }

        [Ignore]
        public Address Address { get; set; }

        [Ignore]
        public ObservableCollection<MediaLink> MediaLinks { get; set; }

        [JsonIgnore]
        public bool IsToSync { get; set; }

        [JsonIgnore]
        public string PropertyIgnore
        {
            get
            {
                return string.Join(",", nameof(IsToSync), nameof(Client), nameof(Address), nameof(AccountId));
            }
        }

        public Equipment(EquipmentResponse response)
        {
            if (!string.IsNullOrWhiteSpace(response.K) && Guid.TryParse(response.K, out Guid id))
                Id = id;
            else
                Id = Guid.NewGuid();

            ServerId = response.ServerId;
            CodeId = response.CodeId;
            Title = response.Title;
            ClientServerId = response.ClientServerId;
            AdresseServerId = response.AddressServerId;
            DateBuy = response.DateBuy;
            DateInstall = response.DateInstall;
            DateGuaranteeEnd = response.DateGuaranteeEnd;
            DateGuaranteeStart = response.DateGuaranteeStart;
            IsActif = response.IsActif;
            EditDate = response.EditDate ?? DateTime.Now;
            IsToSync = false;
        }

        public void UpdateFromResponse(EquipmentResponse response)
        {
            ServerId = response.ServerId;
            CodeId = response.CodeId;
            Title = response.Title;
            ClientServerId = response.ClientServerId;
            AdresseServerId = response.AddressServerId;
            DateBuy = response.DateBuy;
            DateInstall = response.DateInstall;
            DateGuaranteeEnd = response.DateGuaranteeEnd;
            DateGuaranteeStart = response.DateGuaranteeStart;
            IsActif = response.IsActif;
            EditDate = response.EditDate ?? DateTime.Now;
        }
        public Equipment() { }

    }
}
