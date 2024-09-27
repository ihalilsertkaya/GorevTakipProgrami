using System;
using System.Collections.Generic;

namespace GorevTakipProgrami.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public int? UserId { get; set; }

    public int? TaskId { get; set; }

    public string CommentText { get; set; } = null!;

    public DateTime? CommentDate { get; set; }

    public virtual DBTask? Task { get; set; }

    public virtual User? User { get; set; }
}

