using Microsoft.EntityFrameworkCore;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderEntity = Order.Data.Entities.Order;
using OrderItemEntity = Order.Data.Entities.OrderItem;

namespace Order.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _orderContext;

        public OrderRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync(string statusName = null)
        {
            var ordersQuery = _orderContext.Order
                .Include(x => x.Items)
                .Include(x => x.Status)
                .AsQueryable();

            if (statusName is not null)
            {
                ordersQuery = ordersQuery.Where(x => x.Status.Name == statusName);
            }

            var orders = await ordersQuery
                .Select(x => new OrderSummary
                {
                    Id = x.Id,
                    ResellerId = x.ResellerId,
                    CustomerId = x.CustomerId,
                    StatusId = x.StatusId,
                    StatusName = x.Status.Name,
                    ItemCount = x.Items.Count,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    CreatedDate = x.CreatedDate
                })
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return orders;
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderContext.Order
                .Where(x => x.Id == orderId)
                .Select(x => new OrderDetail
                {
                    Id = x.Id,
                    ResellerId = x.ResellerId,
                    CustomerId = x.CustomerId,
                    StatusId = x.StatusId,
                    StatusName = x.Status.Name,
                    CreatedDate = x.CreatedDate,
                    TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                    TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                    Items = x.Items.Select(i => new Model.OrderItem
                    {
                        Id = i.Id,
                        OrderId = i.OrderId,
                        ServiceId = i.ServiceId,
                        ServiceName = i.Service.Name,
                        ProductId = i.ProductId,
                        ProductName = i.Product.Name,
                        UnitCost = i.Product.UnitCost,
                        UnitPrice = i.Product.UnitPrice,
                        TotalCost = i.Product.UnitCost * i.Quantity.Value,
                        TotalPrice = i.Product.UnitPrice * i.Quantity.Value,
                        Quantity = i.Quantity.Value
                    })
                }).SingleOrDefaultAsync();

            return order;
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, Guid statusId)
        {
            var order = await _orderContext.Order.SingleAsync(x => x.Id == orderId);
            order.StatusId = statusId;
            await _orderContext.SaveChangesAsync();
        }

        public async Task<bool> OrderExistsAsync(Guid orderId)
        {
            return await _orderContext.Order.AnyAsync(x => x.Id == orderId);
        }

        public async Task<OrderDetail> CreateOrderAsync(OrderCreateRequest request)
        {
            // honestly not really sure why service IDs are stored in the order item table as well,
            // I would expect the services to be directly linked to the products. This would be
            // better to discuss with management in a real-world scenario
            //
            // Similarly to before, running this through foreach because the better SQL "IN" syntax
            // doesn't seem to be available here for .Contains method
            var productServiceIds = new Dictionary<Guid, Guid>();
            foreach (var item in request.Items)
            {
                var serviceId = await _orderContext.OrderProduct
                    .Where(x => x.Id == item.ProductId)
                    .Select(x => x.ServiceId)
                    .SingleAsync();
                productServiceIds[item.ProductId] = serviceId;
            }
            var newOrder = new OrderEntity
            {
                Id = Guid.NewGuid(),
                StatusId = request.StatusId,
                ResellerId = request.ResellerId,
                CustomerId = request.CustomerId,
                Items = request.Items.Select(x => new OrderItemEntity
                {
                    Id = Guid.NewGuid(),
                    ProductId = x.ProductId,
                    Quantity = x.Quantity,
                    ServiceId = productServiceIds[x.ProductId]
                }).ToHashSet(),
                CreatedDate = DateTime.UtcNow
            };

            _orderContext.Order.Add(newOrder);

            await _orderContext.SaveChangesAsync();

            return await GetOrderByIdAsync(newOrder.Id);
        }

        public async Task<IEnumerable<ProfitSummary>> GetProfitSummary()
        {
            var orderProfits = await _orderContext.Order
                .Include(x => x.Status)
                .Where(x => x.Status.Name == "Completed")
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .Select(x => new
                {
                    x.CreatedDate,
                    Profit = x.Items.Sum(i => (i.Quantity ?? 0) * (i.Product.UnitPrice - i.Product.UnitCost))
                })
            .ToArrayAsync();

            // Need to split the query here and compute the actual monthly profits on the API.
            // The database should be able to handle this as a joint query, but the current provider
            // doesn't seem to translate it correctly (maybe due to old DB version?)
            var monthlyProfits = orderProfits
                .GroupBy(x => new
                {
                    x.CreatedDate.Year,
                    x.CreatedDate.Month
                })
                .Select(g => new ProfitSummary
                {
                    Period = new DateTime(g.Key.Year, g.Key.Month, 1),
                    TotalProfit = g.Sum(x => x.Profit)
                })
                .OrderBy(x => x.Period);

            return monthlyProfits;
        }
    }
}
