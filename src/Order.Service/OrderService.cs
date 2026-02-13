using Order.Data;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync(string statusName = null)
        {
            var orders = await _orderRepository.GetOrdersAsync(statusName);
            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            return order;
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, Guid statusId)
        {
            await _orderRepository.UpdateOrderStatusAsync(orderId, statusId);
        }

        public async Task<bool> OrderExistsAsync(Guid orderId)
        {
            return await _orderRepository.OrderExistsAsync(orderId);
        }

        public async Task<OrderDetail> CreateOrderAsync(OrderCreateRequest createRequest)
        {
            return await _orderRepository.CreateOrderAsync(createRequest);
        }

        public async Task<IEnumerable<ProfitSummary>> GetProfitSummary()
        {
            return await _orderRepository.GetProfitSummary();
        }
    }
}
