using System;
using System.Threading.Tasks;

namespace Order.Data
{
    public interface IOrderProductRepository
    {
        Task<bool> AllProductsExistAsync(Guid[] productIds);
    }
}
