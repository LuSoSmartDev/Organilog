using Organilog.IServices;
using Organilog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.Extensions;

namespace Organilog.Services
{
    public class GroupService : BaseService, IGroupService
    {
        public Task<List<Category>> GetGroups(int userId, int offset = 0, int limit = 0)
        {
            IEnumerable<Category> result = App.LocalDb.Table<Category>().ToList().FindAll(a => a.UserId == userId && a.IsActif == 1).OrderBy(c => c.Title);

            if (offset > 0)
                result = result.Skip(offset);

            if (limit > 0)
                result = result.Take(limit);

            return Task.FromResult(result.ToList());
        }

        public Task<List<Category>> SearchGroup(int userId, string searchKey, int limit = 0)
        {
            IEnumerable<Category> result;

            if (string.IsNullOrWhiteSpace(searchKey))
            {
                result = App.LocalDb.Table<Category>().ToList().FindAll(con => con.UserId == userId && !string.IsNullOrWhiteSpace(con.Title) && con.IsActif == 1).OrderBy(c => c.Title);
            }
            else
            {
                result = App.LocalDb.Table<Category>().ToList().FindAll(con => con.UserId == userId && (!string.IsNullOrWhiteSpace(con.Title) && con.Title.UnSignContains(searchKey)) && con.IsActif == 1).OrderBy(c => c.Title);
            }

            if (limit > 0)
                result = result.Take(limit);

            return Task.FromResult(result.ToList());
        }

        public Task<List<Category>> SearchGroupByClient(Client client, int userId, string searchKey, int limit = 0)
        {
            IEnumerable<Category> result;

            if (string.IsNullOrWhiteSpace(searchKey))
            {
                result = App.LocalDb.Table<Category>().ToList().FindAll(con => con.UserId == userId && con.ClientId == client.ServerId && !string.IsNullOrWhiteSpace(con.Title) && con.IsActif == 1).OrderBy(c => c.Title);
            }
            else
            {
                result = App.LocalDb.Table<Category>().ToList().FindAll(con => con.UserId == userId && con.ClientId == client.ServerId && (!string.IsNullOrWhiteSpace(con.Title) && con.Title.UnSignContains(searchKey)) && con.IsActif == 1).OrderBy(c => c.Title);
            }

            if (limit > 0)
                result = result.Take(limit);

            return Task.FromResult(result.ToList());
        }


    }
}
