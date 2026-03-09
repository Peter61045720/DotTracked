using DotTracked.Shared.Enums;

namespace DotTracked.Shared.DTOs;

public class AbsenceDto
{
    public Guid Id { get; set; }
    public AbsenceType Type { get; set; }
    public string Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}