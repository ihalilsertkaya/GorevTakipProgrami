using System;
using System.Collections.Generic;

namespace GorevTakipProgrami.Models;

public partial class Notification
{
    public int NotificationId { get; set; }

    public int? UserId { get; set; }

    public int? TaskId { get; set; }

    public string NotificationContent { get; set; } = null!;

    public bool? IsRead { get; set; }

    public DateTime? NotificationDate { get; set; }

    public virtual DBTask? Task { get; set; }

    public virtual User? User { get; set; }
}
