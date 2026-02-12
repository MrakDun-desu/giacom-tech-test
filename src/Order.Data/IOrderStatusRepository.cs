using System;
using System.Threading.Tasks;

namespace Order.Data
{
    public interface IOrderStatusRepository
    {
        Task<bool> OrderStatusExistsAsync(Guid orderStatusId);
    }
}
