using System;

namespace Order.Model;

public class OrderStatusUpdateRequest
{
    public Guid NewStatusId { get; init; }
}
