using FluentAssertions;
using HotelBilling.Application.DTOs.Request;
using HotelBilling.Application.DTOs.Response;
using HotelBilling.Application.Interfaces;
using HotelBilling.Web.Controllers;
using HotelBillingWeb.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit; // xUnit only

namespace HotelBilling.Tests.Controllers
{
    public class BillingControllerTests
    {
        private readonly Mock<IBillingService> _billingServiceMock;
        private readonly Mock<ICustomerService> _customerServiceMock;
        private readonly BillingController _controller;

        public BillingControllerTests()
        {
            _billingServiceMock = new Mock<IBillingService>();
            _customerServiceMock = new Mock<ICustomerService>();
            _controller = new BillingController(_billingServiceMock.Object, _customerServiceMock.Object);
        }

        [Fact]
        public async Task Index_Should_Return_View_With_Bills()
        {
            // Arrange
            var bills = new List<BillResponse>
            {
                new BillResponse { Id = 1, TotalAmount = 5000 },
                new BillResponse { Id = 2, TotalAmount = 6500 }
            };

            _billingServiceMock.Setup(s => s.GetAllAsync())
                               .ReturnsAsync(bills);

            // Act
            var result = await _controller.Index();
            var viewResult = result as ViewResult;

            // Assert
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().BeEquivalentTo(bills);
        }

        [Fact]
        public async Task Create_GET_Should_Return_View()
        {
            // Act
            var result = await _controller.Create(); // await the async method
            var viewResult = result as ViewResult;

            // Assert
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().BeNull();
        } // GET action usually returns empty view
        

        [Fact]
        public async Task Create_POST_Should_Redirect_To_Index_When_Model_Is_Valid()
        {
            // Arrange
            var request = new CreateBillRequest
            {
                CustomerId = 1,
                RoomId = 101,
                RoomCharge = 5000,
                FoodCharge = 1200,
                OtherCharges = 300
            };

            _billingServiceMock.Setup(s => s.GenerateBillAsync(request))
                               .ReturnsAsync(new BillResponse { TotalAmount = 6500 });

            // Act
            var result = await _controller.Create(request);
            var redirectResult = result as RedirectToActionResult;

            // Assert
            redirectResult.Should().NotBeNull();
            redirectResult!.ActionName.Should().Be("Index");

            _billingServiceMock.Verify(s => s.GenerateBillAsync(request), Times.Once);
        }

        [Fact]
        public async Task Details_Should_Return_View_With_Bill()
        {
            // Arrange
            var billId = 1;
            var bill = new BillResponse { Id = billId, TotalAmount = 6500 };

            _billingServiceMock.Setup(s => s.GetByIdAsync(billId))
                               .ReturnsAsync(bill);

            // Act
            var result = await _controller.Details(billId);
            var viewResult = result as ViewResult;

            // Assert
            viewResult.Should().NotBeNull();
            viewResult!.Model.Should().BeEquivalentTo(bill);
        }
    }
}