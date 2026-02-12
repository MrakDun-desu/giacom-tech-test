using Order.Data;
using System;
using System.Threading.Tasks;

namespace Order.Service
{
    public class OrderProductService : IOrderProductService
    {
        private readonly IOrderProductRepository _orderProductRepository;

        public OrderProductService(IOrderProductRepository orderProductRepository)
        {
            _orderProductRepository = orderProductRepository;
        }

        public async Task<bool> AllProductsExistAsync(Guid[] productIds)
        {
            return await _orderProductRepository.AllProductsExistAsync(productIds);
        }
    }
}
