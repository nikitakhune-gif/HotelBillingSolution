using HotelBilling.Application.DTOs.Room;

namespace HotelBilling.Application.Interfaces
{
    public interface IRoomService
    {
        Task<IEnumerable<RoomDto>> GetAllAsync();

        Task<RoomDto?> GetByIdAsync(int id);

        Task<int> CreateAsync(CreateRoomDto request);

        Task<bool> UpdateAsync(int id, UpdateRoomDto request);

        Task<bool> DeleteAsync(int id);

        // Keep parity with other services - returns dynamic for views
        Task<dynamic> GetAllForViewAsync();

        // check existence by room number
        Task<bool> ExistsByRoomNumberAsync(string roomNumber);
    }
}
