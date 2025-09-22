using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PimpleNet.Data;
using PimpleNet.Data.Models;
using Social_Media.ViewModels;
using Social_Media.ViewModels.Home;

namespace Social_Media.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDb _context;

        public HomeController(ILogger<HomeController> logger, AppDb context)
        {
            _logger = logger;
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            ViewBag.CurrentUserId = HttpContext.Session.GetInt32("UserId");
            return HttpContext.Session.GetInt32("UserId");
        }

        private User? GetCurrrentUser()
        {

            var userId = HttpContext.Session.GetInt32("UserId");
            var currentUser = userId.HasValue ? _context.Users.FirstOrDefault(u => u.Id == userId.Value) : null;
            return currentUser;

        }

        public async Task<IActionResult> FavoritesL()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("RegisterLogin", "Account");

            var FavoritesList = await _context.Favorites
                .Include(f => f.User)
                .Include(f => f.Post)
                    .ThenInclude(p => p.User)
                .Include(f => f.Post)
                    .ThenInclude(p => p.Likes)
                .Include(f => f.Post)
                    .ThenInclude(p => p.Favorites)
                .Include(f => f.Post)
                    .ThenInclude(p => p.Comments)
                        .ThenInclude(c => c.User)
                .Where(f => f.UserId == userId.Value)
                .OrderByDescending(f => f.Post.CreatedAt)
                .ToListAsync();

            return View(FavoritesList);
        }

        public async Task<IActionResult> Friends()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("RegisterLogin", "Account");

            var friendsList = await _context.Friendships
                .Include(f => f.User)    // підвантажуємо User
                .Include(f => f.Friend)  // підвантажуємо Friend
                .Where(f => f.UserId == userId.Value || f.FriendId == userId.Value)
                .ToListAsync();

            return View(friendsList);
        }

        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("RegisterLogin", "Account");

            var allPosts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Favorites)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var allUsers = await _context.Users
                .Where(u => u.Id != userId.Value)
                .ToListAsync();

            ViewBag.allUsers = allUsers;

            return View(allPosts);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(PostVM post)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("RegisterLogin", "Account");

            var newPost = new Post()
            {
                PostContent = post.Content,
                ImageUrl = "",
                NrOfReports = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = userId.Value
            };

            if (post.Image != null && post.Image.Length > 0)
            {
                string RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (post.Image.ContentType.Contains("image"))
                {
                    string RootImagesFolder = Path.Combine(RootFolder, "images/uploaded");
                    Directory.CreateDirectory(RootImagesFolder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(post.Image.FileName);
                    string fullPath = Path.Combine(RootImagesFolder, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await post.Image.CopyToAsync(stream);
                    }

                    newPost.ImageUrl = "/images/uploaded/" + fileName;
                }
            }

            await _context.Posts.AddAsync(newPost);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLike(PostLikeVM postLikeVM)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("RegisterLogin", "Account");

            var like = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postLikeVM.PostId && l.UserId == userId.Value);

            if (like != null)
            {
                _context.Likes.Remove(like);
            }
            else
            {
                var newLike = new Like()
                {
                    PostId = postLikeVM.PostId,
                    UserId = userId.Value
                };
                await _context.Likes.AddAsync(newLike);
            }

            await _context.SaveChangesAsync();

            var likesCount = await _context.Likes.CountAsync(l => l.PostId == postLikeVM.PostId);
            var isLiked = await _context.Likes.AnyAsync(l => l.PostId == postLikeVM.PostId && l.UserId == userId.Value);

            return Json(new { likesCount, isLiked });
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(PostCommentVM commentVM)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("RegisterLogin", "Account");

            var newComment = new Comment()
            {
                CreatedAt = DateTime.UtcNow,
                Content = commentVM.Content,
                UserId = userId.Value,
                PostId = commentVM.PostId
            };

            await _context.Comments.AddAsync(newComment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveComment(RemoveCommentVM removeCommentVM)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("RegisterLogin", "Account");

            var commentDb = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == removeCommentVM.CommentId && c.UserId == userId.Value);

            if (commentDb != null)
            {
                _context.Comments.Remove(commentDb);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemovePost(PostRemoveVM removePostVM)
        {
            var user = GetCurrrentUser();
            if (user == null) return RedirectToAction("RegisterLogin", "Account");
            var postDb = await _context.Posts
                .FirstOrDefaultAsync(p =>
                    p.Id == removePostVM.PostId &&
                    (p.UserId == user.Id || user.isAdmin));
            if (postDb != null)
            {
                _context.Posts.Remove(postDb);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite(PostFavoriteVM postFavoriteVM)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("RegisterLogin", "Account");

            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(l => l.PostId == postFavoriteVM.PostId && l.UserId == userId.Value);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
            }
            else
            {
                var newFavorite = new Favorite
                {
                    PostId = postFavoriteVM.PostId,
                    UserId = userId.Value
                };
                await _context.Favorites.AddAsync(newFavorite);
            }

            await _context.SaveChangesAsync();

            var isFavorited = await _context.Favorites
                .AnyAsync(l => l.PostId == postFavoriteVM.PostId && l.UserId == userId.Value);
            var favoritesCount = await _context.Favorites
                .CountAsync(l => l.PostId == postFavoriteVM.PostId);

            return Json(new { isFavorited, favoritesCount });
        }

        public async Task<IActionResult> TogglePrivacy(PostVisibilityVM postVisibilityVM)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("RegisterLogin", "Account");

            var post = await _context.Posts
                .FirstOrDefaultAsync(l => l.Id == postVisibilityVM.PostId && l.UserId == userId.Value);

            if (post != null)
            {
                post.IsPrivate = !post.IsPrivate;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFriend(AddFriendVM addFriendVM)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("RegisterLogin", "Account");

            // Шукаємо дружбу в будь-якому напрямку
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f =>
                    (f.UserId == userId.Value && f.FriendId == addFriendVM.FriendId) ||
                    (f.UserId == addFriendVM.FriendId && f.FriendId == userId.Value));

            if (friendship != null)
            {
                // Видаляємо існуючу дружбу
                _context.Friendships.Remove(friendship);
            }
            else
            {
                // Створюємо нову дружбу
                var newFriendship = new Friendship
                {
                    UserId = userId.Value,
                    FriendId = addFriendVM.FriendId,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Friendships.AddAsync(newFriendship);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

    }
}
