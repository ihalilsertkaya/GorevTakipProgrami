using GorevTakipProgrami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace GorevTakipProgrami.Controllers;

public class BaseController : Controller
{
    protected readonly AppDbContext _context;
    protected readonly ILogger<BaseController> _logger;

    public BaseController(AppDbContext context, ILogger<BaseController> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        var userName = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(userName))
        {
            filterContext.Result = RedirectToAction("Login", "Account");
            return;
        }
        var loggedUser = _context.Users.Where(u => u.Username == userName).FirstOrDefault();
        ViewBag.UserFullName = loggedUser?.FullName;
        ViewBag.UserPhoto = loggedUser?.ProfilePhoto;
        ViewBag.Users = _context.Users.ToList();
        base.OnActionExecuting(filterContext);
    }
}