using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HotelManagementSystem_weapp.Models
{
    [Table("Forecasts")] // This forces EF to use the singular name from your DB
    public class Forecast
    {
        [Key]
        public int ForecastId { get; set; }
        public DateTime ForecastDate { get; set; }
        public int OccupancyCount { get; set; }
    }
}
