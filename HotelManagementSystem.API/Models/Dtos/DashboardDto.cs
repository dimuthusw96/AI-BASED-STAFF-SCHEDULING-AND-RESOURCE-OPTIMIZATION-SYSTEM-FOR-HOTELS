namespace HotelManagementSystem.API_.Models.Dtos
{
    public class DashboardDto
    {
        public decimal TotalLaborCost { get; set; }
        public int TotalShifts { get; set; }
        public List<ChartPoint> ForecastData { get; set; } = new List<ChartPoint>();
    }
    public class ChartPoint
    {
        public string DateLabel { get; set; } // e.g., "Nov 29"
        public int Value { get; set; }        // e.g., 120 guests
    }
}

