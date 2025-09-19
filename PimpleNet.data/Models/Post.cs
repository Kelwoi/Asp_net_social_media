using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace PimpleNet.Data.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        public string PostContent { get; set; }

        public string? ImageUrl { get; set; }

        public int NrOfReports { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // Foreign Key
        public int UserId { get; set; }

        // Navigation property
        public User User { get; set; }


    }
}
