using HotelManagementSystem_weapp.Models;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem_weapp.Models
{
    public class Shift
    {
        [Key]
        public int ShiftID { get; set; }
        public int StaffID { get; set; }
        public DateTime ShiftDate { get; set; }
        public string ShiftType { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        // Add this navigation property if each Shift is assigned to a single Staff member
        public int StaffId { get; set; }
        public Staff Staff { get; set; }
    }
}

