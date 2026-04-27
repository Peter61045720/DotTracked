namespace DotTracked.Shared.DTOs;

public class CommentDto
{
    public Guid Id { get; set; }
    public string Content { get; set; }
    public string CreatorEmail { get; set; }
    public string AvatarText { get; set; }
    public bool IsOwner { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}