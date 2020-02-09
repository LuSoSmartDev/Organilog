using System;
using Newtonsoft.Json;
using Organilog.Models.Response;
using SQLite;

namespace Organilog.Models
{
    [Table("Category")]
    public class Category : BaseModel
    {
        [JsonProperty("grId")]
        [PrimaryKey]
        public Guid Id { get; set; }

        [JsonProperty("grServer")]
        public int ServerId { get; set; }

        [JsonProperty("grUserId")]
        public int UserId { get; set; }

        [JsonProperty("grClientServerId")]
        public int ClientId { get; set; }

        [JsonProperty("grAdresseServerId")]
        public int AdresseId { get; set; }

        [JsonProperty("grCode")]
        public int CodeId { get; set; }

        [JsonProperty("grTitle")]
        public string Title { get; set; }

        [JsonProperty("grIsActif")]
        public int IsActif { get; set; }

        [JsonProperty("grSynchronizationDate")]
        public DateTime? SynchronizationDate { get; set; }

        [JsonProperty("grAddDate")]
        public DateTime? AddDate { get; set; }

        [JsonProperty("grModifOn")]
        public DateTime? EditDate { get; set; }

        [JsonProperty("grLastViewDate")]
        public DateTime? LastViewDate { get; set; }

        public bool IsToSync { get; set; }

        public Category() { {} }
        
        public Category(CategoryResponse response)
        {
            if (!string.IsNullOrWhiteSpace(response.K) && Guid.TryParse(response.K, out Guid id))
                Id = id;
            else
                Id = Guid.NewGuid();

            ServerId = response.ServerId;
            CodeId = response.CodeId;
            
            Title = response.Title;
            ClientId = response.FkClientServerId;
            AdresseId = response.FkAddressServerId;
            AddDate = DateTime.Now;
            IsActif = response.IsActif;
            EditDate = response.ModifyDate ?? DateTime.Now;
        }

        public void UpdateFromResponse(CategoryResponse response)
        {
            ServerId = response.ServerId;
            CodeId = response.CodeId;
            ClientId = response.FkClientServerId;
            AdresseId = response.FkAddressServerId;
            IsActif = response.IsActif;
            Title = response.Title;
            EditDate = response.ModifyDate ?? DateTime.Now;
        }
   
    }
}
