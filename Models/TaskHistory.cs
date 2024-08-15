using System;
using System.Collections.Generic;

namespace GorevTakipProgrami.Models;

public partial class TaskHistory
{
    public int HistoryId { get; set; }

    public int? TaskId { get; set; }

    public string? OldStatus { get; set; }

    public string? NewStatus { get; set; }

    public DateTime? ChangedAt { get; set; }

    public int? ChangedBy { get; set; }

    public virtual User? ChangedByNavigation { get; set; }

    public virtual DBTask? Task { get; set; }
}
