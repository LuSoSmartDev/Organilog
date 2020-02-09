using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Organilog.IServices;
using Organilog.Models;
using Xamarin.Forms.Extensions;

namespace Organilog.Services
{
    public class IssueService: BaseService,IIssueService
    {
        public IssueService()
        {

        }

        public Task<List<Issue>> GetIssues ()
        {
            IEnumerable<Issue> result = App.LocalDb.Table<Issue>().ToList().FindAll(iss => iss.UserId == CurrentUser.Id && iss.IsActif == 1)
               .OrderByDescending(iss => iss.DateStart).ThenByDescending(iss => iss.EditDate).ThenByDescending(iss => iss.AddDate);

            foreach (var item in result)
            {
                //TODO 
                GetRelations(item);

            }

            return Task.FromResult(result.ToList());
        }

        public Task<Issue> GetRelations(Issue issue)
        {
            issue.Client = App.LocalDb.Table<Client>().FirstOrDefault(c => c.UserId == CurrentUser.Id && ((c.ServerId > 0 && c.ServerId == issue.FkClientServerId)) && c.IsActif == 1);
            issue.Address = App.LocalDb.Table<Address>().FirstOrDefault(a => a.UserId == CurrentUser.Id && (a.ServerId > 0 && a.ServerId == issue.FkAddressServerId) && a.IsActif == 1);
            var filiale =  App.LocalDb.Table<Filiale>().FirstOrDefault(fil => fil.UserId == CurrentUser.Id && (fil.ServerId > 0 && fil.ServerId == issue.FkFilialeServerId) && fil.IsActif == 1);

            if (filiale != null)
                issue.FilialeName = filiale.Nom;

            issue.Messages = App.LocalDb.Table<IssueLink>().ToList().FindAll(issr => issr.UserId == CurrentUser.Id && ((issr.IssueId > 0 && issr.IssueId == issue.ServerId)||(!issr.IssueApplId.Equals(Guid.Empty)&& issr.IssueApplId.Equals(issue.Id))) && issr.IsActif == 1).ToObservableCollection();

            foreach (var item in issue.Messages)
            {
                if (item != null)
                {
                    var authorName = "Unknown";
                    if (item.AuthorType == 0)
                    {
                        var user = App.LocalDb.Table<User>().ToList().FirstOrDefault(u => item.AddedByFkUserId > 0 && item.AddedByFkUserId == u.ServerId && u.IsActif == 1);
                        if (user != null)
                            authorName = user.FullName;
                    }
                    else if (item.AuthorType == 1)
                    {
                        authorName = issue.Client.FullName;
                    }


                    var dateformatstring = String.Format("{0:dd/MM/yyyy à HH:mm}", item.EditDate != null ? item.EditDate : item.AddDate);
                    item.displayOwner = string.Format("{0}, le {1}", authorName, dateformatstring);

                }

            }

            issue.MediaLinks = App.LocalDb.Table<MediaLink>().ToList().FindAll(medl => medl.UserId == CurrentUser.Id && ((medl.FkColumnServerId > 0 && medl.FkColumnServerId == issue.ServerId) || (!medl.FkColumnAppliId.Equals(Guid.Empty) && medl.FkColumnAppliId.Equals(issue.Id))) && !medl.IsDelete && medl.IsActif == 1).ToObservableCollection();
            foreach (var medl in issue.MediaLinks)
            {
                medl.Media = App.LocalDb.Table<Media>().ToList().FirstOrDefault(med => med.UserId == CurrentUser.Id && ((medl.FkMediaServerId > 0 && medl.FkMediaServerId == med.ServerId) || (!medl.FkMediaAppliId.Equals(Guid.Empty) && !med.Id.Equals(Guid.Empty) && medl.FkMediaAppliId.Equals(med.Id))) && med.IsActif == 1);
            }
            return Task.FromResult(issue);
        }

      
        public Task<Issue> GetIssueDetail(Guid id)
        {
            if (App.LocalDb.Table<Issue>().ToList().Find(i => !i.Id.Equals(Guid.Empty) && !id.Equals(Guid.Empty) && i.Id.Equals(id)) is Issue issue)
            {
                GetRelations(issue);
                return Task.FromResult(issue);
            }
      
            return null;
        }

       

        public Task<List<Issue>> SearchIssues(string searchKey, int limit = 0)
        {
            return null;
        }

        public Task<bool> UpdateIssue(Issue issue)
        {
            try
            {
                return Task.FromResult(App.LocalDb.Update(issue) > 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Update intervention failed: " + ex);
                return Task.FromResult(false);
            }
        }

        public Task<bool> CreateIssue(Issue issue)
        {
            try
            {
                return Task.FromResult(App.LocalDb.Insert(issue) > 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Create issue failed: " + ex);
                return Task.FromResult(false);
            }
        }

        public Task<bool> SaveIssue(Issue issue)
        {
            try
            {
                return Task.FromResult(App.LocalDb.InsertOrReplace(issue) > 0);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Save issue failed: " + ex);
                return Task.FromResult(false);
            }
        }

       
    }
}
