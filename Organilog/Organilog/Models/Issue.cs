using System;
using TinyMVVM;
using SQLite;
using Newtonsoft.Json;
using Organilog.Models.Response;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using Xamarin.Forms;

namespace Organilog.Models
{
    [Table("Issue")]
    public class Issue : BaseModel
    {
        [JsonProperty("issId")]
        [PrimaryKey]
        public Guid Id { get; set; }

        [JsonProperty("issIdServer")]
        public int ServerId { get; set; }

        [JsonIgnore]
        public int UserId { get; set; }

        [JsonIgnore]
        public int AccountId { get; set; }

        [JsonProperty("issFkUserAppliId")]
        public Guid FkUserAppId { get; set; }

        [JsonProperty("issFkUserId")]
        public int FkUserServerlId { get; set; }

        [JsonProperty("issFkClientAppliId")]
        public Guid FkClientAppId { get; set; }

        [JsonProperty("issFkClientId")]
        public int FkClientServerId { get; set; }

        [JsonProperty("issFkAdresseId")]
        public int FkAddressServerId { get; set; }

        [JsonProperty("issFkAdresseAppliId")]
        public Guid FkAddressAppliId { get; set; }


        [JsonProperty("issFkCategoryServerId")]
        public int FkCategoryServerId { get; set; }

        [JsonProperty("issFkOrigineServerId")]
        public int FkOrigineServerId { get; set; }


        [JsonProperty("issFkFilialeApplId")]
        public Guid FilialeId { get; set; }

        [JsonProperty("issFkFilialeId")]
        public int FkFilialeServerId { get; set; }


        [JsonProperty("issCodeId")]
        public int CodeId { get; set; }

        [JsonProperty("issTitle")]
        public string Nom { get; set; }

        [JsonProperty("issContent")]
        public string Content { get; set; }

        [JsonProperty("issStatus")]
        public int Status { get; set; }

        [JsonProperty("issPriority")]
        public int Priority { get; set; }

        [JsonProperty("issEstimatedHours")]
        public string EstimatedHours { get; set; }

        //todo
        [JsonProperty("issDemandeurCivilite")]
        public string DemandeurCivilite { get; set; }

        [JsonProperty("issDemandeurName")]
        public string DemandeurName { get; set; }

        [JsonProperty("issDemandeurAdresse")]
        public string DemandeurAdresse { get; set; }

        [JsonProperty("issDemandeurPhone")]
        public string DemandeurPhone { get; set; }

        [JsonProperty("issDemandeurEmail")]
        public string DemandeurEmail { get; set; }

        [JsonProperty("issDoneRatio")]
        public int DoneRatio { get; set; }

    
        private DateTime? dateStart;
        [JsonProperty("issDateStart")]
        public DateTime? DateStart { get => dateStart; set => SetProperty(ref dateStart, value, onChanged: () => OnPropertyChanged(nameof(DateStart), nameof(Display))); }


        private DateTime? dateEnd;
        [JsonProperty("issDateEnd")]
        public DateTime? DateEnd { get => dateEnd; set => SetProperty(ref dateEnd, value, onChanged: () => OnPropertyChanged(nameof(DateEnd), nameof(Display))); }


        [JsonProperty("issHourStart")]
        public string HourStart { get; set; }

        [JsonProperty("issHourEnd")]
        public string HourEnd { get; set; }

        //End Todo
        [JsonProperty("issOn")]
        public int IsActif { get; set; }

        [JsonProperty("issAddDate")]
        public DateTime? AddDate { get; set; }

        [JsonProperty("issModifOn")]
        public DateTime? EditDate { get; set; }


        private int sendMail;
        [JsonProperty("issSendToClient")]
        public int SendMail { get => sendMail; set => SetProperty(ref sendMail, value); }


        private Client client;

        [Ignore]
        [JsonIgnore]
        public string LegalTitle
        {
            get
            {
                if (CodeId > 0 && AppSettings.MobileShowNumberTitle)
                {
                    return "N° " + CodeId + " " +Nom;

                }
                return Nom;
            }
        }

        private int selectedIndex;
        [JsonIgnore]
        public int SelectedIndex { get => selectedIndex; set => SetProperty(ref selectedIndex, value); }

        [Ignore]
        [JsonIgnore]
        public Client Client { get => client; set => SetProperty(ref client, value); }

        private Address address;
        [Ignore]
        [JsonIgnore]
        public Address Address { get => address; set => SetProperty(ref address, value); }

        //Todo
        [Ignore]
        [JsonIgnore]
        public ObservableCollection<MediaLink> MediaLinks { get; set; }

        [Ignore]
        [JsonIgnore]
        public List<UniteLink> UniteLinks { get; set; }


        [JsonIgnore]
        public string FilialeName { get; set; }

