using Google.OrTools.Sat;
using HotelManagementSystem.API_.Data;
using HotelManagementSystem.API_.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.API_.Services
{
    public class OptimizationService
    {
        private readonly ApplicationDbContext _context;

        public OptimizationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateRosterAsync(DateTime startDate, int days)
        {
           //startDate = new DateTime(2018, 4, 4, 16, 0, 0);
            // 1. Fetch Data
            var staffList = await _context.Staff.Where(s => s.IsActive).ToListAsync();
            var forecasts = await _context.Forecasts
                .Where(f => f.ForecastDate >= startDate && f.ForecastDate < startDate.AddDays(days))
                .ToListAsync();

            if (!staffList.Any() || !forecasts.Any())
                return "Error: No staff or forecasts found.";

            // 2. Initialize Solver
            CpModel model = new CpModel();

            // Dictionary to hold variables: [StaffID, DayIndex] -> Boolean (Working or Not)
            Dictionary<(int, int), BoolVar> shifts = new Dictionary<(int, int), BoolVar>();

            // 3. Create Variables
            for (int d = 0; d < days; d++)
            {
                foreach (var employee in staffList)
                {
                    shifts.Add((employee.StaffID, d), model.NewBoolVar($"emp{employee.StaffID}_day{d}"));
                }
            }

            // 4. HARD CONSTRAINT: Demand Satisfaction
            // For each day, ensure we have enough staff.
            // Rule: 1 Staff Member handles approx 15 guests (Simplified rule for demo)
            for (int d = 0; d < days; d++)
            {
                var currentDate = startDate.AddDays(d);
                var forecastForDay = forecasts.FirstOrDefault(f => f.ForecastDate.Date == currentDate.Date);

                if (forecastForDay != null)
                {
                    //int staffNeeded = (int)Math.Ceiling(forecastForDay.OccupancyCount / 15.0);
                    int staffNeeded = (int)Math.Ceiling(forecastForDay.OccupancyCount / 30.0);

                    var staffWorkingToday = new List<BoolVar>();
                    foreach (var employee in staffList)
                    {
                        staffWorkingToday.Add(shifts[(employee.StaffID, d)]);
                    }

                    // Sum of staff working today >= Staff Needed
                    model.Add(LinearExpr.Sum(staffWorkingToday) >= staffNeeded);
                }
            }

            // 5. HARD CONSTRAINT: Max 5 shifts per week per person
            foreach (var employee in staffList)
            {
                var employeeShifts = new List<BoolVar>();
                for (int d = 0; d < days; d++)
                {
                    employeeShifts.Add(shifts[(employee.StaffID, d)]);
                }
                model.Add(LinearExpr.Sum(employeeShifts) <= 5);
            }

            // 6. Solve
            CpSolver solver = new CpSolver();
            CpSolverStatus status = solver.Solve(model);

            if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
            {
                // 7. Save Results to DB
                // First, remove old shifts for this period to avoid duplicates
                // (In production, be careful with this!)

                int shiftsCreated = 0;
                for (int d = 0; d < days; d++)
                {
                    foreach (var employee in staffList)
                    {
                        if (solver.Value(shifts[(employee.StaffID, d)]) == 1)
                        {
                            var newShift = new Shift // Assuming you have a Shift model
                            {
                                StaffID = employee.StaffID,
                                ShiftDate = startDate.AddDays(d),
                                ShiftType = "Day", // Simplified
                                StartTime = new TimeSpan(9, 0, 0),
                                EndTime = new TimeSpan(17, 0, 0)
                            };
                            _context.Shifts.Add(newShift);
                            shiftsCreated++;
                        }
                    }
                }
                await _context.SaveChangesAsync();
                return $"Success! Generated {shiftsCreated} shifts.";
            }

            return "No solution found. You might be understaffed.";
        }
    }
}
