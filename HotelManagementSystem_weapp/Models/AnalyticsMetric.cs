namespace HotelManagementSystem_weapp.Models
{
    public class AnalyticsMetric
    {
        public DateTime Date { get; set; }
        public int ForecastedOccupancy { get; set; }
        public int ScheduledStaffCount { get; set; }
        public double TotalStaffHours { get; set; }
        public decimal EstimatedLaborCost { get; set; }

        // Derived Business Logic
        public double EfficiencyRatio => ForecastedOccupancy > 0 ? TotalStaffHours / ForecastedOccupancy : 0;
        public string Status => EfficiencyRatio > 1.5 ? "Overstaffed" : "Optimized";
        public string StatusClass => EfficiencyRatio > 1.5 ? "badge bg-danger-subtle text-danger" : "badge bg-success-subtle text-success";
    }
}
