using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Organilog.Models.Response;
using SQLite;
using TinyMVVM;

namespace Organilog.Models
{
    [Table("IssueLink")]
    public class IssueLink : BaseModel
    {
        [JsonProperty("issrId")]
        [PrimaryKey]
        public Guid Id { get; set; }

        [JsonProperty("issrUserId")]
        public int UserId { get; set; }

        [JsonProperty("issrIdServer")]
        public int ServerId { get; set; }

        [JsonProperty("issrFkIssueId")]
        public int IssueId { get; set; }

        [JsonProperty("issrFkIssueAppliId")]
        public Guid IssueApplId { get; set; }


        [JsonProperty("issrMessage")]
        public string Message { get; set; }

        [JsonProperty("issrCommentPrivate")]
        public string CommentPrivate { get; set; }

        [JsonProperty("issrType")]
        public string Type { get; set; }

        [JsonProperty("issrAddDate")]
        public DateTime? AddDate { get; set; }

        [JsonProperty("issrModifOn")]
        public DateTime? EditDate { get; set; }

        [JsonProperty("issrAuthorType")]
        public int AuthorType { get; set; }

        [JsonProperty("issrAddedByFkUserId")]
        public int AddedByFkUserId { get; set; }

        private int isActif;
        [JsonProperty("issrOn")]
        public int IsActif { get => isActif; set => SetProperty(ref isActif, value); }

        private string name;
        [JsonIgnore]
        public string displayOwner { get => name; set => SetProperty(ref name, value, onChanged: () => OnPropertyChanged(nameof(displayOwner))); } 

        [JsonIgnore]
        public bool IsToSync { get; set; }

        [Ignore]
        public string PropertyIgnore
        {
            get
            {
                return string.Join(",", nameof(IsToSync));
            }
        }


        public IssueLink() { 
            
        }

        public IssueLink(IssueReplyResponse response)
        {
            if (!string.IsNullOrWhiteSpace(response.K) && Guid.TryParse(response.K, out Guid id))
                Id = id;
            else
                Id = Guid.NewGuid();

            AddDate = response.AddDate;
            ServerId = response.ServerId;
            IssueId = response.FKIssueId;
            Message = response.Message;
            CommentPrivate = response.CommentPrivate;
            Type = response.Type;
            IsActif = response.IsActif;
            EditDate = response.EditDate;
            AddDate = response.AddDate;
            AuthorType = response.AuthorType;
            AddedByFkUserId = response.AddedByFkUserId;
        }
        public void UpdateFromResponse(IssueReplyResponse response)
        {
            AddDate = response.AddDate;
            ServerId = response.ServerId;
            IssueId = response.FKIssueId;
            Message = response.Message;
            CommentPrivate = response.CommentPrivate;
            Type = response.Type;
            IsActif = response.IsActif;
            EditDate = response.EditDate;
            AddDate = response.AddDate;
            AuthorType = response.AuthorType;
            AddedByFkUserId = response.AddedByFkUserId;

        }


    }
}
