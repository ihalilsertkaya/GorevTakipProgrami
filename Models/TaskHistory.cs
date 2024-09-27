using System.ComponentModel.DataAnnotations;
namespace GorevTakipProgrami.Models;

public partial class TaskHistory
{
    [Key]
    public int TaskHistoryId { get; set; }
    public int? TaskId { get; set; }
    public string? NewStatus { get; set; }
    public DateTime? ChangeDate { get; set; }
    public int? ChangedByUserId { get; set; }
    public virtual User? ChangedByUser { get; set; }
    public virtual DBTask? Task { get; set; }
}


