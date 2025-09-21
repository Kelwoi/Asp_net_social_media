using Microsoft.AspNetCore.Mvc;

namespace Social_Media.ViewModels.Home
{
    public class AddFriendVM
    {
        public int UserId { get; set; }
        public int FriendId { get; set; }
    }
}
