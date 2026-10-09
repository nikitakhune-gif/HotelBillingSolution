using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotelBilling.Application.DTOs.Settings;

namespace HotelBilling.Application.Interfaces
{
    public interface ISettingsService
    {
        Task<SettingsDto?> GetSettingsAsync(CancellationToken cancellationToken = default);

        Task UpdateSettingsAsync(SettingsDto dto, CancellationToken cancellationToken = default);

        Task<IEnumerable<SupportTicketDto>> GetSupportTicketsAsync(CancellationToken cancellationToken = default);

        Task<SupportTicketDto?> GetSupportTicketByIdAsync(int id, CancellationToken cancellationToken = default);

        Task CreateSupportTicketAsync(CreateSupportTicketDto dto, CancellationToken cancellationToken = default);

        Task UpdateSupportTicketAsync(int id, UpdateSupportTicketDto dto, CancellationToken cancellationToken = default);

        Task<IEnumerable<AlertDto>> GetAlertsAsync(CancellationToken cancellationToken = default);

        Task<AlertDto?> GetAlertByIdAsync(int id, CancellationToken cancellationToken = default);

        Task MarkAlertAsReadAsync(int id, CancellationToken cancellationToken = default);

        Task DeleteAlertAsync(int id, CancellationToken cancellationToken = default);
    }
}
