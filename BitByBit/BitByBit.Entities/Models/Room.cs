using BitByBit.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Entities.Models
{
    public class Room : BaseEntity
    {
        public string RoomName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RoomType Role { get; set; } = RoomType.Standart;
        public int Capacity { get; set; } = 2;
        public int RoomCount { get; set; } = 1;
        public int BathCount { get; set; } = 1;
        public bool Wifi { get; set; } = false;
        public decimal Price { get; set; }

        // Navigation Property
        public ICollection<Images> Images { get; set; } = new List<Images>();
    }
}