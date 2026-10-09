using HotelBilling.Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HotelBilling.Application.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentListDto>> GetAllAsync();
        Task<PaymentDto?> GetByIdAsync(int id);
        Task<PaymentDto> CreateAsync(PaymentCreateDto dto);
        Task<PaymentDto?> UpdateAsync(PaymentUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<PaymentSummaryDto> GetSummaryAsync();
    }
}
