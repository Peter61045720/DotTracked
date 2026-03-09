using DotTracked.Shared.Enums;

namespace DotTracked.Data.Models;

public class Absence
{
    public Guid Id { get; set; }
    public AbsenceType Type { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public string UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
}