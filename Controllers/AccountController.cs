using System.Net;
using System.Net.Mail;
using GorevTakipProgrami.Data;
using GorevTakipProgrami.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

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

        HttpContext.Session.SetString("Username", User.Username);
        HttpContext.Session.SetString("Role", User.Role);
        
        switch (HttpContext.Session.GetString("Role"))
        {
            case "User":
                return RedirectToAction("Index", "Home");
            case "Admin":
                ModelState.AddModelError(string.Empty, "Admin sayfası henüz yok.");
                return View();
            default:
                ModelState.AddModelError(string.Empty, "Geçersiz yetki.");
                return RedirectToAction("Login");
        }
    }

    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        // Kullanıcıyı e-posta adresi ile bul
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Bu maile ait bir hesap bulunamadı.");
            return View();
        }

        var tokensToDelete = _context.PasswordResetTokens
            .Where(t => t.Email == email)
            .ToList();

        if (tokensToDelete.Any())
        { 
            _context.PasswordResetTokens.RemoveRange(tokensToDelete);
            await _context.SaveChangesAsync();
        }

        // Şifre sıfırlama token'ı oluştur ve kaydet
        var token = Guid.NewGuid();
        var expiryDate = DateTime.Now.AddHours(1);

        _context.PasswordResetTokens.Add(new PasswordResetToken
        {
            Email = email,
            Token = token,
            ExpiryDate = expiryDate
        });

        await _context.SaveChangesAsync();

        try
        {
            // Şifre sıfırlama bağlantısı oluştur
            var resetLink = Url.Action("ResetPassword", "Account", new { token = token }, Request.Scheme);
            var mailMessage = new MailMessage("mail@halilsertkaya.net", "Görev Takip")
            {
                Subject = "Şifre Sıfırlama Talebi | Görev Takip",
                Body =
                    $"Merhaba {user.FullName}, <br>Şifrenizi sıfırlamak için lütfen şu bağlantıya tıklayın: <a href=\"{resetLink}\"><b>Şifreyi Sıfırla</b></a>",
                IsBodyHtml = true
            };

            using (var smtpClient = new SmtpClient("mail.halilsertkaya.net"))
            {
                Console.WriteLine("mail göndermeye çalışıyor.");
                smtpClient.Port = 587;
                smtpClient.Credentials = new NetworkCredential("mail@halilsertkaya.net", "Halil.mail27");
                smtpClient.EnableSsl = true;
                smtpClient.Send(mailMessage);
            }

            ViewBag.SuccessMessage = "Sıfırlama linki başarıyla gönderildi.";
        }
        catch (SmtpException ex)
        {
            ModelState.AddModelError(string.Empty,
                "E-posta gönderilirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Bir hata oluştu. Lütfen daha sonra tekrar deneyin.");
        }

        return View();
    }

    public IActionResult ResetPassword(Guid token)
    {
        var resetToken = _context.PasswordResetTokens
            .FirstOrDefault(t => t.Token == token && t.ExpiryDate > DateTime.Now && !t.IsUsed);

        if (resetToken == null)
        {
            return View("ResetPasswordFailed");
        }

        return View(new PasswordResetViewModel { Token = token });
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(PasswordResetViewModel model)
    {
        if (!ModelState.IsValid || model.NewPassword != model.ConfirmPassword)
        {
            ModelState.AddModelError(string.Empty, "Şifreler uyuşmuyor.");
            return View(model);
        }

        var resetToken = await _context.PasswordResetTokens
            .FirstOrDefaultAsync(t => t.Token == model.Token && t.ExpiryDate > DateTime.Now && !t.IsUsed);

        if (resetToken == null)
        {
            return View("ResetPasswordFailed");
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == resetToken.Email);
        if (user == null)
        {
            return View("ResetPasswordFailed");
        }

        user.PasswordHash = model.NewPassword;
        resetToken.IsUsed = true;
        _context.PasswordResetTokens.Update(resetToken);
        await _context.SaveChangesAsync(); 
        
        ViewBag.SuccessMessage = "Şifreniz başarıyla sıfırlandı.";
        ViewBag.IsSuccess = true;
        return View(model);
    }
}