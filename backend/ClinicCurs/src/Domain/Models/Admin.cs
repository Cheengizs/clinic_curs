namespace Domain.Models;

public class Admin
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public DateTime CreatedAt { get; set; }

    // Навигационное свойство
    public virtual Account Account { get; set; } = null!;
}
