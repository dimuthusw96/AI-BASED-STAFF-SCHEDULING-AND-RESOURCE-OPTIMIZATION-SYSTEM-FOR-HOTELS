using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.API_.Models
{
    public class Forecast
    {
        [Key]
        public int ForecastId { get; set; }
        public DateTime ForecastDate { get; set; }
        public int OccupancyCount { get; set; }
    }
}
