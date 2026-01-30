using HotelManagementSystem.API_.Data;
using HotelManagementSystem.API_.Models.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.API_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/dashboard/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var nextWeekStart = DateTime.Today;
            var nextWeekEnd = DateTime.Today.AddDays(7);

            // 1. Get Forecast Data for the Chart
            var forecasts = await _context.Forecasts
                .Where(f => f.ForecastDate >= nextWeekStart && f.ForecastDate < nextWeekEnd)
                .OrderBy(f => f.ForecastDate)
                .ToListAsync();

            var chartPoints = forecasts.Select(f => new ChartPoint
            {
                DateLabel = f.ForecastDate.ToString("MMM dd"),
                Value = f.OccupancyCount
            }).ToList();

            // 2. Calculate Estimated Labor Cost
            // Logic: Find all shifts in this range, multiply hours * rate
            var shifts = await _context.Shifts
                .Where(s => s.ShiftDate >= nextWeekStart && s.ShiftDate < nextWeekEnd)
                .ToListAsync();

            // For accuracy, we need the Staff Hourly Rate. 
            // This is a simplified calculation (assuming avg $15/hr if not joined)
            // In a real app, you would .Include(s => s.Staff)
            decimal estimatedCost = 0;
            foreach (var shift in shifts)
            {
                double hours = (shift.EndTime - shift.StartTime).TotalHours;
                estimatedCost += (decimal)hours * 15.00m; // Using $15 base rate
            }

            var dto = new DashboardDto
            {
                TotalLaborCost = estimatedCost,
                TotalShifts = shifts.Count,
                ForecastData = chartPoints
            };

            return Ok(dto);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetPredictionHistory()
        {
            var history = await _context.Forecasts
                .OrderByDescending(f => f.ForecastDate) // Newest first
                .Select(f => new
                {
                    ForecastDate = f.ForecastDate,
                    OccupancyCount = f.OccupancyCount,
                    // Add simple logic to categorize demand
                    Status = f.OccupancyCount > 120 ? "High Demand" : (f.OccupancyCount > 50 ? "Normal" : "Low Demand")
                })
                .ToListAsync();

            return Ok(history);
        }
    }
}
