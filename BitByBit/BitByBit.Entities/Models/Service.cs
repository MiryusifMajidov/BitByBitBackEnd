using BitByBit.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Entities.Models
{
    public class Service:BaseEntity
    {
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public RoomType Role { get; set; } = RoomType.Standart;

    }
}
