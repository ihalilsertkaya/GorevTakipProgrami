using System;
using System.Collections.Generic;

namespace GorevTakipProgrami.Models;

public partial class InvitePersonToken
{
    public int Id { get; set; }

    public string Email { get; set; } = null!;

    public Guid Token { get; set; }

    public DateTime ExpiryDate { get; set; }

    public bool IsUsed { get; set; }
    
    public String InvitedByUser{ get; set; }
}


