using System.Threading;
using System.Threading.Tasks;
using HotelBilling.Application.DTOs.Dashboard;

namespace HotelBilling.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
    }
}
