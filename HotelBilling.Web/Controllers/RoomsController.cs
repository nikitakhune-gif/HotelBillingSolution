using Microsoft.AspNetCore.Mvc;
using HotelBilling.Application.Interfaces;
using HotelBilling.Application.DTOs.Room;

namespace HotelBilling.Web.Controllers
{
    public class RoomsController : Controller
    {
        private readonly IRoomService _roomService;

        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        // GET: Rooms list view
        public async Task<IActionResult> Index()
        {
            var rooms = await _roomService.GetAllForViewAsync();
            return View(rooms);
        }

        // GET: returns JSON list for AJAX
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await _roomService.GetAllAsync();
            return Json(rooms);
        }

        // GET: Single room
        public async Task<IActionResult> GetRoom(int id)
        {
            var room = await _roomService.GetByIdAsync(id);
            if (room == null) return NotFound();
            return Json(room);
        }

        // GET: Create page
        public IActionResult AddRoom()
        {
            return View();
        }

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> AddRoom(CreateRoomDto request)
        {
            if (!ModelState.IsValid)
                return View(request);

            await _roomService.CreateAsync(request);
            return RedirectToAction("Index");
        }

        // GET: Edit page
        public async Task<IActionResult> Edit(int id)
        {
            var room = await _roomService.GetByIdAsync(id);
            if (room == null) return NotFound();
            return View(room);
        }

        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit(int id, UpdateRoomDto request)
        {
            if (!ModelState.IsValid)
                return View(request);

            await _roomService.UpdateAsync(id, request);
            return RedirectToAction("Index");
        }

        // GET: Delete
        public async Task<IActionResult> Delete(int id)
        {
            await _roomService.DeleteAsync(id);
            return RedirectToAction("Index");
        }
    }
}
