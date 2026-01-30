using HotelManagementSystem.API_.Data;
using HotelManagementSystem.API_.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.API_.Models;

namespace HotelManagementSystem.API_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly OptimizationService _optimizer;
        private readonly ApplicationDbContext _context;

        public ScheduleController(ApplicationDbContext context, OptimizationService optimizer)
        {
            _optimizer = optimizer;
            _context = context;
        }

        // POST api/schedule/generate
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateSchedule()
        {
            // We'll generate a schedule for the next 7 days starting tomorrow
            var result = await _optimizer.GenerateRosterAsync(DateTime.Today.AddDays(1), 7);

            if (result.Contains("Success"))
            {
                return Ok(result);
            }
            return BadRequest(result);
        }


        // URL: api/schedule/list?start=2025-11-30&days=7
        [HttpGet("list")]
        public IActionResult GetShifts(DateTime start, int days = 30)
        {
            // We join 'Shifts' with 'Staff' to get names like "John Smith" instead of "ID: 1"
            var shifts = _context.Shifts
                .Where(s => s.ShiftDate >= start && s.ShiftDate < start.AddDays(days))
                .Join(_context.Staff,
                    shift => shift.StaffID,
                    staff => staff.StaffID,
                    (shift, staff) => new
                    {
                        shift.ShiftID,
                        shift.ShiftDate,
                        shift.StartTime,
                        shift.EndTime,
                        StaffName = staff.FullName, // Get the real name
                        Role = staff.Role
                    })
                .OrderBy(s => s.ShiftDate)
                .ToList();

            return Ok(shifts);
        }

        // 3. PUT: Update a shift after Drag & Drop
        [HttpPut("update")]
        public async Task<IActionResult> UpdateShift([FromBody] ShiftUpdateRequest request)
        {
            var shift = await _context.Shifts.FindAsync(request.ShiftID);
            if (shift == null) return NotFound("Shift not found");

            // Update the database with new details
            shift.ShiftDate = request.NewDate;

            // Find the Staff ID based on the name (since our UI uses names)
            // In a real app, Drag & Drop should pass IDs, but this works for your current setup
            var staff = await _context.Staff.FirstOrDefaultAsync(s => s.FullName == request.NewStaffName);
            if (staff != null)
            {
                shift.StaffID = staff.StaffID;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Shift moved successfully" });
        }

        //// GET: api/schedule/staff/John
        //[HttpGet("staff/{name}")]
        //public IActionResult GetMyShifts(string name)
        //{
        //    // Fix: Use 'Join' instead of 'Include' because the Model lacks the navigation property
        //    var myShifts = _context.Shifts
        //        .Where(s => s.ShiftDate >= DateTime.Today) // Filter Future Shifts
        //        .Join(_context.Staff,
        //            shift => shift.StaffID,
        //            staff => staff.StaffID,
        //            (shift, staff) => new { Shift = shift, Staff = staff }) // Combine them temporarily
        //        .Where(combined => combined.Staff.FullName.Contains(name)) // Filter by Name
        //        .OrderBy(combined => combined.Shift.ShiftDate)
        //        .Select(combined => new
        //        {
        //            ShiftDate = combined.Shift.ShiftDate,
        //            StartTime = combined.Shift.StartTime,
        //            EndTime = combined.Shift.EndTime,
        //            Role = combined.Staff.Role,
        //            StaffName = combined.Staff.FullName
        //        })
        //        .ToList();

        //    return Ok(myShifts);
        //}



        // 1. NEW ENDPOINT: Get list of staff names for the dropdown
        [HttpGet("staff-list")]
        public IActionResult GetStaffNames()
        {
            var names = _context.Staff
                .Where(s => s.IsActive)
                .Select(s => s.FullName)
                .Distinct()
                .OrderBy(n => n)
                .ToList();
            return Ok(names);
        }

        // 2. UPDATED ENDPOINT: Search with Date Range
        // URL: api/schedule/staff/John?start=2025-01-01&end=2025-01-31
        [HttpGet("staff/{name}")]
        public IActionResult GetMyShifts(string name, [FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            // Default to "Today" and "30 Days from now" if no dates provided
            var startDate = start ?? DateTime.Today;
            var endDate = end ?? DateTime.Today.AddDays(30);

            var myShifts = _context.Shifts
                .Where(s => s.ShiftDate >= startDate && s.ShiftDate <= endDate) // <--- Date Filter
                .Join(_context.Staff,
                    shift => shift.StaffID,
                    staff => staff.StaffID,
                    (shift, staff) => new { Shift = shift, Staff = staff })
                .Where(combined => combined.Staff.FullName == name) // <--- Exact Match for Dropdown
                .OrderBy(combined => combined.Shift.ShiftDate)
                .Select(combined => new
                {
                    ShiftDate = combined.Shift.ShiftDate,
                    StartTime = combined.Shift.StartTime,
                    EndTime = combined.Shift.EndTime,
                    Role = combined.Staff.Role,
                    StaffName = combined.Staff.FullName
                })
                .ToList();

            return Ok(myShifts);
        }
    }
}