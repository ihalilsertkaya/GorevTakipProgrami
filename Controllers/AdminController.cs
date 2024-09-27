using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using GorevTakipProgrami.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GorevTakipProgrami.Controllers;

public class AdminController : BaseController
{
    public AdminController(ILogger<AdminController> logger, AppDbContext context) : base(context, logger)
    {
    }

    public async Task<IActionResult> Index()
    {
        var TaskCount = 3;
        var tasks = await _context.Tasks
            .Where(t => t.AssignedByUser.Username == HttpContext.Session.GetString("Username"))
            .Include(t => t.AssignedToUser)
            .OrderByDescending(x => x.CreationDate)
            .Take(TaskCount)
            .ToListAsync();
        return View(tasks);
    }

    public async Task<IActionResult> TaskDetail(int taskId)
    {
        var task = await _context.Tasks
            .Include(t => t.AssignedToUser)
            .Include(t => t.Comments)
            .ThenInclude(c => c.User)
            .FirstOrDefaultAsync(t => t.TaskId == taskId);

        if (task == null)
        {
            return NotFound();
        }

        return View(task);
    }

    public async Task<IActionResult> AddComment(int taskId, string content)
    {
        var task = await _context.Tasks
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.TaskId == taskId);

        if (task == null)
        {
            return NotFound();
        }

        var comment = new Comment
        {
            TaskId = taskId,
            CommentText = content,
            CommentDate = DateTime.Now,
            UserId = _context.Users
                .FirstOrDefault(u => u.Username == HttpContext.Session.GetString("Username"))
                ?.UserId
        };

