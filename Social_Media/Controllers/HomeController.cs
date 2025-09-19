using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
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

        public async Task<IActionResult> Index()
        {
            var allPosts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Likes)
                .Include(p => p.Favorites)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(allPosts);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(PostVM post)
        {
            int loggedInUser = 1;

            var newPost = new Post()
            {
                PostContent = post.Content,
                ImageUrl = "",
                NrOfReports = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                UserId = loggedInUser
            };

            if(post.Image != null && post.Image.Length > 0)
            {
                string RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (post.Image.ContentType.Contains("image"))
                {
                    string RootImagesFolder = Path.Combine(RootFolder, "images/uploaded");
                    Directory.CreateDirectory(RootImagesFolder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(post.Image.FileName);
                    string fullPath = Path.Combine(RootImagesFolder, fileName);

                    using(var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await post.Image.CopyToAsync(stream);
                    }

                    newPost.ImageUrl = "/images/uploaded" + fileName;
                }
            }
            await _context.Posts.AddAsync(newPost);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]

        public async Task<IActionResult> ToggleLike(PostLikeVM postLikeVM)
        {
            int loggedInUserId = 1;

            var like = await _context.Likes
                .Where(l => l.PostId == postLikeVM.PostId && l.UserId == loggedInUserId)
                .FirstOrDefaultAsync();

            if (like != null)
            {
                _context.Likes.Remove(like);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newLike = new Like()
                {
                    PostId = postLikeVM.PostId,
                    UserId = loggedInUserId
                };
                await _context.Likes.AddAsync(newLike);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(PostCommentVM commentVM)
        {
            int loggedInUserId = 1;
            var newComment = new Comment()
            {
                CreatedAt = DateTime.UtcNow,
                Content = commentVM.Content,
                UserId = loggedInUserId,
                PostId = commentVM.PostId
            };
            await _context.Comments.AddAsync(newComment);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveComment(RemoveCommentVM removeCommentVM)
        {
            var commentDb = await _context.Comments.FirstOrDefaultAsync(c => c.Id == removeCommentVM.CommentId);
            if(commentDb != null)
            {
                _context.Comments.Remove(commentDb);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");

        }

        [HttpPost]

        public async Task<IActionResult> ToggleFavorite(PostFavoriteVM postFavoriteVM)
        {
            int loggedInUserId = 1;

            var favorite = await _context.Favorites
                .Where(l => l.PostId == postFavoriteVM.PostId && l.UserId == loggedInUserId)
                .FirstOrDefaultAsync();

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
            else
            {
                var newFavorite = new Favorite()
                {
                    PostId = postFavoriteVM.PostId,
                    UserId = loggedInUserId
                };
                await _context.Favorites.AddAsync(newFavorite);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> TogglePrivacy(PostVisibilityVM postVisibilityVM)
        {
            int loggedInUserId = 1;

            var post = await _context.Posts
                .FirstOrDefaultAsync(l => l.Id == postVisibilityVM.PostId && l.UserId == loggedInUserId);

            if (post != null)
            {
                post.IsPrivate = !post.IsPrivate;
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }



    }
}
