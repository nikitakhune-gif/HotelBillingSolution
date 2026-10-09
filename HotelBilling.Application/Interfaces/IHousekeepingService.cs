using HotelBillingSolution.Application.DTOs.Housekeeping;
using HotelBillingSolution.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelBilling.Application.Interfaces
{
    public interface IHousekeepingService
    {
        Task<int> CreateTaskAsync(HousekeepingTaskDto request);

        Task<IEnumerable<HousekeepingTaskDto>> GetAllTasksAsync();

        Task<HousekeepingTaskDto?> GetTaskByIdAsync(int id);

        Task<bool> UpdateTaskAsync(int id, HousekeepingTaskDto request);

        Task<bool> DeleteTaskAsync(int id);

        Task<dynamic> GetAllForViewAsync();
    }
}
