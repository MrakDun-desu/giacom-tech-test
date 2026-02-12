using System;
using System.Threading.Tasks;

namespace Order.Service
{
    public interface IOrderStatusService
    {
        Task<bool> OrderStatusExistsAsync(Guid statusId);
    }
}
