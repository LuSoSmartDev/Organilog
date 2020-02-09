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
    public class ProductService : BaseService, IProductService
    {
        public Task<List<Product>> GetProducts(int userId, int offset = 0, int limit = 0)
        {
            var result = App.LocalDb.Table<Product>().ToList().FindAll(pr => pr.UserId == userId && !string.IsNullOrWhiteSpace(pr.Nom) && pr.IsActif == 1).OrderBy(p => p.Nom).ToList();

            if (offset > 0)
                result = result.Skip(offset).ToList();

            if (limit > 0)
                result = result.Take(limit).ToList();

            return Task.FromResult(result);
        }

        public Task<List<Product>> SearchProduct(int userId, string searchKey, int limit = 0)
        {
            var result =  App.LocalDb.Table<Product>().ToList().FindAll(pr => pr.UserId == userId && ((!string.IsNullOrWhiteSpace(pr.Code) && pr.Code.Contains(searchKey)) || (!string.IsNullOrWhiteSpace(pr.Nom) && pr.Nom.UnSignContains(searchKey))) && pr.IsActif == 1).OrderBy(p=>p.Code).ToList();
            //var result = App.LocalDb.Table<Client>().ToList().FindAll(c => c.UserId == userId && ((c.Code > 0 && c.Code.ToString().Contains(searchKey)) || (!string.IsNullOrWhiteSpace(c.Title) && c.Title.UnSignContains(searchKey)) || (!string.IsNullOrWhiteSpace(c.FullName) && c.FullName.UnSignContains(searchKey))) && c.IsActif == 1).OrderBy(c => c.Code).ToList();

            if (limit > 0)
                result = result.Take(limit).ToList();

            
            return Task.FromResult(result);
        }
    }
}
