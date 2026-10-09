using AutoMapper;
using HotelBilling.Application.DTOs;
using HotelBilling.Application.Interfaces;
using HotelBilling.Domain.Entities;
using HotelBilling.Domain.Enums;
using HotelBilling.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HotelBilling.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IRepository<Payment> _paymentRepo;
        private readonly IRepository<Bill> _billRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PaymentService(
            IRepository<Payment> paymentRepo,
            IRepository<Bill> billRepo,
            IRepository<Customer> customerRepo,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _paymentRepo = paymentRepo;
            _billRepo = billRepo;
            _customerRepo = customerRepo;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PaymentDto> CreateAsync(PaymentCreateDto dto)
        {
            var payment = _mapper.Map<Payment>(dto);
            // Optional external PaymentId: generate if not provided
            payment.PaymentId = string.IsNullOrWhiteSpace(dto.PaymentId) ? Guid.NewGuid().ToString() : dto.PaymentId;

            await _paymentRepo.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _paymentRepo.GetByIdAsync(id);
            if (existing == null) return false;

            _paymentRepo.Delete(existing);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<PaymentListDto>> GetAllAsync()
        {
            var payments = (await _paymentRepo.GetAllAsync()).ToList();

            // Load related bills and customers in bulk
            var billIds = payments.Select(x => x.BillId).Distinct().ToList();
            var customerIds = payments.Select(x => x.CustomerId).Distinct().ToList();

            var bills = (await _billRepo.FindAsync(b => billIds.Contains(b.Id))).ToDictionary(b => b.Id);
            var customers = (await _customerRepo.FindAsync(c => customerIds.Contains(c.Id))).ToDictionary(c => c.Id);

            var list = payments.Select(p => new PaymentListDto
            {
                Id = p.Id,
                PaymentId = p.PaymentId,
                PaymentDate = p.PaymentDate,
                PaymentMethod = p.PaymentMethod.ToString(),
                Amount = p.Amount,
                TotalAmount = p.TotalAmount,
                Status = p.PaymentStatus.ToString(),
                CustomerName = customers.TryGetValue(p.CustomerId, out var c) ? (c.FirstName + " " + c.LastName).Trim() : null,
                BillRef = bills.TryGetValue(p.BillId, out var b) ? b.BillNumber : p.BillId.ToString()
            });

            return list;
        }

        public async Task<PaymentDto?> GetByIdAsync(int id)
        {
            var payment = await _paymentRepo.GetByIdAsync(id);
            if (payment == null) return null;

            return _mapper.Map<PaymentDto>(payment);
        }

        public async Task<PaymentSummaryDto> GetSummaryAsync()
        {
            var payments = await _paymentRepo.GetAllAsync();

            var summary = new PaymentSummaryDto
            {
                TotalPayments = payments.Count(),
                TotalAmount = payments.Sum(p => p.TotalAmount),
                TotalTax = payments.Sum(p => p.TaxAmount),
                TotalDiscount = payments.Sum(p => p.Discount),
                PendingCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Pending),
                SuccessCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Paid || p.PaymentStatus == PaymentStatus.Success),
                FailedCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Cancelled || p.PaymentStatus == PaymentStatus.Failed),
                RefundedCount = payments.Count(p => p.PaymentStatus == PaymentStatus.Refunded)
            };

            return summary;
        }

        public async Task<PaymentDto?> UpdateAsync(PaymentUpdateDto dto)
        {
            var existing = await _paymentRepo.GetByIdAsync(dto.Id);
            if (existing == null) return null;

            _mapper.Map(dto, existing);

            _paymentRepo.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PaymentDto>(existing);
        }
    }
}
