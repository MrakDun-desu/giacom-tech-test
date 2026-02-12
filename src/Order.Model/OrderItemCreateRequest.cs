using System;

namespace Order.Model
{
    public class OrderItemCreateRequest
    {
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }
    }
}
