using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PimpleNet.Data.Models
{
    public class Friendship
    {
        public int Id { get; set; }

        public int UserId { get; set; }      // Хто ініціював дружбу
        public User User { get; set; }

        public int FriendId { get; set; }    // З ким дружить
        public User Friend { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
