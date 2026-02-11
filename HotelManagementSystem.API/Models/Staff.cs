using System.ComponentModel.DataAnnotations;
namespace HotelManagementSystem.API_.Models
{
    public class Staff
    {
        [Key]
        public int StaffID { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public decimal HourlyRate { get; set; }
        public int MaxHoursPerWeek { get; set; }
        public bool IsActive { get; set; }



        public string Username { get; set; }
        public string Password { get; set; }
    }
}

