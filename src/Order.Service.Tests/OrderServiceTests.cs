using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using NUnit.Framework;
using Order.Data;
using Order.Data.Entities;
using Order.Model;
using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using OrderItemEntity = Order.Data.Entities.OrderItem;
using OrderServiceEntity = Order.Data.Entities.OrderService;

namespace Order.Service.Tests
{
    public class OrderServiceTests
    {
        private IOrderService _orderService;
        private IOrderRepository _orderRepository;
        private OrderContext _orderContext;
        private DbConnection _connection;

        private readonly Guid _orderStatusCreatedId = Guid.NewGuid();
        private readonly Guid _orderStatusInProgressId = Guid.NewGuid();
        private readonly Guid _orderStatusCompletedId = Guid.NewGuid();
        private readonly Guid _orderServiceEmailId = Guid.NewGuid();
        private readonly Guid _orderProductEmailId = Guid.NewGuid();

        private readonly string _orderStatusCreatedName = "Created";
        private readonly string _orderStatusInProgressName = "In Progress";
        private readonly string _orderStatusCompletedName = "Completed";


        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<OrderContext>()
                .UseSqlite(CreateInMemoryDatabase())
                .EnableDetailedErrors(true)
                .EnableSensitiveDataLogging(true)
                .Options;

            _connection = RelationalOptionsExtension.Extract(options).Connection;

            _orderContext = new OrderContext(options);
            _orderContext.Database.EnsureDeleted();
            _orderContext.Database.EnsureCreated();

            _orderRepository = new OrderRepository(_orderContext);
            _orderService = new OrderService(_orderRepository);

            await AddReferenceDataAsync(_orderContext);
        }

