using System;
using System.Collections.Generic;

namespace GorevTakipProgrami.Models;

public partial class TaskNotification
{
    public int NotificationId { get; set; }

    public int? UserId { get; set; }

    public int? TaskId { get; set; }

    public string? NotificationText { get; set; }

    public bool? IsRead { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual DBTask? Task { get; set; }

    public virtual User? User { get; set; }
}
