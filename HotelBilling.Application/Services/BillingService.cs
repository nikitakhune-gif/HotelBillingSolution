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
        public async Task<HotelBilling.Application.DTOs.Bill.BillDto> GenerateBillAsync(HotelBilling.Application.DTOs.Bill.BillDto request)
        {
            var customer = await _customerRepo.GetByIdAsync(request.CustomerId);
            if (customer == null) throw new Exception("Customer not found");

            var room = await _roomRepo.GetByIdAsync(request.RoomId);
            if (room == null) throw new Exception("Room not found");

            // Compute total from fields available on BillDto (SubTotal, DiscountAmount, ServiceCharge, TaxAmount)
            decimal totalAmount = Math.Max(0, request.SubTotal - request.DiscountAmount + request.ServiceCharge + request.TaxAmount);

            var bill = new Bill
            {
                CustomerId = request.CustomerId,
                RoomId = request.RoomId,
                TotalAmount = totalAmount,
                PaymentDate = DateTime.UtcNow,
                PaymentStatus = "Pending",
                BillNumber = request.BillNumber
            };

            await _billRepo.AddAsync(bill);
            await _unitOfWork.SaveChangesAsync();

            return new HotelBilling.Application.DTOs.Bill.BillDto
            {
                Id = bill.Id,
                CustomerId = bill.CustomerId,
                RoomId = bill.RoomId,
                TotalAmount = bill.TotalAmount,
                PaymentStatus = bill.PaymentStatus,
                BillNumber = bill.BillNumber
            };
        }
        // GET ALL BILLS
        public async Task<IEnumerable<HotelBilling.Application.DTOs.Bill.BillDto>> GetAllAsync()
        {
            var bills = (await _billRepo.GetAllAsync()).ToList();

            // Load related customers and rooms in bulk to avoid N+1 queries
            var customerIds = bills.Select(x => x.CustomerId).Distinct().ToList();
            var roomIds = bills.Select(x => x.RoomId).Distinct().ToList();

            var customers = (await _customerRepo.FindAsync(c => customerIds.Contains(c.Id))).ToDictionary(c => c.Id);
            var rooms = (await _roomRepo.FindAsync(r => roomIds.Contains(r.Id))).ToDictionary(r => r.Id);

            return bills.Select(b => new HotelBilling.Application.DTOs.Bill.BillDto
            {
                Id = b.Id,
                CustomerId = b.CustomerId,
                CustomerName = customers.TryGetValue(b.CustomerId, out var cust) ? (cust.FirstName + " " + cust.LastName).Trim() : null,
                RoomId = b.RoomId,
                RoomNumber = rooms.TryGetValue(b.RoomId, out var room) ? room.RoomNumber : null,
                SubTotal = b.SubTotal,
                DiscountAmount = b.DiscountAmount,
                TaxAmount = b.TaxAmount,
                ServiceCharge = b.ServiceCharge,
                TotalAmount = b.TotalAmount,
                PaymentStatus = b.PaymentStatus,
                BillNumber = b.BillNumber,
                BillDate = b.BillDate,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                Nights = b.Nights
            });
        }

        // GET BILL BY ID
        public async Task<HotelBilling.Application.DTOs.Bill.BillDto?> GetByIdAsync(int id)
        {
            var bill = await _billRepo.GetByIdAsync(id);

            if (bill == null)
                return null;

            // Ensure related entities are loaded
            var customer = await _customerRepo.GetByIdAsync(bill.CustomerId);
            var room = await _roomRepo.GetByIdAsync(bill.RoomId);

            return new HotelBilling.Application.DTOs.Bill.BillDto
            {
                Id = bill.Id,
                CustomerId = bill.CustomerId,
                CustomerName = customer != null ? (customer.FirstName + " " + customer.LastName).Trim() : null,
                RoomId = bill.RoomId,
                RoomNumber = room != null ? room.RoomNumber : null,
                SubTotal = bill.SubTotal,
                DiscountAmount = bill.DiscountAmount,
                TaxAmount = bill.TaxAmount,
                ServiceCharge = bill.ServiceCharge,
                TotalAmount = bill.TotalAmount,
                PaymentStatus = bill.PaymentStatus,
                BillNumber = bill.BillNumber,
                BillDate = bill.BillDate,
                CheckInDate = bill.CheckInDate,
                CheckOutDate = bill.CheckOutDate,
                Nights = bill.Nights,
                Items = bill.Items != null ? bill.Items.Select(i => new HotelBilling.Application.DTOs.Bill.BillItemDto
                {
                    Id = i.Id,
                    ItemName = i.ItemName,
                    Quantity = i.Quantity,
                    Rate = i.Rate,
                    Amount = i.Amount
                }).ToList() : new List<HotelBilling.Application.DTOs.Bill.BillItemDto>()
            };
        }

        // UPDATE PAYMENT STATUS
        public async Task<bool> UpdatePaymentStatusAsync(int billId, int status)
        {
            var bill = await _billRepo.GetByIdAsync(billId);

            if (bill == null)
                return false;

            // Bill.PaymentStatus is a string property; store the enum name
            bill.PaymentStatus = ((PaymentStatus)status).ToString();

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