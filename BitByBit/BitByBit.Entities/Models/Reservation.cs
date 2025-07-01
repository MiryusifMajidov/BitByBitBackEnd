using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Entities.Models
{
    public class Reservation : BaseEntity
    {
        // Foreign Keys
        public int RoomId { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Reservation Dates
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalNights { get; set; }

        // Navigation Properties
        public Room Room { get; set; }
        public User User { get; set; }
    }
}