using System;
using System.Threading.Tasks;

namespace Order.Service
{
    public interface IOrderProductService
    {
        Task<bool> AllProductsExistAsync(Guid[] productIds);
    }
}