        [TearDown]
        public void TearDown()
        {
            _connection.Dispose();
            _orderContext.Dispose();
        }


        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            return connection;
        }

        [Test]
        public async Task GetOrdersAsync_ReturnsCorrectNumberOfOrders()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrdersAsync();

            // Assert
            Assert.AreEqual(3, orders.Count());
        }

        [Test]
        public async Task GetOrdersAsync_ReturnsOrdersWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrdersAsync();

            // Assert
            var order1 = orders.SingleOrDefault(x => x.Id == orderId1);
            var order2 = orders.SingleOrDefault(x => x.Id == orderId2);
            var order3 = orders.SingleOrDefault(x => x.Id == orderId3);

            Assert.AreEqual(0.8m, order1.TotalCost);
            Assert.AreEqual(0.9m, order1.TotalPrice);

            Assert.AreEqual(1.6m, order2.TotalCost);
            Assert.AreEqual(1.8m, order2.TotalPrice);

            Assert.AreEqual(2.4m, order3.TotalCost);
            Assert.AreEqual(2.7m, order3.TotalPrice);
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrder()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(orderId1, order.Id);
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrderItemCount()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(1, order.Items.Count());
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsOrderWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 2);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(1.6m, order.TotalCost);
            Assert.AreEqual(1.8m, order.TotalPrice);
        }

        [Test]
        public async Task GetOrdersAsync_ReturnsFilteredResults()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);
            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2, _orderStatusInProgressId);
            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrdersAsync(_orderStatusInProgressName);

            // Assert
            Assert.AreEqual(orders.Count(), 1);
            Assert.AreEqual(orders.First().StatusName, _orderStatusInProgressName);
        }

        [Test]
        public async Task UpdateOrderStatusAsync_ChangesTheOrderStatus()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1, _orderStatusCreatedId);

            // Act
            await _orderService.UpdateOrderStatusAsync(orderId1, _orderStatusInProgressId);

            // Assert
            var updatedOrder = await _orderContext.Order.FindAsync(orderId1);
            Assert.AreEqual(updatedOrder.StatusId, _orderStatusInProgressId);
        }

        [Test]
        public async Task CreateOrderAsync_CreatesOrderCorrectly()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var resellerId = Guid.NewGuid();
            var createRequest = new OrderCreateRequest
            {
                StatusId = _orderStatusCreatedId,
                CustomerId = customerId,
                ResellerId = resellerId,
                Items = [
                    new() {
                        ProductId = _orderProductEmailId,
                        Quantity = 1
                    }
                ]
            };

            // Act
            var createdOrder = await _orderService.CreateOrderAsync(createRequest);

            // Assert
            Assert.AreEqual(customerId, createdOrder.CustomerId);
            Assert.AreEqual(resellerId, createdOrder.ResellerId);
            Assert.AreEqual(1, createdOrder.Items.Count());
            Assert.AreEqual(_orderProductEmailId, createdOrder.Items.First().ProductId);
            Assert.AreEqual(1, createdOrder.Items.First().Quantity);
        }

        [Test]
        public async Task GetProfitSummary_GroupsOrdersByMonthAndYear()
        {
            // Arrange
            var currentTime = DateTime.UtcNow;
            var orderId1 = Guid.NewGuid();
            var orderId2 = Guid.NewGuid();
            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId1, 1, _orderStatusCompletedId, currentTime.AddMonths(-1));
            await AddOrder(orderId2, 2, _orderStatusCompletedId, currentTime.AddMonths(-1));
            await AddOrder(orderId3, 3, _orderStatusCompletedId, currentTime.AddMonths(0));

            // Act
            var profitSummary = await _orderService.GetProfitSummary();

            // Assert
            Assert.AreEqual(2, profitSummary.Count());
            var currentMonthStart = new DateTime(currentTime.Year, currentTime.Month, 1);
            Assert.AreEqual(currentMonthStart.AddMonths(-1), profitSummary.First().Period);
            Assert.AreEqual(currentMonthStart, profitSummary.Last().Period);
        }

        [Test]
        public async Task GetProfitSummary_FiltersOnlyCompletedOrders()
        {
            // Arrange
            var currentTime = DateTime.UtcNow;
            var orderId1 = Guid.NewGuid();
            var orderId2 = Guid.NewGuid();
            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId1, 1, _orderStatusCompletedId, currentTime.AddMonths(-1));
            await AddOrder(orderId2, 2, _orderStatusCompletedId, currentTime.AddMonths(-1));
            await AddOrder(orderId3, 3, _orderStatusCreatedId, currentTime.AddMonths(0));

            // Act
            var profitSummary = await _orderService.GetProfitSummary();

            // Assert
            Assert.AreEqual(1, profitSummary.Count());
            var currentMonthStart = new DateTime(currentTime.Year, currentTime.Month, 1);
            Assert.AreEqual(currentMonthStart.AddMonths(-1), profitSummary.First().Period);
        }

        [Test]
        public async Task GetProfitSummary_AddsUpTheProfitsCorrectly()
        {
            // Arrange
            var currentTime = DateTime.UtcNow;
            var orderId0 = Guid.NewGuid();
            var orderId1 = Guid.NewGuid();
            var orderId2 = Guid.NewGuid();
            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId0, 10, _orderStatusCompletedId, currentTime.AddMonths(-2));
            await AddOrder(orderId1, 1, _orderStatusCompletedId, currentTime.AddMonths(-1));
            await AddOrder(orderId2, 2, _orderStatusCompletedId, currentTime.AddMonths(-1));
            await AddOrder(orderId3, 3, _orderStatusCompletedId, currentTime.AddMonths(0));

            // Act
            var profitSummary = await _orderService.GetProfitSummary();

            // Assert
            Assert.AreEqual(1m, profitSummary.ElementAt(0).TotalProfit);
            Assert.AreEqual(0.3m, profitSummary.ElementAt(1).TotalProfit);
            Assert.AreEqual(0.3m, profitSummary.ElementAt(2).TotalProfit);
        }

        private async Task AddOrder(
                Guid orderId,
                int quantity,
                Guid? statusId = null,
                DateTime? createdDate = null)
        {
            var realStatusId = statusId.HasValue ? statusId.Value : _orderStatusCreatedId;
            var realCreatedDate = createdDate.HasValue ? createdDate.Value : DateTime.UtcNow;
            _orderContext.Order.Add(new Data.Entities.Order
            {
                Id = orderId,
                ResellerId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                CreatedDate = realCreatedDate,
                StatusId = realStatusId,
            });

            _orderContext.OrderItem.Add(new OrderItemEntity
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ServiceId = _orderServiceEmailId,
                ProductId = _orderProductEmailId,
                Quantity = quantity
            });

            await _orderContext.SaveChangesAsync();
        }

        private async Task AddReferenceDataAsync(OrderContext orderContext)
        {
            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusCreatedId,
                Name = _orderStatusCreatedName,
            });

            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusInProgressId,
                Name = _orderStatusInProgressName,
            });

            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusCompletedId,
                Name = _orderStatusCompletedName,
            });

            orderContext.OrderService.Add(new OrderServiceEntity
            {
                Id = _orderServiceEmailId,
                Name = "Email"
            });

            orderContext.OrderProduct.Add(new OrderProduct
            {
                Id = _orderProductEmailId,
                Name = "100GB Mailbox",
                UnitCost = 0.8m,
                UnitPrice = 0.9m,
                ServiceId = _orderServiceEmailId
            });

            await orderContext.SaveChangesAsync();
        }
    }
}
