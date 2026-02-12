using System;

namespace Order.Data.Entities
{
    public partial class OrderItem
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public Guid ServiceId { get; set; }
        public int? Quantity { get; set; }

        public virtual Order Order { get; set; }
        public virtual OrderProduct Product { get; set; }
        public virtual OrderService Service { get; set; }
    }
}
