using System;
using System.Collections.Generic;

namespace GorevTakipProgrami.Models;

public partial class UserTask
{
    public int UserTaskId { get; set; }

    public int? UserId { get; set; }

    public int? TaskId { get; set; }

    public DateTime? AssignedAt { get; set; }

    public virtual DBTask? Task { get; set; }

    public virtual User? User { get; set; }
}
