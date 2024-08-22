using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using GorevTakipProgrami.Data;
using Microsoft.AspNetCore.Mvc;
using GorevTakipProgrami.Models;

namespace GorevTakipProgrami.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _context;

    public HomeController(ILogger<HomeController> logger, AppDbContext context)
    {
        _context = context;
        _logger = logger;
    }
    
    public IActionResult Index()
    {
        var userName = HttpContext.Session.GetString("Username");
        if (string.IsNullOrEmpty(userName))
        {
            return RedirectToAction("Login", "Account");
        }
        var loggedUser = _context.Users.Where(u => u.Username == userName).FirstOrDefault();
        ViewBag.UserFullName = loggedUser.FullName;
        ViewBag.UserPhoto = loggedUser.UserPhoto;
        return View();
    }

    public IActionResult InvitePerson()
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("Username")))
        {
            return RedirectToAction("Login", "Account");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> InvitePerson(String InviteMail)
    {
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
        <p>Görev Takip Ekibi</p>
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
        return View();
    }
    
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}