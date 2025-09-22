using Microsoft.AspNetCore.Mvc;

namespace Social_Media.Controllers
{
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Authentication.Cookies;
    using Microsoft.AspNetCore.Mvc;
    using PimpleNet.Data;
    using PimpleNet.Data.Models;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    namespace Social_Media.Controllers
    {
        public class AccountController : Controller
        {
            private readonly AppDb _context;

            public AccountController(AppDb context)
            {
                _context = context;
            }

            [HttpGet]
            public IActionResult RegisterLogin()
            {
                return View();
            }

            [HttpPost]
            public async Task<IActionResult> RegisterLogin(string username, string password, string Email)
            {
                // Перевіряємо, чи існує користувач
                var user = _context.Users.FirstOrDefault(u => u.Fullname == username);

                if (user == null)
                {
                    // Створюємо нового
                    user = new User
                    {
                        Fullname = username,
                        Email = Email,
                        password = password // ⚠️ у реальному проекті обов'язково хешуй
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Якщо пароль не збігається → повертаємо помилку
                    if (user.password != password)
                    {
                        ViewBag.Error = "Невірний пароль";
                        return View();
                    }
                }

                // Зберігаємо userId у сесії
                HttpContext.Session.SetInt32("UserId", user.Id);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Fullname),
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
                
            }

            [HttpPost]
            public IActionResult Logout()
            {
                HttpContext.Session.Clear();
                return RedirectToAction("RegisterLogin");
            }
        }
    }

}
