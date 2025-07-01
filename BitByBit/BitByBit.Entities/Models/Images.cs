using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Entities.Models
{
    public class Images : BaseEntity
    {
        public string ImageUrl { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public int DisplayOrder { get; set; } = 1;
        public bool IsMain { get; set; } = false;

        public int RoomId { get; set; }

        public Room Room { get; set; }
    }
}