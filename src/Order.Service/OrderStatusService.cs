using Order.Data;
using System;
using System.Threading.Tasks;

namespace Order.Service
{
    public class OrderStatusService : IOrderStatusService
    {
        private readonly IOrderStatusRepository _orderStatusRepository;

        public OrderStatusService(IOrderStatusRepository orderStatusRepository)
        {
            _orderStatusRepository = orderStatusRepository;
        }

        public async Task<bool> OrderStatusExistsAsync(Guid statusId)
        {
            return await _orderStatusRepository.OrderStatusExistsAsync(statusId);
        }
    }
}
