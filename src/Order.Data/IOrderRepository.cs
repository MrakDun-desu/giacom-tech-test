using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Data
{
    public interface IOrderRepository
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync(string statusName = null);

        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);

        Task UpdateOrderStatusAsync(Guid orderId, Guid statusId);

        Task<bool> OrderExistsAsync(Guid orderId);

        Task<OrderDetail> CreateOrderAsync(OrderCreateRequest request);

        Task<IEnumerable<ProfitSummary>> GetProfitSummary();
    }
}
