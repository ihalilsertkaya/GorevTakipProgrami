namespace GorevTakipProgrami.Models;

public class InvitePersonToken
{
    public int Id { get; set; }
    public string Email { get; set; }
    public Guid Token { get; set; }
    public DateTime ExpiryDate { get; set; }
    public bool IsUsed { get; set; }
}