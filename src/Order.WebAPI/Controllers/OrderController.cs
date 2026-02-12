using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Model;
using Order.Service;
using System;
using System.Threading.Tasks;

namespace OrderService.WebAPI.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderStatusService _orderStatusService;

        public OrderController(IOrderService orderService, IOrderStatusService orderStatusService)
        {
            _orderService = orderService;
            _orderStatusService = orderStatusService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get([FromQuery] string statusName)
        {
            var orders = await _orderService.GetOrdersAsync(statusName);
            return Ok(orders);
        }

        [HttpGet("{orderId}")]
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
    }
}
