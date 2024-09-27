using System;
using System.Collections.Generic;

namespace GorevTakipProgrami.Models;

public partial class DBTask
{
    public int TaskId { get; set; }

    public string TaskName { get; set; } = null!;

    public string? TaskDescription { get; set; }

    public string? TaskStatus { get; set; }

    public string? Priority { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? CreationDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public int? AssignedToUserId { get; set; }

    public int? AssignedByUserId { get; set; }

    public virtual User? AssignedByUser { get; set; }

    public virtual User? AssignedToUser { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}
