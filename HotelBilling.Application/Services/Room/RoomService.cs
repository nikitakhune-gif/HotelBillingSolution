using HotelBilling.Application.DTOs.Room;
using HotelBilling.Application.Interfaces;
using HotelBilling.Domain.Entities;
using HotelBilling.Domain.Interfaces;

namespace HotelBilling.Application.Services
{
    public class RoomService : IRoomService
    {
        private readonly IRepository<Room> _repo;
        private readonly IUnitOfWork _unitOfWork;

        public RoomService(IRepository<Room> repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> ExistsByRoomNumberAsync(string roomNumber)
        {
            if (string.IsNullOrWhiteSpace(roomNumber)) return false;
            return await _repo.AnyAsync(r => r.RoomNumber == roomNumber);
        }

        public async Task<int> CreateAsync(CreateRoomDto request)
        {
            var room = new Room
            {
                RoomNumber = request.RoomNumber,
                RoomType = request.RoomType,
                RoomName = request.RoomName,
                Floor = request.Floor,
                PricePerNight = request.PricePerNight,
                WeekendPrice = request.WeekendPrice,
                ExtraPersonCharge = request.ExtraPersonCharge,
                Discount = request.Discount,
                Tax = request.Tax,
                Availability = request.Availability ?? "Available",
                IsActive = request.IsActive,
                BedType = request.BedType ?? string.Empty,
                RoomSize = request.RoomSize,
                CapacityAdults = request.CapacityAdults,
                CapacityChildren = request.CapacityChildren,
                HousekeepingStatus = request.HousekeepingStatus,
                ImagePaths = request.ImagePaths,
                RoomDescription = request.RoomDescription,
                SpecialNotes = request.SpecialNotes
            };

            await _repo.AddAsync(room);
            await _unitOfWork.SaveChangesAsync();

            return room.Id;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var room = await _repo.GetByIdAsync(id);
            if (room == null) return false;

            _repo.Delete(room);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<RoomDto>> GetAllAsync()
        {
            var rooms = await _repo.GetAllAsync();

            return rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                RoomName = r.RoomName,
                RoomType = r.RoomType,
                Floor = r.Floor,
                PricePerNight = r.PricePerNight,
                WeekendPrice = r.WeekendPrice,
                ExtraPersonCharge = r.ExtraPersonCharge,
                Discount = r.Discount,
                Tax = r.Tax,
                Availability = r.Availability,
                IsActive = r.IsActive,
                CapacityAdults = r.CapacityAdults,
                CapacityChildren = r.CapacityChildren,
                BedType = r.BedType,
                CreatedDate = r.CreatedDate,
                HousekeepingStatus = r.HousekeepingStatus,
                ImagePaths = r.ImagePaths
            });
        }

        public async Task<RoomDto?> GetByIdAsync(int id)
        {
            var r = await _repo.GetByIdAsync(id);
            if (r == null) return null;

            return new RoomDto
            {
                Id = r.Id,
                RoomNumber = r.RoomNumber,
                RoomName = r.RoomName,
                RoomType = r.RoomType,
                Floor = r.Floor,
                PricePerNight = r.PricePerNight,
                WeekendPrice = r.WeekendPrice,
                ExtraPersonCharge = r.ExtraPersonCharge,
                Discount = r.Discount,
                Tax = r.Tax,
                Availability = r.Availability,
                IsActive = r.IsActive,
                CapacityAdults = r.CapacityAdults,
                CapacityChildren = r.CapacityChildren,
                BedType = r.BedType,
                CreatedDate = r.CreatedDate,
                HousekeepingStatus = r.HousekeepingStatus,
                ImagePaths = r.ImagePaths,
                RoomDescription = r.RoomDescription,
                SpecialNotes = r.SpecialNotes
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdateRoomDto request)
        {
            var room = await _repo.GetByIdAsync(id);
            if (room == null) return false;


            // map updatable fields
            room.RoomNumber = request.RoomNumber;
            room.RoomType = request.RoomType;
            room.RoomName = request.RoomName;
            room.Floor = request.Floor;
            room.PricePerNight = request.PricePerNight;
            room.WeekendPrice = request.WeekendPrice;
            room.ExtraPersonCharge = request.ExtraPersonCharge;
            room.Discount = request.Discount;
            room.Tax = request.Tax;
            room.Availability = request.Availability ?? room.Availability;
            room.IsActive = request.IsActive;
            room.BedType = request.BedType ?? room.BedType;
            room.RoomSize = request.RoomSize;
            room.CapacityAdults = request.CapacityAdults;
            room.CapacityChildren = request.CapacityChildren;
            room.HousekeepingStatus = request.HousekeepingStatus;
            room.ImagePaths = request.ImagePaths ?? room.ImagePaths;

            _repo.Update(room);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<dynamic> GetAllForViewAsync()
        {
            var rooms = await GetAllAsync();
            return rooms;
        }
    }
}
