using Xunit;
using Moq;
using FluentAssertions;
using HotelBilling.Application.Interfaces;
using HotelBilling.Application.Services;
using HotelBilling.Application.DTOs.Request;
using HotelBilling.Domain.Entities;
using HotelBilling.Domain.Interfaces;
using System.Threading.Tasks;

namespace HotelBilling.Tests.Services
{
    public class BillingServiceTests
    {
        private readonly BillingService _billingService;
        private readonly Mock<IRepository<Bill>> _billRepoMock;
        private readonly Mock<IRepository<Room>> _roomRepoMock;
        private readonly Mock<IRepository<Customer>> _customerRepoMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;

        public BillingServiceTests()
        {
            _billRepoMock = new Mock<IRepository<Bill>>();
            _roomRepoMock = new Mock<IRepository<Room>>();
            _customerRepoMock = new Mock<IRepository<Customer>>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _billingService = new BillingService(
                _billRepoMock.Object,
                _roomRepoMock.Object,
                _customerRepoMock.Object,
                _unitOfWorkMock.Object
            );
        }

        [Fact]
        public async Task GenerateBillAsync_Should_Calculate_Total_Correctly()
        {
            // Arrange
            var request = new CreateBillRequest
            {
                CustomerId = 1,
                RoomNumber = 101,
                RoomCharge = 5000,
                FoodCharge = 1200,
                OtherCharges = 300
            };

            decimal expectedTotal = request.RoomCharge + request.FoodCharge + request.OtherCharges;

            // Act
            var bill = await _billingService.GenerateBillAsync(request);

            // Assert
            bill.TotalAmount.Should().Be(expectedTotal);
        }

        [Theory]
        [InlineData(5000, 0, 0, 5000)]
        [InlineData(0, 1000, 200, 1200)]
        [InlineData(2000, 500, 100, 2600)]
        public async Task GenerateBillAsync_Should_Handle_Different_Charges(
            decimal room, decimal food, decimal other, decimal expectedTotal)
        {
            // Arrange
            var request = new CreateBillRequest
            {
                CustomerId = 1,
                RoomNumber = 101,
                RoomCharge = room,
                FoodCharge = food,
                OtherCharges = other
            };

            // Act
            var bill = await _billingService.GenerateBillAsync(request);

            // Assert
            bill.TotalAmount.Should().Be(expectedTotal);
        }
    }
}