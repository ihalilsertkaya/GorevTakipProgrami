namespace GorevTakipProgrami.Models;

public class PasswordResetViewModel
{
    public Guid? Token { get; set; } // Token, şifre sıfırlama için
    public string NewPassword { get; set; } // Yeni şifre
    public string ConfirmPassword { get; set; } // Yeni şifreyi onaylama
}
