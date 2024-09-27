using GorevTakipProgrami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GorevTakipProgrami.Controllers;

public class UserController : Controller
{
    private readonly AppDbContext _context;

    public UserController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var TaskCount = 3;
        var tasks = await _context.Tasks
            .Where(t => t.AssignedToUser.Username == HttpContext.Session.GetString("Username"))
            .Include(t => t.AssignedByUser)
            .OrderByDescending(x => x.CreationDate)
            .Take(TaskCount)
            .ToListAsync();
        return View(tasks);
    }

    public IActionResult Tasks()
    {
        var userName = HttpContext.Session.GetString("Username");
        
        var user = _context.Users.FirstOrDefault(u => u.Username == userName);
        if (user == null)
        {
            return NotFound();
        }
        
        var tasks = _context.Tasks
            .Where(t => t.AssignedToUserId == user.UserId)
            .Include(t => t.AssignedByUser)
            .ToList();
        return View(tasks);
    }
    
    public async Task<IActionResult> TaskDetail(int taskId)
    {
        var userName = HttpContext.Session.GetString("Username");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
        if (user == null)
        {
            return NotFound();
        }

        var task = await _context.Tasks
            .Include(t => t.AssignedToUser)
            .Include(t => t.Comments)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => t.TaskId == taskId && t.AssignedToUserId == user.UserId);

        if (task == null)
        {
            return NotFound();
        }

        return View(task);
    }

    public async Task<IActionResult> TaskHistory(int taskId)
    {
        var userName = HttpContext.Session.GetString("Username");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
        if (user == null)
        {
            return NotFound();
        }

        var taskHistory = await _context.TaskHistories
            .Where(th => th.TaskId == taskId && th.Task.AssignedToUserId == user.UserId)
            .Include(th => th.ChangedByUser)
            .ToListAsync();

        return View(taskHistory);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateTaskStatus(int taskId, string taskStatus)
    {
        var userName = HttpContext.Session.GetString("Username");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
        if (user == null)
        {
            return NotFound();
        }

        var task = await _context.Tasks.FirstOrDefaultAsync(t => t.TaskId == taskId && t.AssignedToUserId == user.UserId);
        if (task == null)
        {
            return NotFound();
        }

        task.TaskStatus = taskStatus;

        _context.Tasks.Update(task);
        await _context.SaveChangesAsync();

        ViewBag.SuccessMessage = "Görev durumu başarıyla güncellendi.";
        return RedirectToAction("AssignedTasks");
    }
    
    public async Task<IActionResult> UnfinishedTasks()
    {
        var userName = HttpContext.Session.GetString("Username");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
        if (user == null)
        {
            return NotFound();
        }

        var tasks = await _context.Tasks
            .Where(t => t.TaskStatus == "Baslanmadi" && t.AssignedToUserId == user.UserId)
            .Include(t => t.AssignedByUser)
            .ToListAsync();

        return View(tasks);
    }

    public async Task<IActionResult> FinishedTasks()
    {
        var userName = HttpContext.Session.GetString("Username");
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
        if (user == null)
        {
            return NotFound();
        }

        var tasks = await _context.Tasks
            .Where(t => t.TaskStatus == "Bitti" && t.AssignedToUserId == user.UserId)
            .Include(t => t.AssignedByUser)
            .ToListAsync();

        return View(tasks);
    }

    public IActionResult Profile()
    {
        var userName = HttpContext.Session.GetString("Username");
        var user = _context.Users.FirstOrDefault(u => u.Username == userName);
        return View(user);
    }
}