        task.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return RedirectToAction("TaskDetail", new { taskId = taskId });
    }

    public async Task<IActionResult> TaskHistory(int taskId)
    {
        var taskHistory = await _context.TaskHistories
            .Include(th => th.ChangedByUser)
            .Include(th => th.Task)
            .Where(th => th.TaskId == taskId)
            .OrderByDescending(th => th.ChangeDate)
            .ToListAsync();
        return View(taskHistory);
    }

    public async Task<IActionResult> Tasks()
    {
        var users = await _context.Users
            .Select(u => new SelectListItem
            {
                Value = u.UserId.ToString(),
                Text = u.Username
            }).ToListAsync();
        ViewBag.Users = users;
        var tasks = await _context.Tasks
            .Where(t => t.AssignedByUser.Username == HttpContext.Session.GetString("Username"))
            .Include(t => t.AssignedToUser)
            .ToListAsync();
        return View(tasks);
    }

    public async Task<IActionResult> Profile()
    {
        var username = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(username))
        {
            return RedirectToAction("Login");
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(User model, IFormFile? ProfilePhoto)
    {
        var user = await _context.Users.FindAsync(model.UserId);
        if (user == null)
        {
            return NotFound();
        }

        user.FullName = model.FullName;
        user.Email = model.Email;
        user.Password = model.Password;

        if (ProfilePhoto != null && ProfilePhoto.Length > 0)
        {
            // Dosya adını oluştur
            var fileName = Path.GetFileName(ProfilePhoto.FileName);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/assets/images/users", fileName);

            // Dosyayı kaydet
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await ProfilePhoto.CopyToAsync(stream);
            }

            // Kullanıcı modeline dosya yolunu ata
            user.ProfilePhoto = $"/assets/images/users/{fileName}";
        }

        _context.Users.Update(user);
        var result = await _context.SaveChangesAsync();

        if (result > 0)
        {
            ViewBag.SuccessMessage = "Profil başarıyla güncellendi.";
        }
        else
        {
            ViewBag.ErrorMessage = "Profil güncellenirken bir hata oluştu.";
        }

        var updatedUser = await _context.Users.FirstOrDefaultAsync(u => u.UserId == user.UserId);
        return View("Profile", updatedUser);
    }

    public async Task<IActionResult> EditTask(int taskId)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null)
        {
            return NotFound();
        }

        ViewBag.Users = await _context.Users
            .Select(u => new SelectListItem
            {
                Value = u.UserId.ToString(),
                Text = u.Username
            }).ToListAsync();

        return View(task);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateTask(int taskId, DBTask updatedTask)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null)
        {
            return NotFound();
        }

        bool statusChanged = task.TaskStatus != updatedTask.TaskStatus;

        task.TaskName = updatedTask.TaskName;
        task.TaskDescription = updatedTask.TaskDescription;
        task.TaskStatus = updatedTask.TaskStatus;
        task.Priority = updatedTask.Priority;
        task.DueDate = updatedTask.DueDate;

        _context.Tasks.Update(task);
        var result = await _context.SaveChangesAsync();
        ViewBag.Users = _context.Users.ToList();

        if (result > 0)
        {
            if (statusChanged)
            {
                var taskHistory = new TaskHistory
                {
                    TaskId = taskId,
                    NewStatus = updatedTask.TaskStatus,
                    ChangeDate = DateTime.Now,
                    ChangedByUserId = _context.Users
                        .FirstOrDefault(u => u.Username == HttpContext.Session.GetString("Username"))
                        ?.UserId
                };
                _context.TaskHistories.Add(taskHistory);
                await _context.SaveChangesAsync();
            }

            ViewBag.SuccessMessage = "Görev başarıyla güncellendi.";
        }
        else
        {
            ViewBag.ErrorMessage = "Görev güncellenirken bir hata oluştu.";
        }

        return View("EditTask", task);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteTask(int taskId)
    {
        var task = await _context.Tasks.FindAsync(taskId);
        if (task == null)
        {
            return NotFound();
        }

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return RedirectToAction("Tasks");
    }

    public async Task<IActionResult> UnfinishedTasks()
    {
        var tasks = await _context.Tasks
            .Where(t => t.TaskStatus == "Baslanmadi" &&
                        t.AssignedByUser.Username == HttpContext.Session.GetString("Username"))
            .Include(t => t.AssignedToUser)
            .ToListAsync();
        return View(tasks);
    }


    public async Task<IActionResult> FinishedTasks()
    {
        var tasks = await _context.Tasks
            .Include(t => t.AssignedToUser)
            .Where(t => t.TaskStatus == "Bitti" &&
                        t.AssignedByUser.Username == HttpContext.Session.GetString("Username"))
            .ToListAsync();
        return View(tasks);
    }


    public async Task<IActionResult> AssignedTasks()
    {
        var tasks = await _context.Tasks
            .Where(t => t.AssignedToUser != null &&
                        t.AssignedByUser.Username == HttpContext.Session.GetString("Username"))
            .Include(t => t.AssignedToUser)
            .ToListAsync();
        return View(tasks);
    }

    [HttpPost]
    public async Task<IActionResult> AddTask(DBTask newTask)
    {
        newTask.CreationDate = DateTime.Now;
        newTask.TaskStatus = "Baslanmadi";
        newTask.AssignedByUserId = _context.Users
            .FirstOrDefault(u => u.Username == HttpContext.Session.GetString("Username"))
            ?.UserId;
        _context.Tasks.Add(newTask);
        await _context.SaveChangesAsync();
        return RedirectToAction("Tasks");
    }

    public async Task<IActionResult> Users()
    {
        var users = await _context.Users.ToListAsync();
        return View(users);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return RedirectToAction("Users");
    }

    [HttpPost]
    public async Task<IActionResult> ChangeRole(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound();
        }

        user.Role = user.Role == "Admin" ? "Kullanici" : "Admin";
        _context.Users.Update(user);
        var result = await _context.SaveChangesAsync();
        if (result > 0)
        {
            ViewBag.SuccessMessage = "Rol değişikliği başarıyla gerçekleştirildi.";
        }
        else
        {
            ViewBag.ErrorMessage = "Rol değişikliği sırasında bir hata oluştu.";
        }

        return View("Users", await _context.Users.ToListAsync());
    }

    public async Task<IActionResult> InvitePerson()
    {
        var invitedPersons = await _context.InvitePersonTokens
            .ToListAsync();
        return View(invitedPersons);
    }

    [HttpPost]
    public async Task<IActionResult> InvitePerson(String InviteMail)
    {
        try
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == InviteMail);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty,
                    "Sistemde kayıtlı kullanıcıya tekrar davet gönderemezsiniz.");
                return View(await _context.InvitePersonTokens.ToListAsync()); // Güncel listeyi döndür
            }

            var tokensToDelete = _context.InvitePersonTokens
                .Where(t => t.Email == InviteMail)
                .ToList();

            if (tokensToDelete.Any())
            {
                _context.InvitePersonTokens.RemoveRange(tokensToDelete);
                await _context.SaveChangesAsync();
            }

            var token = Guid.NewGuid();
            string invitationLink = $"http://localhost:5170/invite?token={token}";
            var expiryDate = DateTime.Now.AddDays(1);

            _context.InvitePersonTokens.Add(new InvitePersonToken()
            {
                Email = InviteMail,
                Token = token,
                InvitedByUser = _context.Users
                    .FirstOrDefault(u => u.Username == HttpContext.Session.GetString("Username"))?.FullName,
                ExpiryDate = expiryDate
            });
            await _context.SaveChangesAsync();
            string subject = "Davet: Sizi Platformumuza Katılmaya Davet Ediyoruz!";
            string body = $@"
        <p>Merhaba,</p>
        <p>Sizi platformumuza katılmaya davet ediyoruz! Aşağıdaki bağlantıya tıklayarak davetinizi kabul edebilir ve hesabınızı oluşturabilirsiniz:</p>
        <p><a href='{invitationLink}'>Davetiyeyi Kabul Et</a></p>
        <p>Eğer bağlantı çalışmıyorsa, aşağıdaki URL'yi tarayıcınıza yapıştırarak hesabınızı oluşturabilirsiniz:</p>
        <p>{invitationLink}</p>
        <p>Teşekkürler,</p>
    ";
            var mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("mail@halilsertkaya.net", "Görev Takip App");
            mailMessage.To.Add(InviteMail);
            mailMessage.Subject = subject;
            mailMessage.Body = body;
            mailMessage.IsBodyHtml = true;

            using (var smtpClient = new SmtpClient("mail.halilsertkaya.net"))
            {
                smtpClient.Port = 587;
                smtpClient.Credentials = new NetworkCredential("mail@halilsertkaya.net", "Halil.mail27");
                smtpClient.EnableSsl = true;
                await smtpClient.SendMailAsync(mailMessage);
            }

            ViewBag.Success = "Davet başarıyla gönderildi.";
        }
        catch (SmtpException ex)
        {
            ModelState.AddModelError(string.Empty,
                "Üye Davet edilirken hata oluştu. Lütfen daha sonra tekrar deneyin." + ex);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty,
                "E-posta gönderilirken bir hata oluştu. Lütfen daha sonra tekrar deneyin." + ex);
        }

        return View(await _context.InvitePersonTokens.ToListAsync()); // Güncel listeyi döndür
    }

    [HttpPost]
    public async Task<IActionResult> DeleteInvite(int inviteId)
    {
        Console.WriteLine("Silme işlemi başladı" + inviteId);
        var invite = await _context.InvitePersonTokens.FindAsync(inviteId);
        if (invite == null)
        {
            return NotFound();
        }

        _context.InvitePersonTokens.Remove(invite);
        await _context.SaveChangesAsync();
        return View("InvitePerson", await _context.InvitePersonTokens.ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> AddInviteTime(int inviteId)
    {
        var invite = await _context.InvitePersonTokens.FindAsync(inviteId);
        if (invite == null)
        {
            return NotFound();
        }

        invite.ExpiryDate = invite.ExpiryDate.AddDays(1);
        _context.InvitePersonTokens.Update(invite);
        await _context.SaveChangesAsync();
        return View("InvitePerson", await _context.InvitePersonTokens.ToListAsync());
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}