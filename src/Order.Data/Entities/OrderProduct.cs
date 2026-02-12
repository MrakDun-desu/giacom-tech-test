using System;
using System.Collections.Generic;

namespace Order.Data.Entities
{
    public partial class OrderProduct
    {
        public OrderProduct()
        {
            OrderItem = new HashSet<OrderItem>();
        }

        public Guid Id { get; set; }
        public Guid ServiceId { get; set; }
        public string Name { get; set; }
        public decimal UnitCost { get; set; }
        public decimal UnitPrice { get; set; }

        public virtual OrderService Service { get; set; }
        public virtual ICollection<OrderItem> OrderItem { get; set; }
    }
}
