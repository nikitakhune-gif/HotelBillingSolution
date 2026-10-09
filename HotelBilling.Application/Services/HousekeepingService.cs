using HotelBilling.Application.Interfaces;
using HotelBillingSolution.Application.DTOs.Housekeeping;
using HotelBillingSolution.Models;
using HotelBillingSolution.Domain.Entities;
using HotelBilling.Domain.Interfaces;

namespace HotelBilling.Application.Services
{
    public class HousekeepingService : IHousekeepingService
    {
        private readonly IRepository<HousekeepingTask> _repo;
        private readonly IUnitOfWork _unitOfWork;

        public HousekeepingService(IRepository<HousekeepingTask> repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> CreateTaskAsync(HousekeepingTaskDto request)
        {
            var task = new HousekeepingTask
            {
                RoomId = request.RoomId,
                GuestName = request.GuestName,
                TaskDate = request.TaskDate,
                DueDate = request.DueDate,
                TaskType = request.TaskType,
                Priority = request.Priority,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Pending" : request.Status,
                Floor = request.Floor.HasValue ? request.Floor.Value.ToString() : null,
                AssignedToStaffId = request.AssignedTo,
                Description = request.Description,
                Instructions = request.Instructions,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            return task.HousekeepingTaskId;
        }

        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _repo.GetByIdAsync(id);
            if (task == null) return false;

            _repo.Delete(task);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<HousekeepingTaskDto>> GetAllTasksAsync()
        {
            var tasks = await _repo.GetAllAsync();

            return tasks.Select(t => new HousekeepingTaskDto
            {
                HousekeepingTaskId = t.HousekeepingTaskId,
                RoomId = t.RoomId,
                GuestName = t.GuestName,
                TaskDate = t.TaskDate,
                DueDate = t.DueDate,
                TaskType = t.TaskType,
                Priority = t.Priority,
                Status = t.Status,
                Floor = int.TryParse(t.Floor, out var _f) ? _f : 0,
                AssignedTo = t.AssignedToStaffId,
                Description = t.Description,
                Instructions = t.Instructions,
                Notes = t.Notes
            });
        }

        public async Task<HousekeepingTaskDto?> GetTaskByIdAsync(int id)
        {
            var t = await _repo.GetByIdAsync(id);
            if (t == null) return null;

            return new HousekeepingTaskDto
            {
                HousekeepingTaskId = t.HousekeepingTaskId,
                RoomId = t.RoomId,
                GuestName = t.GuestName,
                TaskDate = t.TaskDate,
                DueDate = t.DueDate,
                TaskType = t.TaskType,
                Priority = t.Priority,
                Status = t.Status,
                Floor = int.TryParse(t.Floor, out var _ff) ? _ff : (int?)null,
                AssignedTo = t.AssignedToStaffId,
                Description = t.Description,
                Instructions = t.Instructions,
                Notes = t.Notes
            };
        }

        public async Task<bool> UpdateTaskAsync(int id, HousekeepingTaskDto request)
        {
            var task = await _repo.GetByIdAsync(id);
            if (task == null) return false;

            task.RoomId = request.RoomId;
            task.GuestName = request.GuestName;
            task.TaskDate = request.TaskDate;
            task.DueDate = request.DueDate;
            task.TaskType = request.TaskType;
            task.Priority = request.Priority;
            task.Status = request.Status ?? task.Status;
            task.Floor = request.Floor.HasValue ? request.Floor.Value.ToString() : task.Floor;
            task.AssignedToStaffId = request.AssignedTo;
            task.Description = request.Description;
            task.Instructions = request.Instructions;
            task.Notes = request.Notes;

            _repo.Update(task);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<dynamic> GetAllForViewAsync()
        {
            var tasks = await GetAllTasksAsync();
            return tasks;
        }
    }
}
