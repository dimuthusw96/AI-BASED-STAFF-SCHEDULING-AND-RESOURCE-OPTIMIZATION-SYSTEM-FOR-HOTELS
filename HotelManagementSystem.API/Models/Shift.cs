using System.ComponentModel.DataAnnotations;

namespace HotelManagementSystem.API_.Models
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
    }
    public class ShiftUpdateRequest
    {
        public int ShiftID { get; set; }
        public DateTime NewDate { get; set; }
        public string NewStaffName { get; set; }
    }
}

