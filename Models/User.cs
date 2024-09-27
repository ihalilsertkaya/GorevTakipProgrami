using System;
using System.Collections.Generic;

namespace GorevTakipProgrami.Models;

public partial class User
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? ProfilePhoto { get; set; }

    public string? Role { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<DBTask> TaskAssignedByUsers { get; set; } = new List<DBTask>();

    public virtual ICollection<DBTask> TaskAssignedToUsers { get; set; } = new List<DBTask>();
}