        [JsonIgnore]
        public string CategoryName { get; set; }

        private ObservableCollection<IssueLink> mes;

        [Ignore]
        [JsonIgnore]
        public ObservableCollection<IssueLink> Messages { 
                get { return mes; }
                set => SetProperty(ref mes, value, onChanged:()=> OnPropertyChanged(nameof(Messages)));
                 
          } //list of message of reply 
        [JsonIgnore]
        public bool IsToSync { get; set; }
        [Ignore]
        [JsonIgnore]
        public string Times {
                get {
                    return string.Format("{0} - {1}",HourStart==null ? "00": HourStart,HourEnd==null? "00":HourEnd);
                 } 
        }

        [Ignore]
        [JsonIgnore]
        public string DateAndTime
        {
            get
            {
                return string.Format("{0} {1}", String.Format("{0:dd/MM/yyyy}", DateStart), Times);
            }
        }

        [Ignore]
        [JsonIgnore]
        public string Dates
        {
            get
            {
                if(DateStart!=null)
                    return String.Format("{0:m}", DateStart);

                return String.Format("{0:m}", EditDate);
            }
        }
        [Ignore]
        [JsonIgnore]
        public string Display
        {
            get
            {
                return string.Format("{0} - {1}", String.Format("{0:dd/MM/yyyy}", DateStart), String.Format("{0:dd/MM/yyyy}", DateEnd));
            }
        }
        [Ignore]
        [JsonIgnore]
        public string PropertyIgnore
        {
            get
            {
                return string.Join(",", nameof(IsToSync));
            }
        }
        [Ignore]
        [JsonIgnore]
        public Color PropColor
        {
            get {
                if(Status == 1)
                    return Color.Transparent;

                else if(Status==2)
                    return Color.FromHex("#f7ecb7");

                else if (Status == 3)
                    return Color.FromHex("#f7ecb7");

                else if (Status == 4)
                    return Color.FromHex("#dff0d8");

                else if (Status == 5)
                    return Color.FromHex("#f2dede");

                else if (Status == 6)
                    return Color.FromHex("#f2dede");

                return Color.Transparent;

            }
        }
        public Issue(IssueResponse response)
        {
            if (!string.IsNullOrWhiteSpace(response.K) && Guid.TryParse(response.K, out Guid id))
                Id = id;
            else
                Id = Guid.NewGuid();
            AddDate = DateTime.Now;
            ServerId = response.ServerId;
            CodeId = response.IssueCodeId;
            FkUserServerlId = response.IssueUserId;
            FkClientServerId = response.IssueClientId;
            FkAddressServerId = response.IssueAdresseId;
            Nom = response.IssueNom;
            Content = response.IssueContent;
            Status = response.IssueStatus;
            Priority = response.IssuePriority;
            EstimatedHours = response.IssueEstimatedHours;
            DemandeurCivilite = response.IssueDemandeurCivilite;
            DemandeurName = response.IssueDemandeurName;
            DemandeurAdresse = response.IssueDemandeurAdresse;
            DemandeurPhone = response.IssueDemandeurPhone;
            DemandeurEmail = response.IssueDemandeurEmail;
            DoneRatio = response.IssueDoneRatio;
            DateStart = response.IssueDateStart;
            DateEnd = response.IssueDateEnd;
            HourStart = response.IssueHourStart;
            HourEnd = response.IssueHourEnd;
            IsActif = response.IsActif;
            EditDate = response.EditDate;

            FkFilialeServerId = response.IssueFilialeId;
           
        }

        public void UpdateFromResponse(IssueResponse response)
        {
            AddDate = DateTime.Now;
            ServerId = response.ServerId;
            CodeId = response.IssueCodeId;
            FkUserServerlId = response.IssueUserId;
            FkClientServerId = response.IssueClientId;
            FkAddressServerId = response.IssueAdresseId;
            Nom = response.IssueNom;
            Content = response.IssueContent;
            Status = response.IssueStatus;
            Priority = response.IssuePriority;
            EstimatedHours = response.IssueEstimatedHours;
            DemandeurCivilite = response.IssueDemandeurCivilite;
            DemandeurName = response.IssueDemandeurName;
            DemandeurAdresse = response.IssueDemandeurAdresse;
            DemandeurPhone = response.IssueDemandeurPhone;
            DemandeurEmail = response.IssueDemandeurEmail;
            DoneRatio = response.IssueDoneRatio;
            DateStart = response.IssueDateStart;
            DateEnd = response.IssueDateEnd;
            HourStart = response.IssueHourStart;
            HourEnd = response.IssueHourEnd;
            IsActif = response.IsActif;
            EditDate = response.EditDate;
            FkFilialeServerId = response.IssueFilialeId;
        }
        public Issue()
        {

        }
       
    }
}
