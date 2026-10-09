using AutoMapper;
using HotelBilling.Application.DTOs;
using HotelBillingSolution.Domain.Entities;
using HotelBilling.Domain.Entities;
using HotelBilling.Domain.Enums;
using System;
using HotelBillingSolution.Models;
using HotelBillingSolution.Application.DTOs.Housekeeping;

namespace HotelBilling.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Entity -> DTO
            CreateMap<Payment, PaymentDto>()
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.PaymentStatus.ToString()))
                .ForMember(d => d.CreatedDate,
                    o => o.MapFrom(s => s.CreatedDate))
                .ForMember(d => d.UpdatedDate,
                    o => o.MapFrom(s => s.UpdatedDate))
                .ForMember(d => d.Method,
                    o => o.MapFrom(s => s.PaymentMethod.ToString()));

            // Create DTO -> Entity
            CreateMap<PaymentCreateDto, Payment>()
                .ForMember(d => d.PaymentStatus,
                    o => o.MapFrom(s => PaymentStatus.Pending))
                .ForMember(d => d.CreatedDate,
                    o => o.MapFrom(s => DateTime.UtcNow))
                .AfterMap((src, dest) =>
                {
                    if (!string.IsNullOrWhiteSpace(src.Method) &&
                        Enum.TryParse<PaymentMethod>(src.Method, true, out var m))
                    {
                        dest.PaymentMethod = m;
                    }
                    else
                    {
                        dest.PaymentMethod = PaymentMethod.Cash;
                    }
                });

            // Update DTO -> Entity
            CreateMap<PaymentUpdateDto, Payment>()
                .AfterMap((src, dest) =>
                {
                    if (!string.IsNullOrWhiteSpace(src.Status) &&
                        Enum.TryParse<PaymentStatus>(src.Status, true, out var status))
                    {
                        dest.PaymentStatus = status;
                    }

                    if (!string.IsNullOrWhiteSpace(src.Method) &&
                        Enum.TryParse<PaymentMethod>(src.Method, true, out var pm))
                    {
                        dest.PaymentMethod = pm;
                    }
                });

            // Entity -> List DTO
            CreateMap<Payment, PaymentListDto>()
                .ForMember(d => d.Status,
                    o => o.MapFrom(s => s.PaymentStatus.ToString()))
                .ForMember(d => d.PaymentMethod,
                    o => o.MapFrom(s => s.PaymentMethod.ToString()))
                .ForMember(d => d.CustomerName,
                    o => o.MapFrom(s => s.Customer != null ? s.Customer.FirstName + " " + s.Customer.LastName : string.Empty))
                .ForMember(d => d.BillRef,
                    o => o.MapFrom(s => s.Bill != null ? s.Bill.BillNumber : s.BillId.ToString()));

            // Housekeeping mappings
            CreateMap<HousekeepingTask, HotelBillingSolution.Models.HousekeepingTaskDto>()
                .ForMember(d => d.HousekeepingTaskId, o => o.MapFrom(s => s.HousekeepingTaskId))
                .ForMember(d => d.AssignedTo, o => o.MapFrom(s => s.AssignedToStaffId))
                .ForMember(d => d.Floor, o => o.MapFrom(s => ConvertFloorToNullableInt(s.Floor)));

            CreateMap<HotelBillingSolution.Models.HousekeepingTaskDto, HousekeepingTask>()
                .ForMember(d => d.AssignedToStaffId, o => o.MapFrom(s => s.AssignedTo))
                .ForMember(d => d.HousekeepingTaskId, o => o.MapFrom(s => s.HousekeepingTaskId))
                .ForMember(d => d.Floor, o => o.MapFrom(s => s.Floor.HasValue ? s.Floor.Value.ToString() : null));

            // Bill mappings
            CreateMap<Bill, HotelBilling.Application.DTOs.Bill.BillDto>()
                .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null ? s.Customer.FirstName + " " + s.Customer.LastName : string.Empty))
                .ForMember(d => d.RoomNumber, o => o.MapFrom(s => s.Room != null ? s.Room.RoomNumber : string.Empty));

            CreateMap<HotelBilling.Application.DTOs.Bill.BillDto, Bill>()
                .ForMember(d => d.Id, o => o.Ignore());

            // Customer mappings (needed by views/controllers)
            CreateMap<Customer, HotelBilling.Application.DTOs.Customer.CustomerDto>()
                .ForMember(d => d.FullName, o => o.MapFrom(s => (s.FirstName + " " + s.LastName).Trim()));

            CreateMap<HotelBilling.Application.DTOs.Customer.CustomerDto, Customer>()
                .ForMember(d => d.Id, o => o.Ignore());
        }

        private static int? ConvertFloorToNullableInt(string? floor)
        {
            if (string.IsNullOrWhiteSpace(floor))
                return null;

            if (int.TryParse(floor, out var v))
                return v;

            return null;
        }
    }
}
