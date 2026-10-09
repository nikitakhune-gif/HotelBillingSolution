using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotelBilling.Application.DTOs.Settings;
using HotelBilling.Application.Interfaces;

namespace HotelBilling.Application.Services
{
    // Minimal, in-memory implementation used to satisfy controller DI
    // and provide simple behaviour during development.
    public class SettingsService : ISettingsService
    {
        private static readonly object _lock = new object();
        private static readonly List<SupportTicketDto> _tickets = new List<SupportTicketDto>();
        private static readonly List<AlertDto> _alerts = new List<AlertDto>();
        private static int _nextTicketId = 1;
        private static int _nextAlertId = 1;

        public Task<SettingsDto?> GetSettingsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var settings = new SettingsDto
            {
                SiteName = "Hotel Billing",
                CompanyEmail = "support@hotelbilling.local",
                PhoneNumber = ""
            };

            return Task.FromResult<SettingsDto?>(settings);
        }

        public Task UpdateSettingsAsync(SettingsDto dto, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // No-op for in-memory implementation
            return Task.CompletedTask;
        }

        public Task<IEnumerable<SupportTicketDto>> GetSupportTicketsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_lock)
            {
                return Task.FromResult(_tickets.AsEnumerable());
            }
        }

        public Task<SupportTicketDto?> GetSupportTicketByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_lock)
            {
                var t = _tickets.FirstOrDefault(x => x.Id == id);
                return Task.FromResult(t);
            }
        }

        public Task CreateSupportTicketAsync(CreateSupportTicketDto dto, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var ticket = new SupportTicketDto
            {
                Id = Interlocked.Increment(ref _nextTicketId),
                Subject = dto.Subject ?? string.Empty,
                Message = dto.Message,
                CreatedAt = DateTime.UtcNow
            };

            lock (_lock)
            {
                _tickets.Add(ticket);
            }

            return Task.CompletedTask;
        }

        public Task UpdateSupportTicketAsync(int id, UpdateSupportTicketDto dto, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_lock)
            {
                var t = _tickets.FirstOrDefault(x => x.Id == id);
                if (t != null)
                {
                    t.Subject = dto.Subject ?? t.Subject;
                    t.Message = dto.Message;
                }
            }

            return Task.CompletedTask;
        }

        public Task<IEnumerable<AlertDto>> GetAlertsAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_lock)
            {
                return Task.FromResult(_alerts.AsEnumerable());
            }
        }

        public Task<AlertDto?> GetAlertByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_lock)
            {
                var a = _alerts.FirstOrDefault(x => x.Id == id);
                return Task.FromResult(a);
            }
        }

        public Task MarkAlertAsReadAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_lock)
            {
                var a = _alerts.FirstOrDefault(x => x.Id == id);
                if (a != null)
                {
                    a.IsRead = true;
                }
            }

            return Task.CompletedTask;
        }

        public Task DeleteAlertAsync(int id, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            lock (_lock)
            {
                var a = _alerts.FirstOrDefault(x => x.Id == id);
                if (a != null)
                {
                    _alerts.Remove(a);
                }
            }

            return Task.CompletedTask;
        }
    }
}
