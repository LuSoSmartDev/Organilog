using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Organilog.Models;

namespace Organilog.IServices
{
    public interface IIssueService
    {
        Task<List<Issue>> GetIssues();
        
        Task<List<Issue>> SearchIssues(string searchKey, int limit = 0);

        Task<Issue> GetIssueDetail(Guid id);

        Task<bool> UpdateIssue(Issue issue);

        Task<bool> CreateIssue(Issue issue);

        Task<bool> SaveIssue(Issue issue);

        Task<Issue> GetRelations(Issue issue);

    }
}
