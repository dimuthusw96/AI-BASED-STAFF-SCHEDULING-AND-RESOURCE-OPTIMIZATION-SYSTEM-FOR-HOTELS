using System.Text.Json.Serialization; 

namespace HotelManagementSystem.Web.Models
{
    public class DashboardDto
    {
        [JsonPropertyName("totalLaborCost")]
        public decimal TotalLaborCost { get; set; }

        [JsonPropertyName("totalShifts")]
        public int TotalShifts { get; set; }

        [JsonPropertyName("forecastData")]
        public List<ChartPoint> ForecastData { get; set; } = new List<ChartPoint>();
    }

    public class ChartPoint
    {
        // 1. The API sends "dateLabel", so we map it here
        [JsonPropertyName("dateLabel")]
        public string DateLabel { get; set; }

        // 2. The API sends "value", so we map it here
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }
}