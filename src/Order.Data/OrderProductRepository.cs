using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Data
{
    public class OrderProductRepository : IOrderProductRepository
    {
        private readonly OrderContext _orderContext;

        public OrderProductRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<bool> AllProductsExistAsync(Guid[] productIds)
        {
            foreach (var id in productIds)
            {
                var productExists = await _orderContext.OrderProduct.AnyAsync(x => x.Id == id);
                if (!productExists)
                {
                    return false;
                }
            }
            return true;

            // This approach should be a lot faster using the "IN" query, but it doesn't seem to
            // be translated correctly
            // var realProductCount = await _orderContext.OrderProduct
            //     .Where(x => productIds.Contains(x.Id))
            //     .CountAsync();
            //
            // return productIds.Length == realProductCount;
        }
    }
}
