using Organilog.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Organilog.IServices
{
    public interface IProductService
    {
        Task<List<Product>> GetProducts(int userId, int offset = 0, int limit = 0);


        Task<List<Product>> SearchProduct(int userId, string searchKey, int limit = 0);
    }
}
