using Newtonsoft.Json;
using Organilog.Models.Response;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TinyMVVM;

namespace Organilog.Models
{
    [Table("Client")]
    public class Client : BaseModel
    {
        [JsonProperty("cId")]
        [PrimaryKey]
        public Guid Id { get; set; }

        [JsonProperty("cIdServer")]
        public int ServerId { get; set; }

        [JsonProperty("cUserId")]
        public int UserId { get; set; }

        [JsonProperty("cCode")]
        public int Code { get; set; }

        [JsonProperty("cFkAdresseMainAppliId")]
        public Guid FkMainAdressesAppliId { get; set; }

        [JsonProperty("cFkAdresseMainId")]
        public int FkMainAdressesServerId { get; set; }

        private string titleClient;
        [JsonProperty("cTitle")]
        public string Title { get => titleClient; set { titleClient = value; OnPropertyChanged(nameof(RequiredClientName), nameof(EnableSaveButton),nameof(Addresses)); } }

        [JsonProperty("cCivilite")]
        public int Civilite { get; set; }

        [JsonProperty("cPrenom")]
        public string Prenom { get; set; }

        [JsonProperty("cNom")]
        public string Nom { get; set; }

        [JsonProperty("cSociete")]
        public string Societe { get; set; }

        [JsonProperty("cEmail")]
        public string Email { get; set; }

        [JsonProperty("cPhoneFixe")]
        public string PhoneFixe { get; set; }

        [JsonProperty("cPhoneMobile")]
        public string PhoneMobile { get; set; }

        [JsonProperty("cPhonePro")]
        public string PhonePro { get; set; }

        [JsonProperty("cFax")]
        public string Fax { get; set; }

        [JsonProperty("cLang")]
        public string Lang { get; set; }

        [JsonProperty("cComment")]
        public string Comment { get; set; }

        [JsonProperty("cOn")]
        public int IsActif { get; set; }

        [JsonProperty("cSynchronizationDate")]
        public DateTime? SynchronizationDate { get; set; }

        [JsonProperty("cAddDate")]
        public DateTime? AddDate { get; set; }

        [JsonProperty("cModifOn")]
        public DateTime? EditDate { get; set; }

        [JsonProperty("cLastViewDate")]
        public DateTime? LastViewDate { get; set; }

        [JsonProperty("cPhoneOther")]
        public string PhoneDrivers { get; set; }

        [Ignore]
        public string FullName => (Prenom + " " + Nom)?.Trim();

        private List<Address> addresses;
        [Ignore]
        public List<Address> Addresses { get => addresses; set => SetProperty(ref addresses, value, onChanged: () => { AddressesCount = addresses.Count; OnPropertyChanged(nameof(AddressesCount),nameof(EnableSaveButton)); }); }

        //TODO checking 
        private ObservableCollection<Address> addressesclone;
        [Ignore]
        [JsonIgnore]
        public ObservableCollection<Address> Addressesclone { get => addressesclone; set => SetProperty(ref addressesclone, value); }


        [Ignore]
        public int AddressesCount { get; set; }

        private bool requiredField;
        [Ignore]
        public bool RequiredClientName {
            get {
               // EnableSaveButton = !string.IsNullOrEmpty(Title);
                return string.IsNullOrEmpty(Title);
            }
            set {
                requiredField = value;
              
                //OnPropertyChanged(nameof(EnableSaveButton));
            }
        }
        
        [Ignore]
        public bool EnableSaveButton
        {
            get
            {
                return !string.IsNullOrEmpty(Title);
            }
        }

        [Ignore]
        public string AddressesCountTitle { get {
                if (AddressesCount == 0)
                    return "";
                if (AddressesCount > 1)
                {
                    return "(" +  AddressesCount + " " + "addresses" +")";
                }
                return "("+ AddressesCount + " " + "addresse)";

            }
        }

        public bool IsToSync { get; set; }

        public Client()
        {
        }

        public Client(ClientResponse response)
        {
            if (!string.IsNullOrWhiteSpace(response.K) && Guid.TryParse(response.K, out Guid id))
                Id = id;
            else
                Id = Guid.NewGuid();
            AddDate = response.AddDate;
            ServerId = response.ServerId;
            Code = response.Code;
            FkMainAdressesServerId = response.FkMainAdressesServerId;
            Title = response.Title;
            Civilite = response.Civilite;
            Prenom = response.Prenom;
            Nom = response.Nom;
            Societe = response.Societe;
            Email = response.Email;
            PhoneFixe = response.PhoneFixe;
            PhoneMobile = response.PhoneMobile;
            PhonePro = response.PhonePro;
            Fax = response.Fax;
            Lang = response.Lang;
            Comment = response.Comment;
            PhoneDrivers = response.OtherPhone;
            IsActif = response.IsActif;
        }

        public void UpdateFromResponse(ClientResponse response)
        {
            Code = response.Code;
            FkMainAdressesServerId = response.FkMainAdressesServerId;
            Title = response.Title;
            Civilite = response.Civilite;
            Prenom = response.Prenom;
            Nom = response.Nom;
            Societe = response.Societe;
            Email = response.Email;
            PhoneFixe = response.PhoneFixe;
            PhoneMobile = response.PhoneMobile;
            PhonePro = response.PhonePro;
            Fax = response.Fax;
            Lang = response.Lang;
            Comment = response.Comment;
            PhoneDrivers = response.OtherPhone;
            IsActif = response.IsActif;
        }
    }
}