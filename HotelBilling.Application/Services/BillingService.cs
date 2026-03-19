using HotelBilling.Application.DTOs.Request;
using HotelBilling.Application.DTOs.Response;
using HotelBilling.Application.Interfaces;
using HotelBilling.Domain.Entities;
using HotelBilling.Domain.Enums;
using HotelBilling.Domain.Interfaces;


namespace HotelBilling.Application.Services
{
    public class BillingService : IBillingService
    {
        private readonly IRepository<Bill> _billRepo;
        private readonly IRepository<Room> _roomRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IUnitOfWork _unitOfWork;

        public BillingService(
            IRepository<Bill> billRepo,
            IRepository<Room> roomRepo,
            IRepository<Customer> customerRepo,
            IUnitOfWork unitOfWork)
        {
            _billRepo = billRepo;
            _roomRepo = roomRepo;
            _customerRepo = customerRepo;
            _unitOfWork = unitOfWork;
        }

        // CREATE BILL - updated to match interface
        public async Task<BillResponse> GenerateBillAsync(CreateBillRequest request)
        {
            var customer = await _customerRepo.GetByIdAsync(request.CustomerId);
            if (customer == null) throw new Exception("Customer not found");

            var room = await _roomRepo.GetByIdAsync(request.RoomId);
            if (room == null) throw new Exception("Room not found");

            decimal totalAmount = request.RoomCharge + request.FoodCharge + request.OtherCharges;

            var bill = new Bill
            {
                CustomerId = request.CustomerId,
                RoomId = request.RoomId,
                Amount = totalAmount,
                PaymentStatus = PaymentStatus.Pending
            };

            await _billRepo.AddAsync(bill);
            await _unitOfWork.SaveChangesAsync();

            return new BillResponse
            {
                Id = bill.Id,
                CustomerId = bill.CustomerId,
                RoomId = bill.RoomId,
                Amount = bill.Amount,
                PaymentStatus = bill.PaymentStatus.ToString(),
                TotalAmount = bill.Amount
            };
        }        // GET ALL BILLS
        public async Task<IEnumerable<BillResponse>> GetAllAsync()
        {
            var bills = await _billRepo.GetAllAsync();

            return bills.Select(b => new BillResponse
            {
                Id = b.Id,
                CustomerId = b.CustomerId,
                RoomId = b.RoomId,
                Amount = b.Amount,
                PaymentStatus = b.PaymentStatus.ToString(),
                TotalAmount = b.Amount
            });
        }

        // GET BILL BY ID
        public async Task<BillResponse?> GetByIdAsync(int id)
        {
            var bill = await _billRepo.GetByIdAsync(id);

            if (bill == null)
                return null;

            return new BillResponse
            {
                Id = bill.Id,
                CustomerId = bill.CustomerId,
                RoomId = bill.RoomId,
                Amount = bill.Amount,
                PaymentStatus = bill.PaymentStatus.ToString(),
                TotalAmount = bill.Amount
            };
        }

        // UPDATE PAYMENT STATUS
        public async Task<bool> UpdatePaymentStatusAsync(int billId, int status)
        {
            var bill = await _billRepo.GetByIdAsync(billId);

            if (bill == null)
                return false;

            bill.PaymentStatus = (PaymentStatus)status;

            _billRepo.Update(bill);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        // DELETE BILL
        public async Task<bool> DeleteBillAsync(int id)
        {
            var bill = await _billRepo.GetByIdAsync(id);

            if (bill == null)
                return false;

            _billRepo.Delete(bill);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
    }
}