using System;
using System.Collections.Generic;

namespace Order.Data.Entities
{
    public partial class Order
    {
        public Order()
        {
            Items = new HashSet<OrderItem>();
        }

        public Guid Id { get; set; }
        public Guid ResellerId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid StatusId { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual OrderStatus Status { get; set; }
        public virtual ICollection<OrderItem> Items { get; set; }
    }
}
