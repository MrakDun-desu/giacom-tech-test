using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Model;
using Order.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.WebAPI.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderStatusService _orderStatusService;
        private readonly IOrderProductService _orderProductService;

        public OrderController(
                IOrderService orderService,
                IOrderStatusService orderStatusService,
                IOrderProductService orderProductService
                )
        {
            _orderService = orderService;
            _orderStatusService = orderStatusService;
            _orderProductService = orderProductService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery] string statusName = null)
        {
            var orders = await _orderService.GetOrdersAsync(statusName);
            return Ok(orders);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] OrderCreateRequest createRequest)
        {
            var validationErrors = new Dictionary<string, string[]>();
            var statusExists = await _orderStatusService.OrderStatusExistsAsync(createRequest.StatusId);
            if (!statusExists)
            {
                validationErrors.Add(
                        nameof(createRequest.StatusId),
                        [$"Status with ID {createRequest.StatusId} doesn't exist"]
                    );
            }

            var productIds = createRequest.Items.Select(x => x.ProductId).ToArray();
            if (productIds.Length == 0)
            {
                validationErrors.Add(
                        nameof(createRequest.Items),
                        [$"Cannot create an order with no order items"]
                        );
            }
            else
            {
                var allProductsHavePositiveQuantities = createRequest.Items.Select(x => x.Quantity).All(x => x > 0);
                if (!allProductsHavePositiveQuantities)
                {
                    validationErrors.Add(
                            nameof(createRequest.Items),
                            [$"Cannot create an order with negative product quantity"]
                            );
                }
                var allProductsExist = await _orderProductService.AllProductsExistAsync(productIds);
                if (!allProductsExist)
                {
                    validationErrors.Add(
                            nameof(createRequest.Items),
                            [$"Some of the specified products do not exist"]
                            );
                }
            }

            if (validationErrors.Count != 0)
            {
                return ValidationProblem(new ValidationProblemDetails(validationErrors));
            }

            var order = await _orderService.CreateOrderAsync(createRequest);
            return new CreatedAtRouteResult(nameof(GetOrderById), new { orderId = order.Id }, order);
        }

        [HttpGet("{orderId}", Name = nameof(GetOrderById))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order != null)
            {
                return Ok(order);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("{orderId:guid}/status")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] OrderStatusUpdateRequest updateRequest)
        {
            var orderExists = await _orderService.OrderExistsAsync(orderId);
            if (!orderExists)
            {
                return NotFound();
            }

            var statusExists = await _orderStatusService.OrderStatusExistsAsync(updateRequest.NewStatusId);
            if (!statusExists)
            {
                return BadRequest();
            }

            await _orderService.UpdateOrderStatusAsync(orderId, updateRequest.NewStatusId);

            return NoContent();
        }

        [HttpGet("profit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfitByMonth()
        {
            throw new NotImplementedException();
        }
    }
}
