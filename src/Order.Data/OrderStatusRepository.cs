using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Order.Data
{
    public class OrderStatusRepository : IOrderStatusRepository
    {
        private readonly OrderContext _orderContext;

        public OrderStatusRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public Task<bool> OrderStatusExistsAsync(Guid orderStatusId)
        {
            return _orderContext.OrderStatus.AnyAsync(x => x.Id == orderStatusId);
        }
    }
}
