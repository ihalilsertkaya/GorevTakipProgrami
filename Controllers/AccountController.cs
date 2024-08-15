using GorevTakipProgrami.Data;
using Microsoft.AspNetCore.Mvc;

namespace GorevTakipProgrami.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }
    
    
    public IActionResult Login()
    {
        return View();
    }
    
    [HttpPost]
    public IActionResult Login(String Username, String Password)
    {
        var User = _context.Users.FirstOrDefault(x => x.Username == Username && x.PasswordHash == Password);
        if (User == null)
        {
            ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya parola.");
            return View();
        }
        HttpContext.Session.SetString("Username",User.Username);
        HttpContext.Session.SetString("Role",User.Role);


        switch (HttpContext.Session.GetString("Role"))
        {
            case "User":
                return RedirectToAction("Index", "Home");
            case "Admin":
                return RedirectToAction("Index", "Home");
            default:
                ModelState.AddModelError(string.Empty, "Geçersiz yetki.");
                return RedirectToAction("Login");
            
        }

    }
}
