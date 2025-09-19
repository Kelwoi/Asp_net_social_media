using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PimpleNet.Data.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Fullname { get; set; }
        
        public string? profilePicture { get; set; }

        //nav
        public ICollection<Post> Posts { get; set; } = new List<Post>();
    }
}
