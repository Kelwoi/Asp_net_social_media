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

        public string password { get; set; }
        public string Email { get; set; }

        public bool isAdmin { get; set; } = false;
        public string? profilePicture { get; set; }

        //nav
        public ICollection<Post> Posts { get; set; } = new List<Post>();

        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

        public ICollection<Friendship> Friendships { get; set; } = new List<Friendship>();

    }
}
