using Microsoft.EntityFrameworkCore;
using PimpleNet.Data;
using PimpleNet.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PimpleNet.Data.Helpers
{
    public static class DbInitializer
    {
        public static async Task  SeedAsync(AppDb appDbContext) 
        {
            if (!appDbContext.Users.Any() && !appDbContext.Posts.Any())
            {
                var newUs = new User()
                {
                    Fullname = "Test User",
                    profilePicture = "data:image/webp;base64,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",
                };
                await appDbContext.Users.AddAsync(newUs);
                await appDbContext.SaveChangesAsync();
                var newPostWOPicture = new Post()
                {
                    PostContent = "This is a test post without a picture for test using.",
                    ImageUrl = "",
                    NrOfReports = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UserId = newUs.Id
                };

                var newPWPicture = new Post()
                {
                    PostContent = "This is a test post with a picture for test using.",
                    ImageUrl = "https://th.bing.com/th/id/OIP.305oQm--foUcvds3lVIz7QHaHa?w=176&h=180&c=7&r=0&o=7&pid=1.7&rm=3",
                    NrOfReports = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    UserId = newUs.Id
                };

                await appDbContext.Posts.AddRangeAsync(newPostWOPicture, newPWPicture);
                await appDbContext.SaveChangesAsync();
            }


        }
    }
}
